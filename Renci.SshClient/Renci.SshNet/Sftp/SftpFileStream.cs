using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Renci.SshNet.Common;

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
        private FileAccess _access;
        private bool _ownsHandle;
        private bool _isAsync;
        private string _path;
        private SftpSession _session;

        // Buffer information.
        private int _bufferSize;
        private byte[] _buffer;
        private int _bufferPosn;
        private int _bufferLen;
        private long _position;
        private bool _bufferOwnedByWrite;
        private bool _canSeek;
        private ulong _serverFilePosition;

        private SftpFileAttributes _attributes;

        private object _lock = new object();

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get
            {
                return ((this._access & FileAccess.Read) != 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get
            {
                return this._canSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get
            {
                return ((this._access & FileAccess.Write) != 0);
            }
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
                // Validate that the object can actually do this.
                if (!this._canSeek)
                {
                    throw new NotSupportedException("Seek operation is not supported.");
                }

                // Lock down the file stream while we do this.
                lock (this._lock)
                {
                    if (this._handle == null)
                    {
                        // ECMA says this should be IOException even though
                        // everywhere else uses ObjectDisposedException.
                        throw new IOException("Stream is closed.");
                    }

                    // Flush the write buffer, because it may
                    // affect the length of the stream.
                    if (this._bufferOwnedByWrite)
                    {
                        this.FlushWriteBuffer();
                    }

                    //  Update file attributes
                    this._attributes = this._session.RequestFStat(this._handle);

                    if (this._attributes != null && this._attributes.Size > -1)
                    {
                        return this._attributes.Size;
                    }
                    else
                    {
                        throw new IOException("Seek operation failed.");
                    }
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
                if (!this._canSeek)
                {
                    throw new NotSupportedException("Seek operation not supported.");
                }
                return this._position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
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
                return this._isAsync;
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
                this.Flush();
                return this._handle;
            }
        }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public TimeSpan Timeout { get; set; }

        internal SftpFileStream(SftpSession session, string path, FileMode mode)
            : this(session, path, mode, FileAccess.ReadWrite, 4096, false)
        {
            // Nothing to do here.
        }

        internal SftpFileStream(SftpSession session, string path, FileMode mode, FileAccess access)
            : this(session, path, mode, access, 4096, false)
        {
            // Nothing to do here.
        }

        internal SftpFileStream(SftpSession session, string path, FileMode mode, FileAccess access, int bufferSize)
            : this(session, path, mode, access, bufferSize, false)
        {
            // Nothing to do here.
        }

        internal SftpFileStream(SftpSession session, string path, FileMode mode, FileAccess access, int bufferSize, bool useAsync)
        {
            // Validate the parameters.
            if (session == null)
                throw new SshConnectionException("Client not connected.");

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (bufferSize <= 0 || bufferSize > 16 * 1024)
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

            this.Timeout = TimeSpan.FromSeconds(30);
            this.Name = path;

            // Initialize the object state.
            this._session = session;
            this._access = access;
            this._ownsHandle = true;
            this._isAsync = useAsync;
            this._path = path;
            this._bufferSize = bufferSize;
            this._buffer = new byte[bufferSize];
            this._bufferPosn = 0;
            this._bufferLen = 0;
            this._bufferOwnedByWrite = false;
            this._canSeek = true;
            this._position = 0;
            this._serverFilePosition = 0;
            this._session.Disconnected += Session_Disconnected;

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
                    break;
            }

            switch (mode)
            {
                case FileMode.Append:
                    flags |= Flags.Append;
                    break;
                case FileMode.Create:
                    this._handle = this._session.RequestOpen(path, flags | Flags.Truncate, true);
                    if (this._handle == null)
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
                    break;
            }

            if (this._handle == null)
                this._handle = this._session.RequestOpen(this._path, flags);

            this._attributes = this._session.RequestFStat(this._handle);

            if (mode == FileMode.Append)
            {
                this._position = this._attributes.Size;
                this._serverFilePosition = (ulong)this._attributes.Size;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SftpFileStream"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpFileStream()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> that represents the asynchronous read, which could still be pending.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">Attempted an asynchronous read past the end of the stream, or a disk error occurs. </exception>
        ///   
        /// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The current Stream implementation does not support the read operation. </exception>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>
        /// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="asyncResult"/> is null. </exception>
        ///   
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream. </exception>
        ///   
        /// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return base.EndRead(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>
        /// An IAsyncResult that represents the asynchronous write, which could still be pending.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs. </exception>
        ///   
        /// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The current Stream implementation does not support the write operation. </exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="asyncResult"/> is null. </exception>
        ///   
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="asyncResult"/> did not originate from a <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)"/> method on the current stream. </exception>
        ///   
        /// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }

        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        public override void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="ObjectDisposedException">Stream is closed.</exception>
        public override void Flush()
        {
            lock (this._lock)
            {
                if (this._handle != null)
                {
                    if (this._bufferOwnedByWrite)
                    {
                        this.FlushWriteBuffer();
                    }
                    else
                    {
                        this.FlushReadBuffer();
                    }
                }
                else
                {
                    throw new ObjectDisposedException("Stream is closed.");
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
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        ///   
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="buffer"/> is null. </exception>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        ///   
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int readLen = 0;
            int tempLen;

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            else if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            else if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Invalid array range.");
            }

            // Lock down the file stream while we do this.
            lock (this._lock)
            {
                // Set up for the read operation.
                this.SetupRead();

                // Read data into the caller's buffer.
                while (count > 0)
                {
                    // How much data do we have available in the buffer?
                    tempLen = this._bufferLen - this._bufferPosn;
                    if (tempLen <= 0)
                    {
                        this._bufferPosn = 0;

                        var data = this._session.RequestRead(this._handle, (ulong)this._position, (uint)this._bufferSize);

                        this._bufferLen = data.Length;

                        Buffer.BlockCopy(data, 0, this._buffer, 0, this._bufferLen);
                        this._serverFilePosition = (ulong)this._position;

                        if (this._bufferLen < 0)
                        {
                            this._bufferLen = 0;
                            //  TODO:   Add SFTP error code or message if possible
                            throw new IOException("Read operation failed.");
                        }
                        else if (this._bufferLen == 0)
                        {
                            break;
                        }
                        else
                        {
                            tempLen = this._bufferLen;
                        }
                    }

                    // Don't read more than the caller wants.
                    if (tempLen > count)
                    {
                        tempLen = count;
                    }

                    // Copy stream data to the caller's buffer.
                    Buffer.BlockCopy(this._buffer, this._bufferPosn, buffer, offset, tempLen);

                    // Advance to the next buffer positions.
                    readLen += tempLen;
                    offset += tempLen;
                    count -= tempLen;
                    this._bufferPosn += tempLen;
                    this._position += tempLen;
                }
            }

            // Return the number of bytes that were read to the caller.
            return readLen;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="System.IO.IOException">Read operation failed.</exception>
        public override int ReadByte()
        {
            // Lock down the file stream while we do this.
            lock (this._lock)
            {
                // Setup the object for reading.
                this.SetupRead();

                // Read more data into the internal buffer if necessary.
                if (this._bufferPosn >= this._bufferLen)
                {
                    this._bufferPosn = 0;

                    var data = this._session.RequestRead(this._handle, (ulong)this._position, (uint)this._bufferSize);

                    this._bufferLen = data.Length;
                    Buffer.BlockCopy(data, 0, this._buffer, 0, this._bufferSize);
                    this._serverFilePosition = (ulong)this._position;

                    if (this._bufferLen < 0)
                    {
                        this._bufferLen = 0;
                        //  TODO:   Add SFTP error code or message if possible
                        throw new IOException("Read operation failed.");
                    }
                    else if (this._bufferLen == 0)
                    {
                        // We've reached EOF.
                        return -1;
                    }
                }

                // Extract the next byte from the buffer.
                ++this._position;
                return this._buffer[this._bufferPosn++];
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
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosn = -1;

            // Bail out if this stream is not capable of seeking.
            if (!this._canSeek)
            {
                throw new NotSupportedException("Seek is not supported.");
            }

            // Lock down the file stream while we do this.
            lock (this._lock)
            {
                // Bail out if the handle is invalid.
                if (this._handle == null)
                {
                    throw new ObjectDisposedException("Stream is closed.");
                }

                // Don't do anything if the position won't be moving.
                if (origin == SeekOrigin.Begin && offset == this._position)
                {
                    return offset;
                }
                else if (origin == SeekOrigin.Current && offset == 0)
                {
                    return this._position;
                }

                this._attributes = this._session.RequestFStat(this._handle);

                // The behaviour depends upon the read/write mode.
                if (this._bufferOwnedByWrite)
                {
                    // Flush the write buffer and then seek.
                    this.FlushWriteBuffer();

                    switch (origin)
                    {
                        case SeekOrigin.Begin:
                            newPosn = offset;
                            break;
                        case SeekOrigin.Current:
                            newPosn = this._position + offset;
                            break;
                        case SeekOrigin.End:
                            newPosn = this._attributes.Size - offset;
                            break;
                        default:
                            break;
                    }

                    if (newPosn == -1)
                    {
                        throw new EndOfStreamException("End of stream.");
                    }
                    this._position = newPosn;
                    this._serverFilePosition = (ulong)newPosn;
                }
                else
                {
                    // Determine if the seek is to somewhere inside
                    // the current read buffer bounds.
                    if (origin == SeekOrigin.Begin)
                    {
                        newPosn = this._position - this._bufferPosn;
                        if (offset >= newPosn && offset <
                                (newPosn + this._bufferLen))
                        {
                            this._bufferPosn = (int)(offset - newPosn);
                            this._position = offset;
                            return this._position;
                        }
                    }
                    else if (origin == SeekOrigin.Current)
                    {
                        newPosn = this._position + offset;
                        if (newPosn >= (this._position - this._bufferPosn) &&
                           newPosn < (this._position - this._bufferPosn + this._bufferLen))
                        {
                            this._bufferPosn =
                                (int)(newPosn - (this._position - this._bufferPosn));
                            this._position = newPosn;
                            return this._position;
                        }
                    }

                    // Abandon the read buffer.
                    this._bufferPosn = 0;
                    this._bufferLen = 0;

                    // Seek to the new position.
                    switch (origin)
                    {
                        case SeekOrigin.Begin:
                            newPosn = offset;
                            break;
                        case SeekOrigin.Current:
                            newPosn = this._position + offset;
                            break;
                        case SeekOrigin.End:
                            newPosn = this._attributes.Size - offset;
                            break;
                        default:
                            break;
                    }

                    if (newPosn < 0)
                    {
                        throw new EndOfStreamException();
                    }

                    this._position = newPosn;
                }
                return this._position;
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> must be greater than zero.</exception>
        public override void SetLength(long value)
        {
            // Validate the parameters and setup the object for writing.
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (!this._canSeek)
            {
                throw new NotSupportedException("Seek is not supported.");
            }

            // Lock down the file stream while we do this.
            lock (this._lock)
            {
                // Setup this object for writing.
                this.SetupWrite();

                this._attributes.Size = value;

                this._session.RequestFSetStat(this._handle, this._attributes);
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        ///   
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="buffer"/> is null. </exception>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        ///   
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            int tempLen;

            // Validate the parameters
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            else if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            else if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Invalid array range.");
            }

            // Lock down the file stream while we do this.
            lock (this._lock)
            {
                // Setup this object for writing.
                this.SetupWrite();

                // Write data to the file stream.
                while (count > 0)
                {
                    // Determine how many bytes we can write to the buffer.
                    tempLen = this._bufferSize - this._bufferPosn;
                    if (tempLen <= 0)
                    {
                        var data = new byte[this._bufferPosn];

                        Buffer.BlockCopy(this._buffer, 0, data, 0, this._bufferPosn);

                        using (var wait = new AutoResetEvent(false))
                        {
                            this._session.RequestWrite(this._handle, this._serverFilePosition, data, wait);
                            this._serverFilePosition += (ulong)data.Length;
                        }

                        this._bufferPosn = 0;
                        tempLen = this._bufferSize;
                    }
                    if (tempLen > count)
                    {
                        tempLen = count;
                    }

                    // Can we short-cut the internal buffer?
                    if (this._bufferPosn == 0 && tempLen == this._bufferSize)
                    {
                        // Yes: write the data directly to the file.
                        var data = new byte[tempLen];

                        Buffer.BlockCopy(buffer, offset, data, 0, tempLen);

                        using (var wait = new AutoResetEvent(false))
                        {
                            this._session.RequestWrite(this._handle, this._serverFilePosition, data, wait);
                            this._serverFilePosition += (ulong)data.Length;
                        }
                    }
                    else
                    {
                        // No: copy the data to the write buffer first.
                        Buffer.BlockCopy(buffer, offset, this._buffer, this._bufferPosn, tempLen);
                        this._bufferPosn += tempLen;
                    }

                    // Advance the buffer and stream positions.
                    this._position += tempLen;
                    offset += tempLen;
                    count -= tempLen;
                }

                // If the buffer is full, then do a speculative flush now,
                // rather than waiting for the next call to this method.
                if (this._bufferPosn >= this._bufferSize)
                {
                    var data = new byte[this._bufferPosn];

                    Buffer.BlockCopy(this._buffer, 0, data, 0, this._bufferPosn);

                    using (var wait = new AutoResetEvent(false))
                    {
                        this._session.RequestWrite(this._handle, this._serverFilePosition, data, wait);
                        this._serverFilePosition += (ulong)data.Length;
                    }

                    this._bufferPosn = 0;
                }
            }
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void WriteByte(byte value)
        {
            // Lock down the file stream while we do this.
            lock (this._lock)
            {
                // Setup the object for writing.
                this.SetupWrite();

                // Flush the current buffer if it is full.
                if (this._bufferPosn >= this._bufferSize)
                {
                    var data = new byte[this._bufferPosn];

                    Buffer.BlockCopy(this._buffer, 0, data, 0, this._bufferPosn);

                    using (var wait = new AutoResetEvent(false))
                    {
                        this._session.RequestWrite(this._handle, this._serverFilePosition, data, wait);
                        this._serverFilePosition += (ulong)data.Length;
                    }

                    this._bufferPosn = 0;
                }

                // Write the byte into the buffer and advance the posn.
                this._buffer[this._bufferPosn++] = value;
                ++this._position;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._session != null)
            {
                lock (this._lock)
                {
                    if (this._session != null)
                    {
                        if (this._handle != null)
                        {
                            if (this._bufferOwnedByWrite)
                            {
                                this.FlushWriteBuffer();
                            }

                            if (this._ownsHandle)
                            {
                                this._session.RequestClose(this._handle);
                            }

                            this._handle = null;
                        }

                        this._session.Disconnected -= Session_Disconnected;
                        this._session = null;
                    }
                }
            }
        }

        /// <summary>
        /// Flushes the read data from the buffer.
        /// </summary>
        private void FlushReadBuffer()
        {
            if (this._canSeek)
            {
                if (this._bufferPosn < this._bufferLen)
                {
                    this._position -= this._bufferPosn;
                }
                this._bufferPosn = 0;
                this._bufferLen = 0;
            }
        }

        /// <summary>
        /// Flush any buffered write data to the file.
        /// </summary>
        private void FlushWriteBuffer()
        {
            if (this._bufferPosn > 0)
            {
                var data = new byte[this._bufferPosn];

                Buffer.BlockCopy(this._buffer, 0, data, 0, this._bufferPosn);

                using (var wait = new AutoResetEvent(false))
                {
                    this._session.RequestWrite(this._handle, this._serverFilePosition, data, wait);
                    this._serverFilePosition += (ulong)data.Length;
                }

                this._bufferPosn = 0;
            }
        }

        /// <summary>
        /// Setups the read.
        /// </summary>
        private void SetupRead()
        {
            if ((this._access & FileAccess.Read) == 0)
            {
                throw new NotSupportedException("Read not supported.");
            }
            if (this._handle == null)
            {
                throw new ObjectDisposedException("Stream is closed.");
            }
            if (this._bufferOwnedByWrite)
            {
                this.FlushWriteBuffer();
                this._bufferOwnedByWrite = false;
            }
        }

        /// <summary>
        /// Setups the write.
        /// </summary>
        private void SetupWrite()
        {
            if ((this._access & FileAccess.Write) == 0)
            {
                throw new NotSupportedException("Write not supported.");
            }
            if (this._handle == null)
            {
                throw new ObjectDisposedException("Stream is closed.");
            }
            if (!this._bufferOwnedByWrite)
            {
                this.FlushReadBuffer();
                this._bufferOwnedByWrite = true;
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            lock (this._lock)
            {
                this._session.Disconnected -= Session_Disconnected;
                this._session = null;
            }
        }
    }
}