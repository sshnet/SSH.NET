namespace Renci.SshNet.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Globalization;

    /// <summary>
    /// PipeStream is a thread-safe read/write data stream for use between two threads in a 
    /// single-producer/single-consumer type problem.
    /// </summary>
    /// <version>2006/10/13 1.0</version>
    /// <remarks>Update on 2008/10/9 1.1 - uses Monitor instead of Manual Reset events for more elegant synchronicity.</remarks>
    /// <license>
    ///	Copyright (c) 2006 James Kolpack (james dot kolpack at google mail)
    ///	
    ///	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    ///	associated documentation files (the "Software"), to deal in the Software without restriction, 
    ///	including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    ///	sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    ///	furnished to do so, subject to the following conditions:
    ///	
    ///	The above copyright notice and this permission notice shall be included in all copies or 
    ///	substantial portions of the Software.
    ///	
    ///	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
    ///	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
    ///	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
    ///	LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT 
    ///	OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
    ///	OTHER DEALINGS IN THE SOFTWARE.
    /// </license>
    public class PipeStream : Stream
    {
        #region Private members

        /// <summary>
        /// Queue of bytes provides the datastructure for transmitting from an
        /// input stream to an output stream.
        /// </summary>
        /// <remarks>Possible more effecient ways to accomplish this.</remarks>
        private readonly Queue<byte> _buffer = new Queue<byte>();

        /// <summary>
        /// Indicates that the input stream has been flushed and that
        /// all remaining data should be written to the output stream.
        /// </summary>
        private bool _isFlushed;

        /// <summary>
        /// Maximum number of bytes to store in the buffer.
        /// </summary>
        private long _maxBufferLength = 200 * 1024 * 1024;

        /// <summary>
        /// Setting this to true will cause Read() to block if it appears
        /// that it will run out of data.
        /// </summary>
        private bool _canBlockLastRead;

        /// <summary>
        /// Indicates whether the current <see cref="PipeStream"/> is disposed.
        /// </summary>
        private bool _isDisposed;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the maximum number of bytes to store in the buffer.
        /// </summary>
        /// <value>The length of the max buffer.</value>
        public long MaxBufferLength
        {
            get { return _maxBufferLength; }
            set { _maxBufferLength = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to block last read method before the buffer is empty.
        /// When true, Read() will block until it can fill the passed in buffer and count.
        /// When false, Read() will not block, returning all the available buffer data.
        /// </summary>
        /// <remarks>
        /// Setting to true will remove the possibility of ending a stream reader prematurely.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if block last read method before the buffer is empty; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public bool BlockLastReadBuffer
        {
            get
            {
                if (_isDisposed)
                    throw CreateObjectDisposedException();

                return _canBlockLastRead;
            }
            set
            {
                if (_isDisposed)
                    throw CreateObjectDisposedException();

                _canBlockLastRead = value;

                // when turning off the block last read, signal Read() that it may now read the rest of the buffer.
                if (!_canBlockLastRead)
                    lock (_buffer)
                        Monitor.Pulse(_buffer);
            }
        }

        #endregion

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
            if (_isDisposed)
                throw CreateObjectDisposedException();

            _isFlushed = true;
            lock (_buffer)
            {
                // unblock read hereby allowing buffer to be partially filled
                Monitor.Pulse(_buffer);
            }
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
            if (offset != 0)
                throw new NotSupportedException("Offsets with value of non-zero are not supported");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            if (BlockLastReadBuffer && count >= _maxBufferLength)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "count({0}) > mMaxBufferLength({1})", count, _maxBufferLength));
            if (_isDisposed)
                throw CreateObjectDisposedException();
            if (count == 0)
                return 0;

            var readLength = 0;

            lock (_buffer)
            {
                while (!_isDisposed && !ReadAvailable(count))
                {
                    Monitor.Wait(_buffer);
                }

                // return zero when the read is interrupted by a close/dispose of the stream
                if (_isDisposed)
                {
                    return 0;
                }

                // fill the read buffer
                for (; readLength < count && _buffer.Count > 0; readLength++)
                {
                    buffer[readLength] = _buffer.Dequeue();
                }

                Monitor.Pulse(_buffer);
            }

            return readLength;
        }

        /// <summary>
        /// Returns true if there are
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns><c>True</c> if data available; otherwise<c>false</c>.</returns>
        private bool ReadAvailable(int count)
        {
            var length = Length;
            return (_isFlushed || length >= count) && (length >= (count + 1) || !BlockLastReadBuffer);
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

            lock (_buffer)
            {
                // wait until the buffer isn't full
                while (Length >= _maxBufferLength)
                    Monitor.Wait(_buffer);

                _isFlushed = false; // if it were flushed before, it soon will not be.

                // queue up the buffer data
                for (var i = offset; i < offset + count; i++)
                {
                    _buffer.Enqueue(buffer[i]);
                }

                Monitor.Pulse(_buffer); // signal that write has occurred
            }
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
                lock (_buffer)
                {
                    _isDisposed = true;
                    Monitor.Pulse(_buffer);
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
            get { return !_isDisposed; }
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
            get { return !_isDisposed; }
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
            get
            {
                if (_isDisposed)
                    throw CreateObjectDisposedException();

                return _buffer.Count;
            }
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
            get { return 0; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }
}
