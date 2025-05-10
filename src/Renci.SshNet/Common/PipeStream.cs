#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// PipeStream is a thread-safe read/write data stream for use between two threads in a
    /// single-producer/single-consumer type problem.
    /// </summary>
    public class PipeStream : Stream
    {
        private readonly object _sync = new object();

        private System.Net.ArrayBuffer _buffer = new(1024);
        private bool _disposed;

        /// <summary>
        /// This method always throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>Never.</returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
#if !NET
            ThrowHelper.
#endif
            ValidateBufferArguments(buffer, offset, count);

            return Read(buffer.AsSpan(offset, count));
        }

#if NET
        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
#else
        private int Read(Span<byte> buffer)
#endif
        {
            lock (_sync)
            {
                while (_buffer.ActiveLength == 0 && !_disposed)
                {
                    _ = Monitor.Wait(_sync);
                }

                var bytesRead = Math.Min(buffer.Length, _buffer.ActiveLength);

                _buffer.ActiveReadOnlySpan.Slice(0, bytesRead).CopyTo(buffer);

                _buffer.Discard(bytesRead);

                return bytesRead;
            }
        }

#if NET
        /// <inheritdoc/>
        public override int ReadByte()
        {
            byte b = default;
            var read = Read(new Span<byte>(ref b));
            return read == 0 ? -1 : b;
        }
#endif

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
#if !NET
            ThrowHelper.
#endif
            ValidateBufferArguments(buffer, offset, count);

            lock (_sync)
            {
                WriteCore(buffer.AsSpan(offset, count));
            }
        }

#if NET
        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            lock (_sync)
            {
                WriteCore(buffer);
            }
        }
#endif

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            lock (_sync)
            {
                WriteCore([value]);
            }
        }

        private void WriteCore(ReadOnlySpan<byte> buffer)
        {
            Debug.Assert(Monitor.IsEntered(_sync));

            ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

            _buffer.EnsureAvailableSpace(buffer.Length);

            buffer.CopyTo(_buffer.AvailableSpan);

            _buffer.Commit(buffer.Length);

            Monitor.PulseAll(_sync);
        }

        // We provide overrides for async Write methods but not async Read.
        // The default implementations from the base class effectively call the
        // sync methods on a threadpool thread, but only allowing one async
        // operation at a time (for protecting thread-unsafe implementations).
        // This constraint is desirable for reads because if there were multiple
        // readers and no data coming in, our current Monitor.Wait implementation
        // would just block as many threadpool threads as there are readers.
        // But since a write is just short-lived buffer copying and can unblock
        // readers, it is beneficial to circumvent the one-at-a-time constraint,
        // as otherwise a waiting async read will block the async write that could
        // unblock it.

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
#if !NET
            ThrowHelper.
#endif
            ValidateBufferArguments(buffer, offset, count);

            return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

#if NET
        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
#else
        private async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
#endif
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Monitor.TryEnter(_sync))
            {
                // If we cannot immediately enter the lock and complete the write
                // synchronously, then go async and wait for it there.
                // This is not great! But since there is very little work being
                // done under the lock, this should be a rare case and we should
                // not be blocking threads for long.

                await Task.Yield();

                Monitor.Enter(_sync);
            }

            try
            {
                WriteCore(buffer.Span);
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return TaskToAsyncResult.Begin(WriteAsync(buffer, offset, count), callback, state);
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            TaskToAsyncResult.End(asyncResult);
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="cancellationToken">Unobserved cancellation token.</param>
        /// <returns><see cref="Task.CompletedTask"/>.</returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                base.Dispose(disposing);
                return;
            }

            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                Monitor.PulseAll(_sync);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value>
        /// <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// It is safe to read from <see cref="PipeStream"/> even after disposal.
        /// </remarks>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value>
        /// <see langword="false"/>.
        /// </value>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this stream has not been disposed and the underlying channel
        /// is still open, otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// A value of <see langword="true"/> does not necessarily mean a write will succeed. It is possible
        /// that the stream is disposed by another thread between a call to <see cref="CanWrite"/> and the call to write.
        /// </remarks>
        public override bool CanWrite
        {
            get { return !_disposed; }
        }

        /// <summary>
        /// Gets the number of bytes currently available for reading.
        /// </summary>
        /// <value>A long value representing the length of the stream in bytes.</value>
        public override long Length
        {
            get
            {
                lock (_sync)
                {
                    return _buffer.ActiveLength;
                }
            }
        }

        /// <summary>
        /// This property always returns 0, and throws <see cref="NotSupportedException"/>
        /// when calling the setter.
        /// </summary>
        /// <value>
        /// 0.
        /// </value>
        /// <exception cref="NotSupportedException">The setter is called.</exception>
#pragma warning disable SA1623 // The property's documentation should begin with 'Gets or sets'
        public override long Position
#pragma warning restore SA1623 // The property's documentation should begin with 'Gets or sets'
        {
            get { return 0; }
            set { throw new NotSupportedException(); }
        }
    }
}
