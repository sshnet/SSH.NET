#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Exposes a <see cref="Stream"/> around a remote SFTP file, supporting
    /// both synchronous and asynchronous read and write operations.
    /// </summary>
    public sealed partial class SftpFileStream : Stream
    {
        private const int MaxPendingReads = 100;

        private readonly ISftpSession _session;
        private readonly FileAccess _access;
        private readonly bool _canSeek;
        private readonly int _readBufferSize;

        private SftpFileReader? _sftpFileReader;
        private ReadOnlyMemory<byte> _readBuffer;
        private System.Net.ArrayBuffer _writeBuffer;

        private long _position;
        private TimeSpan _timeout;
        private bool _disposed;

        /// <inheritdoc/>
        public override bool CanRead
        {
            get { return !_disposed && (_access & FileAccess.Read) == FileAccess.Read; }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get { return !_disposed && _canSeek; }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get { return !_disposed && (_access & FileAccess.Write) == FileAccess.Write; }
        }

        /// <summary>
        /// Gets a value indicating whether timeout properties are usable for <see cref="SftpFileStream"/>.
        /// </summary>
        /// <value>
        /// <see langword="false"/> in all cases.
        /// </value>
        public override bool CanTimeout
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                ThrowIfNotSeekable();

                Flush();

                var size = _session.RequestFStat(Handle).Size;

                Debug.Assert(size >= 0, "fstat should return size as checked in ctor");

                return size;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                ThrowIfNotSeekable();

                return _position;
            }
            set
            {
                _ = Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Gets the name of the path that was used to construct the current <see cref="SftpFileStream"/>.
        /// </summary>
        /// <value>
        /// The name of the path that was used to construct the current <see cref="SftpFileStream"/>.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the operating system file handle for the file that the current <see cref="SftpFileStream"/> encapsulates.
        /// </summary>
        /// <value>
        /// The operating system file handle for the file that the current <see cref="SftpFileStream"/> encapsulates.
        /// </value>
        public byte[] Handle { get; }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)] // Unused
        public TimeSpan Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                value.EnsureValidTimeout(nameof(Timeout));

                _timeout = value;
            }
        }

        private SftpFileStream(
            ISftpSession session,
            string path,
            FileAccess access,
            bool canSeek,
            int readBufferSize,
            int writeBufferSize,
            byte[] handle,
            long position,
            SftpFileReader? initialReader)
        {
            Timeout = TimeSpan.FromSeconds(30);
            Name = path;

            _session = session;
            _access = access;
            _canSeek = canSeek;

            Handle = handle;
            _readBufferSize = readBufferSize;
            _position = position;
            _writeBuffer = new System.Net.ArrayBuffer(writeBufferSize);
            _sftpFileReader = initialReader;
        }

        internal static SftpFileStream Open(
            ISftpSession? session,
            string path,
            FileMode mode,
            FileAccess access,
            int bufferSize,
            bool isDownloadFile = false)
        {
            return Open(session, path, mode, access, bufferSize, isDownloadFile, isAsync: false, CancellationToken.None).GetAwaiter().GetResult();
        }

        internal static Task<SftpFileStream> OpenAsync(
            ISftpSession? session,
            string path,
            FileMode mode,
            FileAccess access,
            int bufferSize,
            CancellationToken cancellationToken,
            bool isDownloadFile = false)
        {
            return Open(session, path, mode, access, bufferSize, isDownloadFile, isAsync: true, cancellationToken);
        }

        private static async Task<SftpFileStream> Open(
            ISftpSession? session,
            string path,
            FileMode mode,
            FileAccess access,
            int bufferSize,
            bool isDownloadFile,
            bool isAsync,
            CancellationToken cancellationToken)
        {
            Debug.Assert(isAsync || cancellationToken == default);

            ThrowHelper.ThrowIfNull(path);

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Cannot be less than or equal to zero.");
            }

            if (session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            var flags = access switch
            {
                FileAccess.Read => Flags.Read,
                FileAccess.Write => Flags.Write,
                FileAccess.ReadWrite => Flags.Read | Flags.Write,
                _ => throw new ArgumentOutOfRangeException(nameof(access))
            };

            if (mode == FileMode.Append && access != FileAccess.Write)
            {
                throw new ArgumentException(
                    "Append mode can be requested only with write-only access.",
                    nameof(access));
            }

            if (access == FileAccess.Read &&
                mode is FileMode.Create or FileMode.CreateNew or FileMode.Truncate or FileMode.Append)
            {
                throw new ArgumentException(
                    $"Combining {nameof(FileMode)}: {mode} with {nameof(FileAccess)}: {access} is invalid.",
                    nameof(access));
            }

            switch (mode)
            {
                case FileMode.Append:
                    flags |= Flags.Append | Flags.CreateNewOrOpen;
                    break;
                case FileMode.Create:
                    flags |= Flags.CreateNewOrOpen | Flags.Truncate;
                    break;
                case FileMode.CreateNew:
                    flags |= Flags.CreateNew;
                    break;
                case FileMode.Open:
                    break;
                case FileMode.OpenOrCreate:
                    flags |= Flags.CreateNewOrOpen;
                    break;
                case FileMode.Truncate:
                    flags |= Flags.Truncate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }

            byte[] handle;

            if (isAsync)
            {
                handle = await session.RequestOpenAsync(path, flags, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                handle = session.RequestOpen(path, flags);
            }

            /*
             * Instead of using the specified buffer size as is, we use it to calculate a buffer size
             * that ensures we always receive or send the max. number of bytes in a single SSH_FXP_READ
             * or SSH_FXP_WRITE message.
             */

            var readBufferSize = (int)session.CalculateOptimalReadLength((uint)bufferSize);
            var writeBufferSize = (int)session.CalculateOptimalWriteLength((uint)bufferSize, handle);

            SftpFileAttributes? attributes;

            try
            {
                if (isAsync)
                {
                    attributes = await session.RequestFStatAsync(handle, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    attributes = session.RequestFStat(handle);
                }
            }
            catch (SftpException ex)
            {
                session.SessionLoggerFactory.CreateLogger<SftpFileStream>().LogInformation(
                    ex, "fstat failed after opening {Path}. Will set CanSeek=false.", path);

                attributes = null;
            }

            bool canSeek;
            long position = 0;
            SftpFileReader? initialReader = null;

            if (attributes?.Size >= 0)
            {
                canSeek = true;

                if (mode == FileMode.Append)
                {
                    position = attributes.Size;
                }
                else if (isDownloadFile)
                {
                    // If we are in a call to SftpClient.DownloadFile, then we know that we will read the whole file,
                    // so we can let there be several in-flight requests from the get go.
                    // This optimisation is mostly only beneficial to smaller files on higher latency connections.
                    // The +2 is +1 for rounding up to cover the whole file, and +1 for the final request to receive EOF.
                    var initialPendingReads = (int)Math.Max(1, Math.Min(MaxPendingReads, 2 + (attributes.Size / readBufferSize)));

                    initialReader = new(handle, session, readBufferSize, position, MaxPendingReads, (ulong)attributes.Size, initialPendingReads);
                }
                else if ((access & FileAccess.Read) == FileAccess.Read)
                {
                    // The reader can use the size information to reduce in-flight requests near the expected EOF,
                    // so pass it in here.
                    initialReader = new(handle, session, readBufferSize, position, MaxPendingReads, (ulong)attributes.Size);
                }
            }
            else
            {
                // Either fstat is failing or it doesn't return the size, in which case we can't support Length,
                // so CanSeek must return false.
                canSeek = false;
            }

            return new SftpFileStream(session, path, access, canSeek, readBufferSize, writeBufferSize, handle, position, initialReader);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

            var writeLength = _writeBuffer.ActiveLength;

            if (writeLength == 0)
            {
                return;
            }

            // Under normal usage the offset will be nonnegative, but we nevertheless
            // perform a checked conversion to prevent writing to a very large offset
            // in case of corruption due to e.g. invalid multithreaded usage.
            var serverOffset = checked((ulong)(_position - writeLength));

            using (var wait = new AutoResetEvent(initialState: false))
            {
                _session.RequestWrite(
                    Handle,
                    serverOffset,
                    _writeBuffer.DangerousGetUnderlyingBuffer(),
                    _writeBuffer.ActiveStartOffset,
                    writeLength,
                    wait);

                _writeBuffer.Discard(writeLength);
            }
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

            var writeLength = _writeBuffer.ActiveLength;

            if (writeLength == 0)
            {
                return;
            }

            // Under normal usage the offset will be nonnegative, but we nevertheless
            // perform a checked conversion to prevent writing to a very large offset
            // in case of corruption due to e.g. invalid multithreaded usage.
            var serverOffset = checked((ulong)(_position - writeLength));

            await _session.RequestWriteAsync(
                Handle,
                serverOffset,
                _writeBuffer.DangerousGetUnderlyingBuffer(),
                _writeBuffer.ActiveStartOffset,
                writeLength,
                cancellationToken).ConfigureAwait(false);

            _writeBuffer.Discard(writeLength);
        }

        private void InvalidateReads()
        {
            _readBuffer = ReadOnlyMemory<byte>.Empty;
            _sftpFileReader?.Dispose();
            _sftpFileReader = null;
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
            ThrowIfNotReadable();

            if (_readBuffer.IsEmpty)
            {
                if (_sftpFileReader is null)
                {
                    Flush();
                    _sftpFileReader = new(Handle, _session, _readBufferSize, _position, MaxPendingReads);
                }

                _readBuffer = _sftpFileReader.ReadAsync(CancellationToken.None).GetAwaiter().GetResult();

                if (_readBuffer.IsEmpty)
                {
                    // If we've hit EOF then throw away this reader instance.
                    // If Read is called again we will create a new reader.
                    // This takes care of the case when a file is expanding
                    // during reading.
                    _sftpFileReader.Dispose();
                    _sftpFileReader = null;
                }
            }

            Debug.Assert(_writeBuffer.ActiveLength == 0, "Write buffer should be empty when reading.");

            var bytesRead = Math.Min(buffer.Length, _readBuffer.Length);

            _readBuffer.Span.Slice(0, bytesRead).CopyTo(buffer);
            _readBuffer = _readBuffer.Slice(bytesRead);

            _position += bytesRead;

            return bytesRead;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
#if !NET
            ThrowHelper.
#endif
            ValidateBufferArguments(buffer, offset, count);

            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

#if NET
        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
#else
        private async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
#endif
        {
            ThrowIfNotReadable();

            if (_readBuffer.IsEmpty)
            {
                if (_sftpFileReader is null)
                {
                    await FlushAsync(cancellationToken).ConfigureAwait(false);

                    _sftpFileReader = new(Handle, _session, _readBufferSize, _position, MaxPendingReads);
                }

                _readBuffer = await _sftpFileReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                if (_readBuffer.IsEmpty)
                {
                    // If we've hit EOF then throw away this reader instance.
                    // If Read is called again we will create a new reader.
                    // This takes care of the case when a file is expanding
                    // during reading.
                    _sftpFileReader.Dispose();
                    _sftpFileReader = null;
                }
            }

            Debug.Assert(_writeBuffer.ActiveLength == 0, "Write buffer should be empty when reading.");

            var bytesRead = Math.Min(buffer.Length, _readBuffer.Length);

            _readBuffer.Slice(0, bytesRead).CopyTo(buffer);
            _readBuffer = _readBuffer.Slice(bytesRead);

            _position += bytesRead;

            return bytesRead;
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
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return TaskToAsyncResult.Begin(ReadAsync(buffer, offset, count), callback, state);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return TaskToAsyncResult.End<int>(asyncResult);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
#if !NET
            ThrowHelper.
#endif
            ValidateBufferArguments(buffer, offset, count);

            Write(buffer.AsSpan(offset, count));
        }

#if NET
        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
#else
        private void Write(ReadOnlySpan<byte> buffer)
#endif
        {
            ThrowIfNotWriteable();

            InvalidateReads();

            while (!buffer.IsEmpty)
            {
                var byteCount = Math.Min(buffer.Length, _writeBuffer.AvailableLength);

                buffer.Slice(0, byteCount).CopyTo(_writeBuffer.AvailableSpan);

                buffer = buffer.Slice(byteCount);

                _writeBuffer.Commit(byteCount);

                _position += byteCount;

                if (_writeBuffer.AvailableLength == 0)
                {
                    Flush();
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            Write([value]);
        }

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
            ThrowIfNotWriteable();

            InvalidateReads();

            while (!buffer.IsEmpty)
            {
                var byteCount = Math.Min(buffer.Length, _writeBuffer.AvailableLength);

                buffer.Slice(0, byteCount).CopyTo(_writeBuffer.AvailableMemory);

                buffer = buffer.Slice(byteCount);

                _writeBuffer.Commit(byteCount);

                _position += byteCount;

                if (_writeBuffer.AvailableLength == 0)
                {
                    await FlushAsync(cancellationToken).ConfigureAwait(false);
                }
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

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfNotSeekable();

            Flush();

            var newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _session.RequestFStat(Handle).Size + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };

            if (newPosition < 0)
            {
                throw new IOException("An attempt was made to move the position before the beginning of the stream.");
            }

            var readBufferStart = _position; // inclusive
            var readBufferEnd = _position + _readBuffer.Length; // exclusive

            if (readBufferStart <= newPosition && newPosition <= readBufferEnd)
            {
                _readBuffer = _readBuffer.Slice((int)(newPosition - readBufferStart));
            }
            else
            {
                InvalidateReads();
            }

            return _position = newPosition;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            ThrowHelper.ThrowIfNegative(value);
            ThrowIfNotWriteable();
            ThrowIfNotSeekable();

            Flush();
            InvalidateReads();

            var attributes = _session.RequestFStat(Handle);
            attributes.Size = value;
            _session.RequestFSetStat(Handle, attributes);

            if (_position > value)
            {
                _position = value;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (disposing && _session.IsOpen)
                {
                    try
                    {
                        Flush();
                    }
                    finally
                    {
                        if (_session.IsOpen)
                        {
                            _session.RequestClose(Handle);
                        }
                    }
                }
            }
            finally
            {
                _disposed = true;
                InvalidateReads();
                base.Dispose(disposing);
            }
        }

#if NET
        /// <inheritdoc/>
#pragma warning disable CA2215 // Dispose methods should call base class dispose
        public override async ValueTask DisposeAsync()
#pragma warning restore CA2215 // Dispose methods should call base class dispose
#else
        internal async ValueTask DisposeAsync()
#endif
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (_session.IsOpen)
                {
                    try
                    {
                        await FlushAsync().ConfigureAwait(false);
                    }
                    finally
                    {
                        if (_session.IsOpen)
                        {
                            await _session.RequestCloseAsync(Handle, CancellationToken.None).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                _disposed = true;
                InvalidateReads();
                base.Dispose(disposing: false);
            }
        }

        private void ThrowIfNotSeekable()
        {
            if (!CanSeek)
            {
                ThrowHelper.ThrowObjectDisposedIf(_disposed, this);
                Throw();
            }

            static void Throw()
            {
                throw new NotSupportedException("Stream does not support seeking.");
            }
        }

        private void ThrowIfNotWriteable()
        {
            if (!CanWrite)
            {
                ThrowHelper.ThrowObjectDisposedIf(_disposed, this);
                Throw();
            }

            static void Throw()
            {
                throw new NotSupportedException("Stream does not support writing.");
            }
        }

        private void ThrowIfNotReadable()
        {
            if (!CanRead)
            {
                ThrowHelper.ThrowObjectDisposedIf(_disposed, this);
                Throw();
            }

            static void Throw()
            {
                throw new NotSupportedException("Stream does not support reading.");
            }
        }
    }
}
