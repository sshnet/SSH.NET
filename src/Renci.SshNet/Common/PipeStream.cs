#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// PipeStream is a thread-safe read/write data stream for use between two threads in a
    /// single-producer/single-consumer type problem.
    /// </summary>
    public class PipeStream : Stream
    {
        private readonly object _sync = new object();

        private byte[] _buffer = new byte[1024];
        private int _head; // The index from which the data starts in _buffer.
        private int _tail; // The index at which to add new data into _buffer.
        private bool _disposed;

#pragma warning disable MA0076 // Do not use implicit culture-sensitive ToString in interpolated strings
        [Conditional("DEBUG")]
        private void AssertValid()
        {
            Debug.Assert(Monitor.IsEntered(_sync), $"Should be in lock on {nameof(_sync)}");
            Debug.Assert(_head >= 0, $"{nameof(_head)} should be non-negative but is {_head}");
            Debug.Assert(_tail >= 0, $"{nameof(_tail)} should be non-negative but is {_tail}");
            Debug.Assert(_head <= _buffer.Length, $"{nameof(_head)} should be <= {nameof(_buffer)}.Length but is {_head}");
            Debug.Assert(_tail <= _buffer.Length, $"{nameof(_tail)} should be <= {nameof(_buffer)}.Length but is {_tail}");
            Debug.Assert(_head <= _tail, $"Should have {nameof(_head)} <= {nameof(_tail)} but have {_head} <= {_tail}");
        }
#pragma warning restore MA0076 // Do not use implicit culture-sensitive ToString in interpolated strings

        /// <summary>
        /// This method does nothing.
        /// </summary>
        public override void Flush()
        {
        }

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
            lock (_sync)
            {
                while (_head == _tail && !_disposed)
                {
                    _ = Monitor.Wait(_sync);
                }

                AssertValid();

                var bytesRead = Math.Min(count, _tail - _head);

                Buffer.BlockCopy(_buffer, _head, buffer, offset, bytesRead);

                _head += bytesRead;

                AssertValid();

                return bytesRead;
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_sync)
            {
                ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

                AssertValid();

                // Ensure sufficient buffer space and copy the new data in.

                if (_buffer.Length - _tail >= count)
                {
                    // If there is enough space after _tail for the new data,
                    // then copy the data there.
                    Buffer.BlockCopy(buffer, offset, _buffer, _tail, count);
                    _tail += count;
                }
                else
                {
                    // We can't fit the new data after _tail.

                    var newLength = _tail - _head + count;

                    if (newLength <= _buffer.Length)
                    {
                        // If there is sufficient space at the start of the buffer,
                        // then move the current data to the start of the buffer.
                        Buffer.BlockCopy(_buffer, _head, _buffer, 0, _tail - _head);
                    }
                    else
                    {
                        // Otherwise, we're gonna need a bigger buffer.
                        var newBuffer = new byte[Math.Max(newLength, _buffer.Length * 2)];
                        Buffer.BlockCopy(_buffer, _head, newBuffer, 0, _tail - _head);
                        _buffer = newBuffer;
                    }

                    // Copy the new data into the freed-up space.
                    Buffer.BlockCopy(buffer, offset, _buffer, _tail - _head, count);

                    _head = 0;
                    _tail = newLength;
                }

                AssertValid();

                Monitor.PulseAll(_sync);
            }
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
                    AssertValid();
                    return _tail - _head;
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
