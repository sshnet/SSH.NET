using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet.Sftp;

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
        public void ChangePermissions(string path, ushort mode)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            //  Ensure that connection is established.
            this.EnsureConnection();

            var file = this._sftpSession.GetSftpFile(path);

            file.SetPermissions(mode);
        }

        /// <summary>
        /// Creates remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="Renci.SshNet.Common.SshPermissionDeniedException"></exception>
        /// <exception cref="Renci.SshNet.Common.SshException"></exception>
        public void CreateDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(path);

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var cmd = new CreateDirectoryCommand(this._sftpSession, fullPath);

            cmd.CommandTimeout = this.OperationTimeout;

            cmd.Execute();
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

            //  Ensure that connection is established.
            this.EnsureConnection();

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var cmd = new StatusCommand(this._sftpSession, fullPath);

            cmd.CommandTimeout = this.OperationTimeout;

            cmd.Execute();

            return cmd.File;
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