using System;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Renci.SshNet.Common;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Exposes a System.IO.Stream around a remote SFTP file, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    public class SftpFileStream : Stream
    {
        //  TODO:   Add security method to set userid, groupid and other permission settings
        // Internal state.
        private byte[] _handle;
        private readonly bool _ownsHandle;
        private readonly bool _isAsync;
        private ISftpSession _session;

        // Buffer information.
        private readonly int _readBufferSize;
        private readonly byte[] _readBuffer;
        private readonly int _writeBufferSize;
        private readonly byte[] _writeBuffer;
        private int _bufferPosition;
        private int _bufferLen;
        private long _position;
        private bool _bufferOwnedByWrite;
        private bool _canRead;
        private bool _canSeek;
        private bool _canWrite;
        private ulong _serverFilePosition;

        private SftpFileAttributes _attributes;

        private readonly object _lock = new object();

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return _canRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return _canSeek; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
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
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="T:System.IO.IOException">IO operation failed. </exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Be design this is the exception that stream need to throw.")]
        public override long Length
        {
            get
            {
                // Lock down the file stream while we do this.
                lock (_lock)
                {
                    CheckSessionIsOpen();

                    if (!CanSeek)
                        throw new NotSupportedException("Seek operation is not supported.");

                    // Flush the write buffer, because it may
                    // affect the length of the stream.
                    if (_bufferOwnedByWrite)
                    {
                        FlushWriteBuffer();
                    }

                    //  Update file attributes
                    _attributes = _session.RequestFStat(_handle);

                    if (_attributes != null && _attributes.Size > -1)
                    {
                        return _attributes.Size;
                    }
                    throw new IOException("Seek operation failed.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        ///   
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get
            {
                CheckSessionIsOpen();
                if (!CanSeek)
                    throw new NotSupportedException("Seek operation not supported.");
                return _position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the FileStream was opened asynchronously or synchronously.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is async; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsAsync
        {
            get
            {
                return _isAsync;
            }
        }

        /// <summary>
        /// Gets the name of the FileStream that was passed to the constructor.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the operating system file handle for the file that the current SftpFileStream object encapsulates.
        /// </summary>
        public virtual byte[] Handle
        {
            get
            {
                Flush();
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

        internal SftpFileStream(ISftpSession session, string path, FileMode mode, FileAccess access, int bufferSize)
            : this(session, path, mode, access, bufferSize, false)
        {
        }

        internal SftpFileStream(ISftpSession session, string path, FileMode mode, FileAccess access, int bufferSize, bool useAsync)
        {
            // Validate the parameters.
            if (session == null)
                throw new SshConnectionException("Client not connected.");

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            if (access < FileAccess.Read || access > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (mode < FileMode.CreateNew || mode > FileMode.Append)
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            Timeout = TimeSpan.FromSeconds(30);
            Name = path;

            // Initialize the object state.
            _session = session;
            _ownsHandle = true;
            _isAsync = useAsync;
            _bufferPosition = 0;
            _bufferLen = 0;
            _bufferOwnedByWrite = false;
            _canRead = ((access & FileAccess.Read) != 0);
            _canSeek = true;
            _canWrite = ((access & FileAccess.Write) != 0);
            _position = 0;
            _serverFilePosition = 0;

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
            }

            if (_handle == null)
                _handle = _session.RequestOpen(path, flags);

            _attributes = _session.RequestFStat(_handle);

            // instead of using the specified buffer size as is, we use it to calculate a buffer size
            // that ensures we always receive or send the max. number of bytes in a single SSH_FXP_READ
            // or SSH_FXP_WRITE message

            _readBufferSize = (int)session.CalculateOptimalReadLength((uint)bufferSize);
            _readBuffer = new byte[_readBufferSize];
            _writeBufferSize = (int)session.CalculateOptimalWriteLength((uint)bufferSize, _handle);
            _writeBuffer = new byte[_writeBufferSize];

            if (mode == FileMode.Append)
            {
                _position = _attributes.Size;
                _serverFilePosition = (ulong)_attributes.Size;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SftpFileStream"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpFileStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="ObjectDisposedException">Stream is closed.</exception>
        public override void Flush()
        {
            lock (_lock)
            {
                CheckSessionIsOpen();

                if (_bufferOwnedByWrite)
                {
                    FlushWriteBuffer();
                }
                else
                {
                    FlushReadBuffer();
                }
            }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is <c>null</c>. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var readLen = 0;

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if ((buffer.Length - offset) < count)
                throw new ArgumentException("Invalid array range.");

            // Lock down the file stream while we do this.
            lock (_lock)
            {
                CheckSessionIsOpen();

                // Set up for the read operation.
                SetupRead();

                // Read data into the caller's buffer.
                while (count > 0)
                {
                    // How much data do we have available in the buffer?
                    var tempLen = _bufferLen - _bufferPosition;
                    if (tempLen <= 0)
                    {
                        _bufferPosition = 0;

                        var data = _session.RequestRead(_handle, (ulong)_position, (uint)_readBufferSize);

                        _bufferLen = data.Length;

                        Buffer.BlockCopy(data, 0, _readBuffer, 0, _bufferLen);
                        _serverFilePosition = (ulong)_position;

                        if (_bufferLen == 0)
                        {
                            break;
                        }
                        tempLen = _bufferLen;
                    }

                    // Don't read more than the caller wants.
                    if (tempLen > count)
                    {
                        tempLen = count;
                    }

                    // Copy stream data to the caller's buffer.
                    Buffer.BlockCopy(_readBuffer, _bufferPosition, buffer, offset, tempLen);

                    // Advance to the next buffer positions.
                    readLen += tempLen;
                    offset += tempLen;
                    count -= tempLen;
                    _bufferPosition += tempLen;
                    _position += tempLen;
                }
            }

            // Return the number of bytes that were read to the caller.
            return readLen;
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <param name="callback">The method to be called when the asynchronous read operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is <c>null</c>. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
           

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if ((buffer.Length - offset) < count)
                throw new ArgumentException("Invalid array range.");

            var asyncResult = new SftpFileStreamAsyncResult(callback, state);


            int bytesToRead = count; //number of bytes per read
            int readLen = 0; //bytes actually read per read (sync)
            int totalBytes = 0; //total read (sync) or requested (async) so far
            uint maxBytes = _session.CalculateOptimalReadLength((uint)count);

            if (maxBytes < count)
            {//will need to loop Reads until we reach count
                bytesToRead = (int)maxBytes;
            }

            if (!_isAsync)
            {//actually do this synchronously
                try
                {
                    do
                    {
                        if (bytesToRead + totalBytes > count)
                            bytesToRead = count - totalBytes; //knock bytesToRead down so it doesn't go past count requested

                        readLen = Read(buffer, offset, bytesToRead);

                        totalBytes += readLen;
                        offset += readLen;
                    } while (totalBytes < count && readLen > 0);

                    asyncResult.Update(totalBytes);
                    asyncResult.SetAsCompleted(null, true);
                }
                catch (Exception exp)
                {
                    asyncResult.SetAsCompleted(exp, true);
                }
            }
            else
            {// asynchronous implementation

                // Lock down the file stream while we do this.
                lock (_lock)
                {
                    CheckSessionIsOpen();

                    // Set up for the read operation.
                    SetupRead();

                    SshException exception = null;
                    byte[] data = null;

                    do 
                    {
                        //offset will be updated after making this request, so we want to save the current values for response handling
                        int thisOffset = offset;
                        int thisBytesToRead = bytesToRead;
                        if (thisBytesToRead + totalBytes > count)
                            thisBytesToRead = count - totalBytes; //knock thisBytesToRead down so it doesn't go past count requested

                        // Read data into the caller's buffer.
                        _session.RequestReadAsync(_handle, (ulong)_position, (uint)thisBytesToRead, 
                            response =>
                                {
                                    try
                                    {
                                        data = response.Data;
                                        if (data.Length > buffer.Length - thisOffset || data.Length > thisBytesToRead)
                                            throw new IOException("The server returned more data than requested");

                                        // Copy stream data to the caller's buffer.
                                        Buffer.BlockCopy(data, 0, buffer, thisOffset, data.Length);

                                        asyncResult.Update(asyncResult.Bytes + data.Length);
                                        if (asyncResult.Bytes == count)
                                            asyncResult.SetAsCompleted(null, false);
                                    }
                                    catch (Exception exp)
                                    {
                                        asyncResult.SetAsCompleted(exp, false);
                                    }
                                },
                            response =>
                                {
                                    try
                                    {
                                        if (response.StatusCode != StatusCodes.Eof)
                                        {
                                            exception = SftpSession.GetSftpException(response);
                                            asyncResult.Update(0);
                                            buffer = Array<byte>.Empty;
                                            if (exception != null) throw exception;
                                        }
                                            
                                        asyncResult.SetAsCompleted(null, false);
                                    }
                                    catch (Exception exp)
                                    {
                                        asyncResult.SetAsCompleted(exp, false);
                                    }
                                });
                        
                        _serverFilePosition = (ulong)_position;

                        // Advance to the next buffer positions.
                        _position += thisBytesToRead;
                        offset += thisBytesToRead;

                        //we didn't actually read the bytes yet--this is just so we don't send too many requests
                        totalBytes += thisBytesToRead;

                    } while (totalBytes < count);

                }
            }
            
            // Return the number of bytes that were read to the caller.
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous read from the stream.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP read request.</param>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndRead(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException">The path was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        public override int EndRead(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpFileStreamAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
            return ar.Bytes;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an <see cref="int"/>, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="IOException">Read operation failed.</exception>
        public override int ReadByte()
        {
            // Lock down the file stream while we do this.
            lock (_lock)
            {
                CheckSessionIsOpen();

                // Setup the object for reading.
                SetupRead();

                // Read more data into the internal buffer if necessary.
                if (_bufferPosition >= _bufferLen)
                {
                    _bufferPosition = 0;

                    var data = _session.RequestRead(_handle, (ulong)_position, (uint)_readBufferSize);

                    _bufferLen = data.Length;
                    Buffer.BlockCopy(data, 0, _readBuffer, 0, _readBufferSize);
                    _serverFilePosition = (ulong)_position;

                    if (_bufferLen == 0)
                    {
                        // We've reached EOF.
                        return -1;
                    }
                }

                // Extract the next byte from the buffer.
                ++_position;
                return _readBuffer[_bufferPosition++];
            }
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosn = -1;

            // Lock down the file stream while we do this.
            lock (_lock)
            {
                CheckSessionIsOpen();

                if (!CanSeek)
                    throw new NotSupportedException("Seek is not supported.");

                // Don't do anything if the position won't be moving.
                if (origin == SeekOrigin.Begin && offset == _position)
                {
                    return offset;
                }
                if (origin == SeekOrigin.Current && offset == 0)
                {
                    return _position;
                }

                _attributes = _session.RequestFStat(_handle);

                // The behaviour depends upon the read/write mode.
                if (_bufferOwnedByWrite)
                {
                    // Flush the write buffer and then seek.
                    FlushWriteBuffer();

                    switch (origin)
                    {
                        case SeekOrigin.Begin:
                            newPosn = offset;
                            break;
                        case SeekOrigin.Current:
                            newPosn = _position + offset;
                            break;
                        case SeekOrigin.End:
                            newPosn = _attributes.Size - offset;
                            break;
                    }

                    if (newPosn == -1)
                    {
                        throw new EndOfStreamException("End of stream.");
                    }
                    _position = newPosn;
                    _serverFilePosition = (ulong)newPosn;
                }
                else
                {
                    // Determine if the seek is to somewhere inside
                    // the current read buffer bounds.
                    if (origin == SeekOrigin.Begin)
                    {
                        newPosn = _position - _bufferPosition;
                        if (offset >= newPosn && offset <
                                (newPosn + _bufferLen))
                        {
                            _bufferPosition = (int)(offset - newPosn);
                            _position = offset;
                            return _position;
                        }
                    }
                    else if (origin == SeekOrigin.Current)
                    {
                        newPosn = _position + offset;
                        if (newPosn >= (_position - _bufferPosition) &&
                           newPosn < (_position - _bufferPosition + _bufferLen))
                        {
                            _bufferPosition =
                                (int)(newPosn - (_position - _bufferPosition));
                            _position = newPosn;
                            return _position;
                        }
                    }

                    // Abandon the read buffer.
                    _bufferPosition = 0;
                    _bufferLen = 0;

                    // Seek to the new position.
                    switch (origin)
                    {
                        case SeekOrigin.Begin:
                            newPosn = offset;
                            break;
                        case SeekOrigin.Current:
                            newPosn = _position + offset;
                            break;
                        case SeekOrigin.End:
                            newPosn = _attributes.Size - offset;
                            break;
                    }

                    if (newPosn < 0)
                    {
                        throw new EndOfStreamException();
                    }

                    _position = newPosn;
                }
                return _position;
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> must be greater than zero.</exception>
        public override void SetLength(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("value");

            // Lock down the file stream while we do this.
            lock (_lock)
            {
                CheckSessionIsOpen();

                if (!CanSeek)
                    throw new NotSupportedException("Seek is not supported.");

                SetupWrite();
                _attributes.Size = value;
                _session.RequestFSetStat(_handle, _attributes);
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if ((buffer.Length - offset) < count)
                throw new ArgumentException("Invalid array range.");

            // Lock down the file stream while we do this.
            lock (_lock)
            {
                CheckSessionIsOpen();

                // Setup this object for writing.
                SetupWrite();

                // Write data to the file stream.
                while (count > 0)
                {
                    // Determine how many bytes we can write to the buffer.
                    var tempLen = _writeBufferSize - _bufferPosition;
                    if (tempLen <= 0)
                    {
                        // flush write buffer, and mark it empty
                        FlushWriteBuffer();
                        // we can now write or buffer the full buffer size
                        tempLen = _writeBufferSize;
                    }

                    // limit the number of bytes to write to the actual number of bytes requested
                    if (tempLen > count)
                    {
                        tempLen = count;
                    }

                    // Can we short-cut the internal buffer?
                    if (_bufferPosition == 0 && tempLen == _writeBufferSize)
                    {
                        using (var wait = new AutoResetEvent(false))
                        {
                            _session.RequestWrite(_handle, _serverFilePosition, buffer, offset, tempLen, wait);
                            _serverFilePosition += (ulong) tempLen;
                        }
                    }
                    else
                    {
                        // No: copy the data to the write buffer first.
                        Buffer.BlockCopy(buffer, offset, _writeBuffer, _bufferPosition, tempLen);
                        _bufferPosition += tempLen;
                    }

                    // Advance the buffer and stream positions.
                    _position += tempLen;
                    offset += tempLen;
                    count -= tempLen;
                }

                // If the buffer is full, then do a speculative flush now,
                // rather than waiting for the next call to this method.
                if (_bufferPosition >= _writeBufferSize)
                {
                    using (var wait = new AutoResetEvent(false))
                    {
                        _session.RequestWrite(_handle, _serverFilePosition, _writeBuffer, 0, _bufferPosition, wait);
                        _serverFilePosition += (ulong) _bufferPosition;
                    }

                    _bufferPosition = 0;
                }
            }
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if ((buffer.Length - offset) < count)
                throw new ArgumentException("Invalid array range.");

            var asyncResult = new SftpFileStreamAsyncResult(callback, state);

            int bytesToWrite = count; //number of bytes per write
            int totalBytes = 0; //total written so far
            uint maxBytes = _session.CalculateOptimalWriteLength((uint)count, _handle);

            if (maxBytes < count)
            {//will need to loop RequestWrites until we reach count
                bytesToWrite = (int)maxBytes;
            }

            if (!_isAsync)
            {
                    try
                    {
                        do
                        {
                            if (bytesToWrite + totalBytes > count)
                                bytesToWrite = count - totalBytes; //knock bytesToWrite down so it doesn't go past count requested

                            Write(buffer, offset, bytesToWrite);
                            offset += bytesToWrite;
                            totalBytes += bytesToWrite;
                        } while (totalBytes < count);

                        asyncResult.Update(totalBytes);
                        asyncResult.SetAsCompleted(null, true);
                    }
                    catch (Exception exp)
                    {
                        asyncResult.SetAsCompleted(exp, true);
                    }
            }
            else
            {
                // Lock down the file stream while we do this.
                lock (_lock)
                {
                    CheckSessionIsOpen();

                    // Setup this object for writing.
                    SetupWrite();

                    do
                    {
                        int thisBytesToWrite = bytesToWrite;
                        if (thisBytesToWrite + totalBytes > count)
                            thisBytesToWrite = count - totalBytes; //knock bytesToWrite down so it doesn't go past count requested

                        // Write data to the file stream.
                        _session.RequestWrite(_handle, _serverFilePosition, buffer, offset, thisBytesToWrite, null,
                            response =>
                            {
                                ThreadAbstraction.ExecuteThread(() =>
                                {
                                    try
                                    {
                                        if (response.StatusCode == StatusCodes.Ok)
                                        {
                                            asyncResult.Update(asyncResult.Bytes + thisBytesToWrite);
                                            if (asyncResult.Bytes == count)
                                                asyncResult.SetAsCompleted(null, false);
                                        }
                                        else
                                        {
                                            throw new IOException("The server returned an error during the write operation: " + response.StatusCode.ToString());
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        asyncResult.SetAsCompleted(exp, false);
                                    }
                                });
                            });

                        // Advance the buffer and stream positions.
                        _position += thisBytesToWrite;
                        _serverFilePosition += (ulong)thisBytesToWrite;

                        offset += thisBytesToWrite;
                        totalBytes += thisBytesToWrite;
                    } while (totalBytes < count);
                }

            }
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous write to the stream.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP write request.</param>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndRead(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException">The path was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpFileStreamAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception

            ar.EndInvoke();
        }
        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void WriteByte(byte value)
        {
            // Lock down the file stream while we do this.
            lock (_lock)
            {
                CheckSessionIsOpen();

                // Setup the object for writing.
                SetupWrite();

                // Flush the current buffer if it is full.
                if (_bufferPosition >= _writeBufferSize)
                {
                    using (var wait = new AutoResetEvent(false))
                    {
                        _session.RequestWrite(_handle, _serverFilePosition, _writeBuffer, 0, _bufferPosition, wait);
                        _serverFilePosition += (ulong) _bufferPosition;
                    }

                    _bufferPosition = 0;
                }

                // Write the byte into the buffer and advance the posn.
                _writeBuffer[_bufferPosition++] = value;
                ++_position;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_session != null)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        if (_session != null)
                        {
                            _canRead = false;
                            _canSeek = false;
                            _canWrite = false;

                            if (_handle != null)
                            {
                                if (_session.IsOpen)
                                {
                                    if (_bufferOwnedByWrite)
                                    {
                                        FlushWriteBuffer();
                                    }

                                    if (_ownsHandle)
                                    {
                                        _session.RequestClose(_handle);
                                    }
                                }

                                _handle = null;
                            }

                            _session = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flushes the read data from the buffer.
        /// </summary>
        private void FlushReadBuffer()
        {
            if (_canSeek)
            {
                if (_bufferPosition < _bufferLen)
                {
                    _position -= _bufferPosition;
                }
                _bufferPosition = 0;
                _bufferLen = 0;
            }
        }

        /// <summary>
        /// Flush any buffered write data to the file.
        /// </summary>
        private void FlushWriteBuffer()
        {
            if (_bufferPosition > 0)
            {
                using (var wait = new AutoResetEvent(false))
                {
                    _session.RequestWrite(_handle, _serverFilePosition, _writeBuffer, 0, _bufferPosition, wait);
                    _serverFilePosition += (ulong) _bufferPosition;
                }

                _bufferPosition = 0;
            }
        }

        /// <summary>
        /// Setups the read.
        /// </summary>
        private void SetupRead()
        {
            if (!CanRead)
                throw new NotSupportedException("Read not supported.");

            if (_bufferOwnedByWrite)
            {
                FlushWriteBuffer();
                _bufferOwnedByWrite = false;
            }
        }

        /// <summary>
        /// Setups the write.
        /// </summary>
        private void SetupWrite()
        {
            if ((!CanWrite))
                throw new NotSupportedException("Write not supported.");

            if (!_bufferOwnedByWrite)
            {
                FlushReadBuffer();
                _bufferOwnedByWrite = true;
            }
        }

        private void CheckSessionIsOpen()
        {
            if (_session == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (!_session.IsOpen)
                throw new ObjectDisposedException(GetType().FullName, "Cannot access a closed SFTP session.");
        }
    }
}