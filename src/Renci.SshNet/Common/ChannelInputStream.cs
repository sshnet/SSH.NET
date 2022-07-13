namespace Renci.SshNet.Common
{
    using System;
    using System.IO;
    using Renci.SshNet.Channels;

    /// <summary>
    /// ChannelInputStream is a one direction stream intended for channel data.
    /// </summary>
    /// <license>
    /// Copyright (c) 2016 Toni Spets (toni.spets@iki.fi)
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    /// associated documentation files (the "Software"), to deal in the Software without restriction, 
    /// including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    /// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    /// furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in all copies or 
    /// substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
    /// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
    /// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
    /// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT 
    /// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
    /// OTHER DEALINGS IN THE SOFTWARE.
    /// </license>
    public class ChannelInputStream : Stream
    {
        #region Private members

        /// <summary>
        /// Channel to send data to.
        /// </summary>
        private IChannelSession _channel;

        /// <summary>
        /// Total bytes passed through the stream.
        /// </summary>
        private long _totalPosition;

        /// <summary>
        /// Indicates whether the current <see cref="PipeStream"/> is disposed.
        /// </summary>
        private bool _isDisposed;

        #endregion

        internal ChannelInputStream(IChannelSession channel)
        {
            _channel = channel;
        }

        #region Stream overide methods

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

        ///<summary>
        ///When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        ///</summary>
        ///<returns>
        ///The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the stream is closed or end of the stream has been reached.
        ///</returns>
        ///<param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        ///<param name="count">The maximum number of bytes to be read from the current stream.</param>
        ///<param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        ///<exception cref="ArgumentException">The sum of offset and count is larger than the buffer length.</exception>
        ///<exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        ///<exception cref="NotSupportedException">The stream does not support reading.</exception>
        ///<exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        ///<exception cref="IOException">An I/O error occurs.</exception>
        ///<exception cref="ArgumentOutOfRangeException">offset or count is negative.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        ///<summary>
        ///When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        ///</summary>
        ///<param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        ///<param name="count">The number of bytes to be written to the current stream.</param>
        ///<param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        ///<exception cref="IOException">An I/O error occurs.</exception>
        ///<exception cref="NotSupportedException">The stream does not support writing.</exception>
        ///<exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        ///<exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        ///<exception cref="ArgumentException">The sum of offset and count is greater than the buffer length.</exception>
        ///<exception cref="ArgumentOutOfRangeException">offset or count is negative.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            if (_isDisposed)
                throw CreateObjectDisposedException();
            if (count == 0)
                return;

            _channel.SendData(buffer, offset, count);

            _totalPosition += count;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Stream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Disposing a <see cref="PipeStream"/> will interrupt blocking read and write operations.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_isDisposed)
            {
                _isDisposed = true;
                if (_totalPosition > 0 && _channel.IsOpen) {
                    _channel.SendEof();
                }
            }
        }

        ///<summary>
        ///When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        ///</summary>
        ///<returns>
        ///true if the stream supports reading; otherwise, false.
        ///</returns>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports seeking; otherwise, <c>false</c>.
        ///</returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports writing; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
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
        /// When overridden in a derived class, gets or sets the position within the current stream.
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

        #endregion

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }
}
