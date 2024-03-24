using System;
using System.IO;
#if NET6_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

using Renci.SshNet.Channels;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// ChannelInputStream is a one direction stream intended for channel data.
    /// </summary>
    internal sealed class ChannelInputStream : Stream
    {
        /// <summary>
        /// Channel to send data to.
        /// </summary>
        private readonly IChannelSession _channel;

        /// <summary>
        /// Total bytes passed through the stream.
        /// </summary>
        private long _totalPosition;

        /// <summary>
        /// Indicates whether the current instance was disposed.
        /// </summary>
        private bool _isDisposed;

        internal ChannelInputStream(IChannelSession channel)
        {
            _channel = channel;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <remarks>
        /// Once flushed, any subsequent read operations no longer block until requested bytes are available. Any write operation reactivates blocking
        /// reads.
        /// </remarks>
        public override void Flush()
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the stream is closed or end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <exception cref="ArgumentException">The sum of offset and count is larger than the buffer length.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The sum of offset and count is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset or count is negative.");
            }

            if (_isDisposed)
            {
                throw CreateObjectDisposedException();
            }

            if (count == 0)
            {
                return;
            }

            _channel.SendData(buffer, offset, count);
            _totalPosition += count;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous connect operation.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The sum of offset and count is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative.</exception>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset or count is negative.");
            }

            if (_isDisposed)
            {
                throw CreateObjectDisposedException();
            }

            if (count == 0)
            {
                return;
            }

            await _channel.SendDataAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            _totalPosition += count;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
            await WriteAsync(buffer.ToArray(), 0, buffer.Length, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
        }
#endif

        /// <summary>
        /// Releases the unmanaged resources used by the Stream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                // Closing the InputStream requires sending EOF.
                if (disposing && _totalPosition > 0 && _channel?.IsOpen == true)
                {
                    _channel.SendEof();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports seeking; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports writing; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="NotSupportedException">A class derived from Stream does not support seeking.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
        public override long Position
        {
            get { return _totalPosition; }
            set { throw new NotSupportedException(); }
        }

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }
}
