using System;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Renci.SshNet.Common;
using System.Diagnostics.Contracts;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IO.Stream" />
    public class SftpFileStream : Stream
    {
        private ulong _serverPosition;

        private byte[] _buffer;   // Shared read/write buffer.  Alloc on first use.
        private bool _canRead;
        private bool _canWrite;
        private bool _canSeek;

        private int _readPos;     // Read pointer within shared buffer.
        private int _readLen;     // Number of bytes read in buffer from file.
        private int _writePos;    // Write pointer within shared buffer.
        private int _bufferSize;  // Length of internal buffer, if it's allocated.

        private byte[] _handle;
        private ISftpSession _session;

        private long _pos;        // Cache current location in the file.
        private long _appendStart;// When appending, prevent overwriting file.

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports reading; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanRead
        {
            get { return _canRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports seeking; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanSeek
        {
            get { return _canSeek; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the stream supports writing; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        /// <summary>
        /// Indicates whether timeout properties are usable for <see cref="SftpFileStream"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> in all cases.
        /// </value>
        public override bool CanTimeout
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="IOException">IO operation failed. </exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Be design this is the exception that stream need to throw.")]
        public override long Length
        {
            get
            {
                EnsureSessionIsOpen();

                if (!CanSeek)
                    throw new NotSupportedException("Seek operation is not supported.");

                var attributes = _session.RequestFStat(_handle, true);
                if (attributes != null)
                {
                    return attributes.Size;
                }
                throw new IOException("Seek operation failed.");
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get
            {
                EnsureSessionIsOpen();

                if (!CanSeek)
                    throw new NotSupportedException("Seek operation is not supported.");

                Contract.Assert((_readPos == 0 && _readLen == 0 && _writePos >= 0) || (_writePos == 0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

                // Compensate for buffer that we read from the handle (_readLen) Vs what the user
                // read so far from the internel buffer (_readPos). Of course add any unwrittern  
                // buffered data
                return _pos + (_readPos - _readLen + _writePos);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                if (_writePos > 0) FlushWrite(false);
                _readPos = 0;
                _readLen = 0;
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Gets the name of the path that was used to construct the current <see cref="SftpFileStream"/>.
        /// </summary>
        /// <value>
        /// The name of the path that was used to construct the current <see cref="SftpFileStream"/>.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the operating system file handle for the file that the current <see cref="SftpFileStream"/> encapsulates.
        /// </summary>
        /// <value>
        /// The operating system file handle for the file that the current <see cref="SftpFileStream"/> encapsulates.
        /// </value>
        public virtual byte[] Handle
        {
            get
            {
                Flush();
                // Explicitly dump any buffered data, since the user could move our
                // position or write to the file.
                _readPos = 0;
                _readLen = 0;
                _writePos = 0;
                _buffer = null;
                return _handle;
            }
        }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFileStream" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="access">The access.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <exception cref="System.ArgumentException">handle</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// access
        /// or
        /// bufferSize
        /// </exception>
        internal SftpFileStream(ISftpSession session, string path, FileMode mode, FileAccess access, int bufferSize)
        {
            Timeout = TimeSpan.FromSeconds(30);
            Name = path;

            _session = session;

            // Now validate arguments.
            if (access < FileAccess.Read || access > FileAccess.ReadWrite)
                throw new ArgumentOutOfRangeException("access");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            _canRead = 0 != (access & FileAccess.Read);
            _canWrite = 0 != (access & FileAccess.Write);
            _canSeek = true;

            _readPos = 0;
            _readLen = 0;
            _writePos = 0;

            var flags = Flags.None;

            switch (access)
            {
                case FileAccess.Read:
                    flags |= Flags.Read;
                    break;
                case FileAccess.Write:
                    flags |= Flags.Write;
                    break;
                case FileAccess.ReadWrite:
                    flags |= Flags.Read;
                    flags |= Flags.Write;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("access");
            }

            if ((access & FileAccess.Write) == 0)
            {
                if (mode == FileMode.Create || mode == FileMode.CreateNew || mode == FileMode.Truncate || mode == FileMode.Append)
                {
                    throw new ArgumentException(string.Format("Combining {0}: {1} with {2}: {3} is invalid.",
                        typeof(FileMode).Name,
                        mode,
                        typeof(FileAccess).Name,
                        access));
                }
            }

            switch (mode)
            {
                case FileMode.Append:
                    flags |= Flags.Append;
                    break;
                case FileMode.Create:
                    _handle = _session.RequestOpen(path, flags | Flags.Truncate, true);
                    if (_handle == null)
                    {
                        flags |= Flags.CreateNew;
                    }
                    else
                    {
                        flags |= Flags.Truncate;
                    }
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
                    throw new ArgumentOutOfRangeException("mode");
            }

            if (_handle == null)
                _handle = _session.RequestOpen(path, flags);

            // instead of using the specified buffer size as is, we use it to calculate a buffer size
            // that ensures we always receive or send the max. number of bytes in a single SSH_FXP_READ
            // or SSH_FXP_WRITE message
            _bufferSize = (int)session.CalculateOptimalWriteLength((uint)bufferSize, _handle);

            _pos = 0;
            _appendStart = -1;

            if (mode == FileMode.Append)
            {
                _appendStart = SeekCore(0, SeekOrigin.End);
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SftpFileStream"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpFileStream()
        {
            if (_handle != null)
            {
                //BCLDebug.Correctness(_handle.IsClosed, "You didn't close a FileStream & it got finalized.  Name: \"" + _fileName + "\"");
                if (_session.IsOpen)
                {
                    throw new InvalidOperationException("You didn't close a SftpFileStream & it got finalized.");
                }
                Dispose(false);
            }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="ObjectDisposedException">Stream is closed.</exception>
        public override void Flush()
        {
            // This code is duplicated in Dispose
            EnsureSessionIsOpen();

            FlushInternalBuffer();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the
        /// number of bytes read.
        /// </summary>
        /// <param name="array">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="array" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested
        /// if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">array</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset
        /// or
        /// count
        /// </exception>
        /// <exception cref="System.ArgumentException">Invalid array range.</exception>
        /// <exception cref="System.NotSupportedException">Read operation is not supported.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override int Read(byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (array.Length - offset < count)
                throw new ArgumentException("Invalid array range.");

            EnsureSessionIsOpen();

            Contract.Assert((_readPos == 0 && _readLen == 0 && _writePos >= 0) || (_writePos == 0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

            bool isBlocked = false;
            int n = _readLen - _readPos;
            // if the read buffer is empty, read into either user's array or our
            // buffer, depending on number of bytes user asked for and buffer size.
            if (n == 0)
            {
                if (!CanRead)
                    throw new NotSupportedException("Read operation is not supported.");

                if (_writePos > 0) FlushWrite(false);
                if (!CanSeek || (count >= _bufferSize))
                {
                    n = ReadCore(array, offset, count);
                    // Throw away read buffer.
                    _readPos = 0;
                    _readLen = 0;
                    return n;
                }
                if (_buffer == null)
                    _buffer = new byte[_bufferSize];
                n = ReadCore(_buffer, 0, _bufferSize);
                if (n == 0) return 0;
                isBlocked = n < _bufferSize;
                _readPos = 0;
                _readLen = n;
            }
            // Now copy min of count or numBytesAvailable (ie, near EOF) to array.
            if (n > count) n = count;
            Buffer.BlockCopy(_buffer, _readPos, array, offset, n);

            _readPos += n;

            // We may have read less than the number of bytes the user asked 
            // for, but that is part of the Stream contract.  Reading again for
            // more data may cause us to block if we're using a device with 
            // no clear end of file, such as a serial port or pipe.  If we
            // blocked here & this code was used with redirected pipes for a
            // process's standard output, this can lead to deadlocks involving
            // two processes. But leave this here for files to avoid what would
            // probably be a breaking change.         -- 



            return n;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an <see cref="int" />, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Read is not supported.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="IOException">Read operation failed.</exception>
        public override int ReadByte()
        {
            EnsureSessionIsOpen();

            if (_readLen == 0 && !CanRead)
                throw new NotSupportedException("Read is not supported.");
            Contract.Assert((_readPos == 0 && _readLen == 0 && _writePos >= 0) || (_writePos == 0 && _readPos <= _readLen), "We're either reading or writing, but not both.");
            if (_readPos == _readLen)
            {
                if (_writePos > 0) FlushWrite(false);
                Contract.Assert(_bufferSize > 0, "_bufferSize > 0");
                if (_buffer == null)
                    _buffer = new byte[_bufferSize];
                _readLen = ReadCore(_buffer, 0, _bufferSize);
                _readPos = 0;
            }
            if (_readPos == _readLen)
                return -1;

            int result = _buffer[_readPos];
            _readPos++;
            return result;
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.ArgumentException">Invalid seek origin.</exception>
        /// <exception cref="System.NotSupportedException">Seek is not supported.</exception>
        /// <exception cref="System.IO.IOException">Unable seek backward to overwrite data that previously existed in a file opened in Append mode.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin < SeekOrigin.Begin || origin > SeekOrigin.End)
                throw new ArgumentException("Invalid seek origin.");

            EnsureSessionIsOpen();

            if (!CanSeek)
                throw new NotSupportedException("Seek is not supported.");

            Contract.Assert((_readPos == 0 && _readLen == 0 && _writePos >= 0) || (_writePos == 0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

            // If we've got bytes in our buffer to write, write them out.
            // If we've read in and consumed some bytes, we'll have to adjust
            // our seek positions ONLY IF we're seeking relative to the current
            // position in the stream.  This simulates doing a seek to the new
            // position, then a read for the number of bytes we have in our buffer.
            if (_writePos > 0)
            {
                FlushWrite(false);
            }
            else if (origin == SeekOrigin.Current)
            {
                // Don't call FlushRead here, which would have caused an infinite
                // loop.  Simply adjust the seek origin.  This isn't necessary
                // if we're seeking relative to the beginning or end of the stream.
                offset -= (_readLen - _readPos);
            }

            long oldPos = _pos + (_readPos - _readLen);
            long pos = SeekCore(offset, origin);

            // Prevent users from overwriting data in a file that was opened in
            // append mode.
            if (_appendStart != -1 && pos < _appendStart)
            {
                SeekCore(oldPos, SeekOrigin.Begin);
                throw new IOException("Unable seek backward to overwrite data that previously existed in a file opened in Append mode.");
            }

            // We now must update the read buffer.  We can in some cases simply
            // update _readPos within the buffer, copy around the buffer so our 
            // Position property is still correct, and avoid having to do more 
            // reads from the disk.  Otherwise, discard the buffer's contents.
            if (_readLen > 0)
            {
                // We can optimize the following condition:
                // oldPos - _readPos <= pos < oldPos + _readLen - _readPos
                if (oldPos == pos)
                {
                    if (_readPos > 0)
                    {
                        //Console.WriteLine("Seek: seeked for 0, adjusting buffer back by: "+_readPos+"  _readLen: "+_readLen);
                        Buffer.BlockCopy(_buffer, _readPos, _buffer, 0, _readLen - _readPos);
                        _readLen -= _readPos;
                        _readPos = 0;
                    }
                    // If we still have buffered data, we must update the stream's 
                    // position so our Position property is correct.
                    if (_readLen > 0)
                        SeekCore(_readLen, SeekOrigin.Current);
                }
                else if (oldPos - _readPos < pos && pos < oldPos + _readLen - _readPos)
                {
                    int diff = (int)(pos - oldPos);
                    //Console.WriteLine("Seek: diff was "+diff+", readpos was "+_readPos+"  adjusting buffer - shrinking by "+ (_readPos + diff));
                    Buffer.BlockCopy(_buffer, _readPos + diff, _buffer, 0, _readLen - (_readPos + diff));
                    _readLen -= (_readPos + diff);
                    _readPos = 0;
                    if (_readLen > 0)
                        SeekCore(_readLen, SeekOrigin.Current);
                }
                else
                {
                    // Lose the read buffer.
                    _readPos = 0;
                    _readLen = 0;
                }
                Contract.Assert(_readLen >= 0 && _readPos <= _readLen, "_readLen should be nonnegative, and _readPos should be less than or equal _readLen");
                Contract.Assert(pos == Position, "Seek optimization: pos != Position!  Buffer math was mangled.");
            }
            return pos;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">value</exception>
        /// <exception cref="System.NotSupportedException">
        /// Seek operation is not supported.
        /// or
        /// Write operation is not supported.
        /// </exception>
        /// <exception cref="System.IO.IOException">Unable to truncate data that previously existed in a file opened in Append mode.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value" /> must be greater than zero.</exception>
        public override void SetLength(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("value");

            EnsureSessionIsOpen();

            if (!CanSeek)
                throw new NotSupportedException("Seek operation is not supported.");
            if (!CanWrite)
                throw new NotSupportedException("Write operation is not supported.");

            // Handle buffering updates.
            if (_writePos > 0)
            {
                FlushWrite(false);
            }
            else if (_readPos < _readLen)
            {
                FlushRead();
            }
            _readPos = 0;
            _readLen = 0;

            if (_appendStart != -1 && value < _appendStart)
                throw new IOException("Unable to truncate data that previously existed in a file opened in Append mode.");
            SetLengthCore(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="array">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="array" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="array" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.ArgumentNullException">array</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset
        /// or
        /// count
        /// </exception>
        /// <exception cref="System.ArgumentException">Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.</exception>
        /// <exception cref="System.NotSupportedException">Write is not supported.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override void Write(byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (array.Length - offset < count)
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

            EnsureSessionIsOpen();

            if (_writePos == 0)
            {
                // Ensure we can write to the stream, and ready buffer for writing.
                if (!CanWrite)
                    throw new NotSupportedException("Write is not supported.");

                if (_readPos < _readLen) FlushRead();
                _readPos = 0;
                _readLen = 0;
            }

            // If our buffer has data in it, copy data from the user's array into
            // the buffer, and if we can fit it all there, return.  Otherwise, write
            // the buffer to disk and copy any remaining data into our buffer.
            // The assumption here is memcpy is cheaper than disk (or net) IO.
            // (10 milliseconds to disk vs. ~20-30 microseconds for a 4K memcpy)
            // So the extra copying will reduce the total number of writes, in 
            // non-pathological cases (ie, write 1 byte, then write for the buffer 
            // size repeatedly)
            if (_writePos > 0)
            {
                int numBytes = _bufferSize - _writePos;   // space left in buffer
                if (numBytes > 0)
                {
                    if (numBytes > count)
                        numBytes = count;
                    Buffer.BlockCopy(array, offset, _buffer, _writePos, numBytes);
                    _writePos += numBytes;
                    if (count == numBytes) return;
                    offset += numBytes;
                    count -= numBytes;
                }
                // Reset our buffer.  We essentially want to call FlushWrite
                // without calling Flush on the underlying Stream.

                WriteCore(_buffer, 0, _writePos);
                _writePos = 0;
                _buffer = null;
            }
            // If the buffer would slow writes down, avoid buffer completely.
            if (count >= _bufferSize)
            {
                Contract.Assert(_writePos == 0, "FileStream cannot have buffered data to write here!  Your stream will be corrupted.");
                WriteCore(array, offset, count);
                return;
            }
            else if (count == 0)
                return;  // Don't allocate a buffer then call memcpy for 0 bytes.
            if (_buffer == null)
                _buffer = new byte[_bufferSize];
            // Copy remaining bytes into buffer, to write at a later date.
            Buffer.BlockCopy(array, offset, _buffer, _writePos, count);
            _writePos = count;
            return;
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="System.NotSupportedException">Write operation is not supported.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="NotSupportedException">The stream does not support writing, or the stream is already closed.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override void WriteByte(byte value)
        {
            EnsureSessionIsOpen();

            if (_writePos == 0)
            {
                if (!CanWrite)
                    throw new NotSupportedException("Write operation is not supported.");

                if (_readPos < _readLen) FlushRead();
                _readPos = 0;
                _readLen = 0;
                Contract.Assert(_bufferSize > 0, "_bufferSize > 0");
                if (_buffer == null)
                    _buffer = new byte[_bufferSize];
            }
            if (_writePos == _bufferSize)
                FlushWrite(false);

            _buffer[_writePos] = value;
            _writePos++;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SftpFileStream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Nothing will be done differently based on whether we are 
            // disposing vs. finalizing.  This is taking advantage of the
            // weak ordering between normal finalizable objects & critical
            // finalizable objects, which I included in the SafeHandle 
            // design for FileStream, which would often "just work" when 
            // finalized.
            try
            {

                if (_handle != null && _session.IsOpen)
                {
                    // Flush data to disk iff we were writing.  After 
                    // thinking about this, we also don't need to flush
                    // our read position, regardless of whether the handle
                    // was exposed to the user.  They probably would NOT 
                    // want us to do this.
                    if (_writePos > 0)
                    {
                        FlushWrite(!disposing);
                    }
                }
            }
            finally
            {
                if (_handle != null && _session.IsOpen)
                {
                    _session.RequestClose(_handle);
                    _handle = null;
                }

                _canRead = false;
                _canWrite = false;
                _canSeek = false;
                // Don't set the buffer to null, to avoid a NullReferenceException
                // when users have a race condition in their code (ie, they call
                // Close when calling another method on Stream like Read).
                //_buffer = null;
                base.Dispose(disposing);
            }
        }

        private void FlushInternalBuffer()
        {
            if (_writePos > 0)
            {
                FlushWrite(false);
            }
            else if (_readPos < _readLen && CanSeek)
            {
                FlushRead();
            }
        }

        // Reading is done by blocks from the file, but someone could read
        // 1 byte from the buffer then write.  At that point, the OS's file
        // pointer is out of sync with the stream's position.  All write 
        // functions should call this function to preserve the position in the file.
        private void FlushRead()
        {
            Contract.Assert(_writePos == 0, "FileStream: Write buffer must be empty in FlushRead!");
            if (_readPos - _readLen != 0)
            {
                Contract.Assert(CanSeek, "FileStream will lose buffered read data now.");
                SeekCore(_readPos - _readLen, SeekOrigin.Current);
            }
            _readPos = 0;
            _readLen = 0;
        }

        // Writes are buffered.  Anytime the buffer fills up 
        // (_writePos + delta > _bufferSize) or the buffer switches to reading
        // and there is left over data (_writePos > 0), this function must be called.
        private void FlushWrite(bool calledFromFinalizer)
        {
            Contract.Assert(_readPos == 0 && _readLen == 0, "FileStream: Read buffer must be empty in FlushWrite!");

            WriteCore(_buffer, 0, _writePos);

            _writePos = 0;
            _buffer = null;
        }

        // We absolutely need this method broken out so that BeginWriteCore can call
        // a method without having to go through buffering code that might call
        // FlushWrite.
        private void SetLengthCore(long value)
        {
            Contract.Assert(value >= 0, "value >= 0");
            long origPos = _pos;

            if (_pos != value)
                SeekCore(value, SeekOrigin.Begin);

            //  TODO:   Oleg - Check if its needed (Set remote file size to serverPoistion), truncate remote file, perhaps issue set attribute or something to truncate remote file
            //if (!Win32Native.SetEndOfFile(_handle))
            //{
            //    int hr = Marshal.GetLastWin32Error();
            //    if (hr == __Error.ERROR_INVALID_PARAMETER)
            //        throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_FileLengthTooBig"));
            //    __Error.WinIOError(hr, String.Empty);
            //}
            // Return file pointer to where it was before setting length
            if (origPos != value)
            {
                if (origPos < value)
                    SeekCore(origPos, SeekOrigin.Begin);
                else
                    SeekCore(0, SeekOrigin.End);
            }
        }

        /// <summary>
        /// Reads the core.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        private int ReadCore(byte[] buffer, int offset, int count)
        {
            EnsureSessionIsOpen();
            Contract.Assert(CanRead, "CanRead");

            Contract.Assert(buffer != null, "buffer != null");
            Contract.Assert(_writePos == 0, "_writePos == 0");
            Contract.Assert(offset >= 0, "offset is negative");
            Contract.Assert(count >= 0, "count is negative");


            int r = ReadFromServer(buffer, offset, count);
            Contract.Assert(r >= 0, "FileStream's ReadCore is likely broken.");
            _pos += r;

            return r;
        }

        // This doesn't do argument checking.  Necessary for SetLength, which must
        // set the file pointer beyond the end of the file. This will update the 
        // internal position
        private long SeekCore(long offset, SeekOrigin origin)
        {
            EnsureSessionIsOpen();
            Contract.Assert(origin >= SeekOrigin.Begin && origin <= SeekOrigin.End, "origin>=SeekOrigin.Begin && origin<=SeekOrigin.End");

            switch (origin)
            {
                case SeekOrigin.Begin:
                    _serverPosition = (ulong)offset;
                    break;
                case SeekOrigin.Current:
                    _serverPosition += (ulong)offset;
                    break;
                case SeekOrigin.End:
                    try
                    {
                        var attributes = _session.RequestFStat(_handle, false);
                        _serverPosition = (ulong)attributes.Size - (ulong)offset;
                    }
                    finally
                    {
                        _session.RequestClose(_handle);
                    }
                    break;
                default:
                    break;
            }

            _pos = (long)_serverPosition;
            return (long)_serverPosition;
        }

        /// <summary>
        /// Writes the core.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        private void WriteCore(byte[] buffer, int offset, int count)
        {
            EnsureSessionIsOpen();

            Contract.Assert(CanWrite, "CanWrite");

            Contract.Assert(buffer != null, "buffer != null");
            Contract.Assert(_readPos == _readLen, "_readPos == _readLen");
            Contract.Assert(offset >= 0, "offset is negative");
            Contract.Assert(count >= 0, "count is negative");
            // Make sure we are writing to the position that we think we are

            int r = WriteToServer(buffer, offset, count);
            Contract.Assert(r >= 0, "FileStream's WriteCore is likely broken.");
            _pos += r;
            return;
        }

        // __ConsoleStream also uses this code. 
        /// <summary>
        /// Reads the file native.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">IndexOutOfRange_IORaceCondition</exception>
        private int ReadFromServer(byte[] bytes, int offset, int count)
        {
            EnsureSessionIsOpen();

            Contract.Requires(offset >= 0, "offset >= 0");
            Contract.Requires(count >= 0, "count >= 0");
            Contract.Requires(bytes != null, "bytes != null");

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.
            if (bytes.Length - offset < count)
                throw new IndexOutOfRangeException("Probable I/O race condition detected while copying memory. The I/O package is not thread safe by default. In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's Synchronized methods. This also applies to classes like StreamWriter and StreamReader.");

            if (bytes.Length == 0)
            {
                return 0;
            }

            var data = _session.RequestRead(_handle, _serverPosition + (ulong)offset, (uint)count);
            int numBytesRead = data.Length;
            _serverPosition += (ulong)offset + (ulong)numBytesRead;

            Buffer.BlockCopy(data, 0, bytes, offset, numBytesRead);


            return numBytesRead;
        }

        /// <summary>
        /// Writes the file native.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">IndexOutOfRange_IORaceCondition</exception>
        private int WriteToServer(byte[] bytes, int offset, int count)
        {
            EnsureSessionIsOpen();

            Contract.Requires(offset >= 0, "offset >= 0");
            Contract.Requires(count >= 0, "count >= 0");
            Contract.Requires(bytes != null, "bytes != null");
            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.  (the OS is reading from
            // the array we pass to WriteFile, but if we read beyond the end and
            // that memory isn't allocated, we could get an AV.)
            if (bytes.Length - offset < count)
                throw new IndexOutOfRangeException("Probable I/O race condition detected while copying memory. The I/O package is not thread safe by default. In multithreaded applications, a stream must be accessed in a thread-safe way, such as a thread-safe wrapper returned by TextReader's or TextWriter's Synchronized methods. This also applies to classes like StreamWriter and StreamReader.");

            // You can't use the fixed statement on an array of length 0.

            using (var wait = new AutoResetEvent(false))
            {
                _session.RequestWrite(_handle, _serverPosition, bytes, offset, count, wait);
                _serverPosition += (ulong)count;
            }

            return count;
        }

        private void EnsureSessionIsOpen()
        {
            if (_session == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (!_session.IsOpen)
                throw new ObjectDisposedException(GetType().FullName, "Cannot access a closed SFTP session.");
        }
    }
}