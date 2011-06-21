using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet.Sftp;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public class SftpClient : BaseClient
    {
        /// <summary>
        /// Holds SftpSession instance that used to communicate to the SFTP server
        /// </summary>
        private SftpSession _sftpSession;

        /// <summary>
        /// Keeps track of all async command execution
        /// </summary>
        private Dictionary<SftpAsyncResult, SftpCommand> _asyncCommands = new Dictionary<SftpAsyncResult, SftpCommand>();

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>The operation timeout.</value>
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer.
        /// </summary>
        /// <value>The size of the buffer.</value>
        public uint BufferSize { get; set; }

        /// <summary>
        /// Gets remote working directory.
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                if (this._sftpSession == null)
                    return null;
                return this._sftpSession.WorkingDirectory;
            }
        }

        /// <summary>
        /// Gets sftp protocol version.
        /// </summary>
        public int ProtocolVersion { get; private set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        public SftpClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
            this.OperationTimeout = new TimeSpan(0, 0, 0, 0, -1);
            this.BufferSize = 1024 * 32 - 38;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        public SftpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        public SftpClient(string host, string username, string password)
            : this(host, 22, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        public SftpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        public SftpClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, 22, username, keyFiles)
        {
        }

        #endregion

        /// <summary>
        /// Changes remote directory to path.
        /// </summary>
        /// <param name="path">New directory path.</param>
        public void ChangeDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            this._sftpSession.ChangeDirectory(path);
        }

        /// <summary>
        /// Changes permissions of file(s) to specified mode.
        /// </summary>
        /// <param name="path">File(s) path, may match multiple files.</param>
        /// <param name="mode">The mode.</param>
        public void ChangePermissions(string path, short mode)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var file = this.Get(path);

            file.SetPermissions(mode);
        }

        /// <summary>
        /// Creates remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException"></exception>
        /// <exception cref="Renci.SshNet.Common.SshException"></exception>
        public void CreateDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(path);

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            using (var cmd = new CreateDirectoryCommand(this._sftpSession, fullPath))
            {
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.Execute();
            }
        }

        /// <summary>
        /// Deletes remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory to be deleted path.</param>
        public void DeleteDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            using (var cmd = new RemoveDirectoryCommand(this._sftpSession, fullPath))
            {
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.Execute();
            }
        }

        /// <summary>
        /// Deletes remote file specified by path.
        /// </summary>
        /// <param name="path">File to be deleted path.</param>
        public void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            using (var cmd = new RemoveFileCommand(this._sftpSession, fullPath))
            {
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.Execute();
            }
        }

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        public void RenameFile(string oldPath, string newPath)
        {
            if (oldPath == null)
                throw new ArgumentNullException("oldPath");

            if (newPath == null)
                throw new ArgumentNullException("newPath");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var oldFullPath = this._sftpSession.GetCanonicalPath(oldPath);

            var newFullPath = this._sftpSession.GetCanonicalPath(newPath);

            using (var cmd = new RenameFileCommand(this._sftpSession, oldFullPath, newFullPath))
            {
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.Execute();
            }
        }

        /// <summary>
        /// Creates a symbolic link from old path to new path.
        /// </summary>
        /// <param name="path">The old path.</param>
        /// <param name="linkPath">The new path.</param>
        public void SymbolicLink(string path, string linkPath)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path");

            if (string.IsNullOrWhiteSpace(linkPath))
                throw new ArgumentException("linkPath");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var linkFullPath = this._sftpSession.GetCanonicalPath(linkPath);

            using (var cmd = new SymbolicLinkCommand(this._sftpSession, fullPath, linkFullPath))
            {
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.Execute();
            }
        }

        /// <summary>
        /// Retrieves list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>List of directory entries</returns>
        public IEnumerable<SftpFile> ListDirectory(string path)
        {
            return this.EndListDirectory(this.BeginListDirectory(path, null, null));
        }

        /// <summary>
        /// Begins an asynchronous operation of retrieving list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback, object state)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var cmd = new ListDirectoryCommand(this._sftpSession, fullPath);

            cmd.CommandTimeout = this.OperationTimeout;

            var async = cmd.BeginExecute(asyncCallback, state);

            lock (this._asyncCommands)
            {
                this._asyncCommands.Add(async, cmd);
            }

            return async;
        }

        /// <summary>
        /// Ends an asynchronous operation of retrieving list of files in remote directory.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <returns>List of files</returns>
        public IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult)
        {
            var sftpAsync = asyncResult as SftpAsyncResult;

            if (this._asyncCommands.ContainsKey(sftpAsync))
            {
                lock (this._asyncCommands)
                {
                    if (this._asyncCommands.ContainsKey(sftpAsync))
                    {
                        var cmd = this._asyncCommands[sftpAsync] as ListDirectoryCommand;

                        if (cmd != null)
                        {
                            try
                            {
                                this._asyncCommands.Remove(sftpAsync);

                                cmd.EndExecute(sftpAsync);

                                var files = cmd.Files;

                                return files;
                            }
                            finally
                            {
                                cmd.Dispose();
                            }
                        }
                    }
                }
            }

            throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndListDirectory was called multiple times with the same IAsyncResult.");
        }

        /// <summary>
        /// Gets reference to remote file or directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public SftpFile Get(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            using (var cmd = new StatusCommand(this._sftpSession, fullPath))
            {
                cmd.CommandTimeout = this.OperationTimeout;

                cmd.Execute();

                if (cmd.Attributes == null)
                {
                    return null;
                }
                else
                {
                    return new SftpFile(this._sftpSession, fullPath, cmd.Attributes);
                }
            }
        }

        /// <summary>
        /// Downloads remote file specified by the path into the stream.
        /// </summary>
        /// <param name="path">File to download.</param>
        /// <param name="output">Stream to write the file into.</param>
        public void DownloadFile(string path, Stream output)
        {
            this.EndDownloadFile(this.BeginDownloadFile(path, output, null, null));
        }

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback, object state)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path");

            if (output == null)
                throw new ArgumentNullException("output");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var cmd = new DownloadFileCommand(this._sftpSession, this.BufferSize, fullPath, output);

            cmd.CommandTimeout = this.OperationTimeout;

            var async = cmd.BeginExecute(asyncCallback, state);

            lock (this._asyncCommands)
            {
                this._asyncCommands.Add(async, cmd);
            }

            return async;
        }

        /// <summary>
        /// Ends an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        public void EndDownloadFile(IAsyncResult asyncResult)
        {
            var sftpAsync = asyncResult as SftpAsyncResult;

            if (this._asyncCommands.ContainsKey(sftpAsync))
            {
                lock (this._asyncCommands)
                {
                    if (this._asyncCommands.ContainsKey(sftpAsync))
                    {
                        var cmd = this._asyncCommands[sftpAsync] as DownloadFileCommand;

                        if (cmd != null)
                        {
                            try
                            {
                                this._asyncCommands.Remove(sftpAsync);

                                cmd.EndExecute(sftpAsync);

                                return;
                            }
                            finally
                            {
                                cmd.Dispose();
                            }
                        }
                    }
                }
            }

            throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndDownloadFile was called multiple times with the same IAsyncResult.");
        }

        /// <summary>
        /// Uploads stream into remote file..
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        public void UploadFile(Stream input, string path)
        {
            this.EndUploadFile(this.BeginUploadFile(input, path, null, null));
        }

        /// <summary>
        /// Begins an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback, object state)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var cmd = new UploadFileCommand(this._sftpSession, this.BufferSize, fullPath, input);

            cmd.CommandTimeout = this.OperationTimeout;

            var async = cmd.BeginExecute(asyncCallback, state);

            lock (this._asyncCommands)
            {
                this._asyncCommands.Add(async, cmd);
            }

            return async;
        }

        /// <summary>
        /// Ends an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        public void EndUploadFile(IAsyncResult asyncResult)
        {
            var sftpAsync = asyncResult as SftpAsyncResult;

            if (this._asyncCommands.ContainsKey(sftpAsync))
            {
                lock (this._asyncCommands)
                {
                    if (this._asyncCommands.ContainsKey(sftpAsync))
                    {
                        var cmd = this._asyncCommands[sftpAsync] as UploadFileCommand;

                        if (cmd != null)
                        {
                            try
                            {
                                this._asyncCommands.Remove(sftpAsync);

                                cmd.EndExecute(sftpAsync);

                                return;
                            }
                            finally
                            {
                                cmd.Dispose();
                            }
                        }
                    }
                }
            }

            throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndUploadFile was called multiple times with the same IAsyncResult.");
        }

        #region File Methods

        /// <summary>
        /// Appends lines to a file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            using (var stream = this.AppendText(path))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Appends lines to a file by using a specified encoding, and then closes the file.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            using (var stream = this.AppendText(path, encoding))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        ///  Opens a file, appends the specified string to the file, and then closes the file. 
        ///  If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        public void AppendAllText(string path, string contents)
        {
            using (var stream = this.AppendText(path))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file.
        /// If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void AppendAllText(string path, string contents, Encoding encoding)
        {
            using (var stream = this.AppendText(path, encoding))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Creates a <see cref="System.IO.StreamWriter"/> that appends UTF-8 encoded text to an existing file.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <returns>A StreamWriter that appends UTF-8 encoded text to an existing file.</returns>
        public StreamWriter AppendText(string path)
        {
            return this.AppendText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Creates a <see cref="System.IO.StreamWriter"/> that appends UTF-8 encoded text to an existing file.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>
        /// A StreamWriter that appends UTF-8 encoded text to an existing file.
        /// </returns>
        public StreamWriter AppendText(string path, Encoding encoding)
        {
            return new StreamWriter(new SftpFileStream(this._sftpSession, path, FileMode.Append, FileAccess.Write), encoding);
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path</returns>
        public SftpFileStream Create(string path)
        {
            return new SftpFileStream(this._sftpSession, path, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Creates or overwrites the specified file.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
        /// <returns>A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path</returns>
        public SftpFileStream Create(string path, int bufferSize)
        {
            return new SftpFileStream(this._sftpSession, path, FileMode.Create, FileAccess.ReadWrite, bufferSize);
        }

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>A <see cref="System.IO.StreamWriter"/> that writes to the specified file using UTF-8 encoding.</returns>
        public StreamWriter CreateText(string path)
        {
            return new StreamWriter(this.OpenWrite(path), Encoding.UTF8);
        }

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns> A <see cref="System.IO.StreamWriter"/> that writes to the specified file using UTF-8 encoding. </returns>
        public StreamWriter CreateText(string path, Encoding encoding)
        {
            return new StreamWriter(this.OpenWrite(path), encoding);
        }

        /// <summary>
        /// Deletes the specified file or directory. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">The name of the file or directory to be deleted. Wildcard characters are not supported.</param>
        public void Delete(string path)
        {
            var file = this.Get(path);

            if (file == null)
            {
                throw new SftpPathNotFoundException(path);
            }

            file.Delete();
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns><c>true</c> if path contains the name of an existing file; otherwise, <c>false</c>.</returns>
        public bool Exists(string path)
        {
            var file = this.Get(path);

            return file != null;
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>A <see cref="System.DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
        public DateTime GetLastAccessTime(string path)
        {
            var file = this.Get(path);
            return file.LastAccessTime;
        }

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>A <see cref="System.DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
        public DateTime GetLastAccessTimeUtc(string path)
        {
            var file = this.Get(path);
            return file.LastAccessTime.ToUniversalTime();
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>A <see cref="System.DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
        public DateTime GetLastWriteTime(string path)
        {
            var file = this.Get(path);
            return file.LastWriteTime;
        }

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>A <see cref="System.DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in UTC time.</returns>
        public DateTime GetLastWriteTimeUtc(string path)
        {
            var file = this.Get(path);
            return file.LastWriteTime.ToUniversalTime();
        }

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path with read/write access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="System.IO.FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <returns>An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and access.</returns>
        public SftpFileStream Open(string path, FileMode mode)
        {
            return new SftpFileStream(this._sftpSession, path, mode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="System.IO.FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="System.IO.FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and access.</returns>
        public SftpFileStream Open(string path, FileMode mode, FileAccess access)
        {
            return new SftpFileStream(this._sftpSession, path, mode, access);
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A read-only System.IO.FileStream on the specified path.</returns>
        public SftpFileStream OpenRead(string path)
        {
            return new SftpFileStream(this._sftpSession, path, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A <see cref="System.IO.StreamReader"/> on the specified path.</returns>
        public StreamReader OpenText(string path)
        {
            return new StreamReader(this.OpenRead(path), Encoding.UTF8);
        }

        /// <summary>
        /// Opens an existing file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>An unshared <see cref="SftpFileStream"/> object on the specified path with <see cref="System.IO.FileAccess.Write"/> access.</returns>
        public SftpFileStream OpenWrite(string path)
        {
            return new SftpFileStream(this._sftpSession, path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public byte[] ReadAllBytes(string path)
        {
            using (var stream = this.OpenRead(path))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public string[] ReadAllLines(string path)
        {
            return this.ReadAllLines(path, Encoding.UTF8);
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public string[] ReadAllLines(string path, Encoding encoding)
        {
            var lines = new List<string>();
            using (var stream = new StreamReader(this.OpenRead(path), encoding))
            {
                while (!stream.EndOfStream)
                {
                    lines.Add(stream.ReadLine());
                }
            }
            return lines.ToArray();
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public string ReadAllText(string path)
        {
            return this.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public string ReadAllText(string path, Encoding encoding)
        {
            var lines = new List<string>();
            using (var stream = new StreamReader(this.OpenRead(path), encoding))
            {
                return stream.ReadToEnd();
            }
        }

        /// <summary>
        /// Reads the lines of a file.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <returns>The lines of the file.</returns>
        public IEnumerable<string> ReadLines(string path)
        {
            return this.ReadAllLines(path);
        }

        /// <summary>
        /// Read the lines of a file that has a specified encoding.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
        /// <returns>The lines of the file.</returns>
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            return this.ReadAllLines(path, encoding);
        }

        /// <summary>
        /// Sets the date and time the specified file was last accessed.
        /// </summary>
        /// <param name="path">The file for which to set the access date and time information.</param>
        /// <param name="lastAccessTime">A <see cref="System.DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in local time.</param>
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last accessed.
        /// </summary>
        /// <param name="path">The file for which to set the access date and time information.</param>
        /// <param name="lastAccessTimeUtc">A <see cref="System.DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in UTC time.</param>
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTime">A System.DateTime containing the value to set for the last write date and time of path. This value is expressed in local time.</param>
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTimeUtc">A System.DateTime containing the value to set for the last write date and time of path. This value is expressed in UTC time.</param>
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public void WriteAllBytes(string path, byte[] bytes)
        {
            using (var stream = this.OpenWrite(path))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Creates a new file, writes a collection of strings to the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            this.WriteAllLines(path, contents, Encoding.UTF8);
        }

        /// <summary>
        /// Creates a new file, write the specified string array to the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        public void WriteAllLines(string path, string[] contents)
        {
            this.WriteAllLines(path, contents, Encoding.UTF8);
        }

        /// <summary>
        /// Creates a new file by using the specified encoding, writes a collection of strings to the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            using (var stream = this.CreateText(path, encoding))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified string array to the file by using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <param name="encoding">An <see cref="System.Text.Encoding"/> object that represents the character encoding applied to the string array.</param>
        public void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            using (var stream = this.CreateText(path, encoding))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public void WriteAllText(string path, string contents)
        {
            using (var stream = this.CreateText(path))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            using (var stream = this.CreateText(path, encoding))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Gets the <see cref="SftpFileAttributes"/> of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The <see cref="SftpFileAttributes"/> of the file on the path.</returns>
        public SftpFileAttributes GetAttributes(string path)
        {
            var fullPath = this._sftpSession.GetCanonicalPath(path);

            return this._sftpSession.GetFileAttributes(fullPath);
        }

        /// <summary>
        /// Sets the specified <see cref="SftpFileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">The desired <see cref="SftpFileAttributes"/>.</param>
        public void SetAttributes(string path, SftpFileAttributes fileAttributes)
        {
            var fullPath = this._sftpSession.GetCanonicalPath(path);

            this._sftpSession.SetFileAttributes(fullPath, fileAttributes);
        }

        //public FileSecurity GetAccessControl(string path);
        //public FileSecurity GetAccessControl(string path, AccessControlSections includeSections);
        //public DateTime GetCreationTime(string path);
        //public DateTime GetCreationTimeUtc(string path);
        //public void SetAccessControl(string path, FileSecurity fileSecurity);
        //public void SetCreationTime(string path, DateTime creationTime);
        //public void SetCreationTimeUtc(string path, DateTime creationTimeUtc);

        #endregion

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            this._sftpSession = new SftpSession(this.Session, this.OperationTimeout);

            this._sftpSession.Connect();

            //  Resolve current running version
            this.ProtocolVersion = this._sftpSession.ProtocolVersion;
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            this._sftpSession.Disconnect();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected override void Dispose(bool disposing)
        {
            if (this._sftpSession != null)
            {
                this._sftpSession.Dispose();
                this._sftpSession = null;
            }

            if (this._asyncCommands != null)
            {
                foreach (var command in this._asyncCommands.Values)
                {
                    command.Dispose();
                }

                this._asyncCommands = null;
            }

            base.Dispose(disposing);
        }
    }
}