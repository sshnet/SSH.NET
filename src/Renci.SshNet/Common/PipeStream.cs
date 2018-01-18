using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides a producer/consumer ring-buffered memory stream. The methods Read() and Write() are
    /// thread-safe for use by multiple readers and writers.
    /// </summary>
    /// <remarks>
    /// The read lock and the write lock can be removed for a small performance gain when used in a
    /// single-producer/single-consumer scenario.
    /// </remarks>
    public class PipeStream : Stream
    {
        private const int DefaultRingBufferSize = 1024 * 1024;

        private readonly AutoResetEvent _bufferIsNotEmpty = new AutoResetEvent(false);
        private readonly AutoResetEvent _bufferIsNotFull = new AutoResetEvent(true);
        private readonly object _commonLock = new object();
        private readonly object _readLock = new object();
        private readonly byte[] _ringBuffer;
        private readonly int _ringBufferSize;
        private readonly object _writeLock = new object();
        private bool _isDisposed;
        private bool _isFlushed;
        private long _readCount;
        private int _readOffset;
        private long _writeCount;
        private int _writeOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeStream"/> class.
        /// </summary>
        public PipeStream()
            : this(DefaultRingBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeStream"/> class.
        /// </summary>
        /// <param name="ringBufferSize">The size in bytes of the ring buffer.</param>
        public PipeStream(int ringBufferSize)
        {
            _ringBufferSize = ringBufferSize;
            _ringBuffer = new byte[ringBufferSize];
        }

        /// <summary>
        /// Gets the length in bytes of the ring buffer.
        /// </summary>
        public int BufferLength
        {
            get { return _ringBufferSize; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return !_isDisposed; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking. Always returns <c>false</c>.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return !_isDisposed; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (_isDisposed)
                {
                    throw CreateObjectDisposedException();
                }

                lock (_commonLock)
                {
                    return _writeCount - _readCount;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get { return 0; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Once flushed, any subsequent read operations no longer block until requested bytes are
        /// available. Any write operation reactivates blocking reads.
        /// </summary>
        public override void Flush()
        {
            if (_isDisposed)
            {
                throw CreateObjectDisposedException();
            }

            lock (_commonLock)
            {
                _isFlushed = true;
            }

            _bufferIsNotEmpty.Set();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the
        /// stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified byte array
        /// with the values between offset and (offset + count - 1) replaced by the bytes read from
        /// the current source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data read from the
        /// current stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes
        /// requested if that many bytes are not currently available, or zero (0) if the end of the
        /// stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Offset must be non-negative.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be non-negative.");
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (_isDisposed)
            {
                throw CreateObjectDisposedException();
            }

            if (count == 0)
            {
                return 0;
            }

            lock (_readLock)
            {
                int bytesAvailable;
                while (true)
                {
                    lock (_commonLock)
                    {
                        if (_isDisposed)
                        {
                            return 0;
                        }

                        bytesAvailable = (int)(_writeCount - _readCount);
                        if (bytesAvailable >= count)
                        {
                            break;
                        }

                        if (_isFlushed)
                        {
                            if (bytesAvailable == 0)
                            {
                                return 0;
                            }

                            break;
                        }
                    }

                    _bufferIsNotEmpty.WaitOne();
                }

                int bytesToRead = Math.Min(count, bytesAvailable);
                int contiguousBytesAvailable = _ringBufferSize - _readOffset;
                if (contiguousBytesAvailable < bytesToRead)
                {
                    Array.Copy(_ringBuffer, _readOffset, buffer, offset, contiguousBytesAvailable);
                    Array.Copy(_ringBuffer, 0, buffer, offset + contiguousBytesAvailable, bytesToRead - contiguousBytesAvailable);
                    _readOffset = (_readOffset + bytesToRead) % _ringBufferSize;
                }
                else
                {
                    Array.Copy(_ringBuffer, _readOffset, buffer, offset, bytesToRead);
                    _readOffset += bytesToRead;
                }

                lock (_commonLock)
                {
                    _readCount += bytesToRead;
                }

                _bufferIsNotFull.Set();
                return bytesToRead;
            }
        }

        /// <summary>
        /// Sets the position within the current stream. Always throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">
        /// A value of type System.IO.SeekOrigin indicating the reference point used to obtain the
        /// new position.
        /// </param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the current stream. Always throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within
        /// this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies count bytes from buffer to the current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying bytes to the current stream.
        /// </param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Offset must be non-negative.");
            }

            if (count < 0 || count > _ringBufferSize)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be non-negative and less than or equal to the ring buffer size.");
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (_isDisposed)
            {
                throw CreateObjectDisposedException();
            }

            if (count == 0)
            {
                return;
            }

            lock (_writeLock)
            {
                int spaceAvailable;
                while (true)
                {
                    lock (_commonLock)
                    {
                        if (_isDisposed)
                        {
                            throw CreateObjectDisposedException();
                        }

                        spaceAvailable = _ringBufferSize - (int)(_writeCount - _readCount);
                        if (spaceAvailable >= count)
                        {
                            _isFlushed = false;
                            break;
                        }
                    }

                    _bufferIsNotFull.WaitOne();
                }

                int contiguousSpaceAvailable = _ringBufferSize - _writeOffset;
                if (contiguousSpaceAvailable < count)
                {
                    Array.Copy(buffer, offset, _ringBuffer, _writeOffset, contiguousSpaceAvailable);
                    Array.Copy(buffer, offset + contiguousSpaceAvailable, _ringBuffer, 0, count - contiguousSpaceAvailable);
                    _writeOffset = (_writeOffset + count) % _ringBufferSize;
                }
                else
                {
                    Array.Copy(buffer, offset, _ringBuffer, _writeOffset, count);
                    _writeOffset += count;
                }

                lock (_commonLock)
                {
                    _writeCount += count;
                }

                _bufferIsNotEmpty.Set();
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="System.IO.Stream"/> and
        /// optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        /// unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_isDisposed)
            {
                lock (_commonLock)
                {
                    _isDisposed = true;
                    _bufferIsNotEmpty.Set();
                    _bufferIsNotFull.Set();
                }
            }
        }

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }
}
