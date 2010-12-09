using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshClient.Sftp;

namespace Renci.SshClient
{
    /// <summary>
    /// 
    /// </summary>
    public class SftpClient
    {
        private SftpSession _sftpSession;

        private Session _session;

        /// <summary>
        /// Gets the connection info.
        /// </summary>
        /// <value>The connection info.</value>
        public ConnectionInfo ConnectionInfo { get; private set; }

        /// <summary>
        /// Gets a value indicating whether client is connected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if client is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                if (this._session == null)
                {
                    return false;
                }
                else
                {
                    return this._session.IsConnected;
                }
            }
        }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>The operation timeout.</value>
        public int OperationTimeout { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer.
        /// </summary>
        /// <value>The size of the buffer.</value>
        public uint BufferSize { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        public SftpClient(ConnectionInfo connectionInfo)
        {
            this.OperationTimeout = -1;
            this.BufferSize = 1024 * 16;
            this.ConnectionInfo = connectionInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public SftpClient(string host, int port, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public SftpClient(string host, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                Password = password,
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        /// <param name="keyFile">The key file.</param>
        public SftpClient(string host, int port, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="keyFile">The key file.</param>
        public SftpClient(string host, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        #endregion

        /// <summary>
        /// Connects client to the server.
        /// </summary>
        public void Connect()
        {
            if (this._session == null)
            {
                this._session = new Session(this.ConnectionInfo);
            }
            this._session.Connect();

            this._sftpSession = new SftpSession(this._session, this.OperationTimeout);

            this._sftpSession.Connect();
        }

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        public void Disconnect()
        {
            this._sftpSession.Disconnect();

            this._session.Disconnect();

            this._session = null;
        }

        #region List Directory

        /// <summary>
        /// Begins the list directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback, object state)
        {
            var cmd = new ListDirectoryCommand(this._sftpSession, path);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the list directory.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        /// <returns></returns>
        public IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<ListDirectoryCommand>();

            cmd.EndExecute(sftpAsyncResult);

            return cmd.Files;
        }

        /// <summary>
        /// Lists the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public IEnumerable<SftpFile> ListDirectory(string path)
        {
            return this.EndListDirectory(this.BeginListDirectory(path, null, null));
        }

        #endregion

        #region Rename file

        /// <summary>
        /// Begins the rename file.
        /// </summary>
        /// <param name="oldFilename">The old filename.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRenameFile(string oldFilename, string newFileName, AsyncCallback asyncCallback, object state)
        {
            var cmd = new RenameFileCommand(this._sftpSession, oldFilename, newFileName);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the rename file.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void EndRenameFile(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<RenameFileCommand>();

            cmd.EndExecute(sftpAsyncResult);
        }

        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="oldFilename">The old filename.</param>
        /// <param name="newFileName">New name of the file.</param>
        public void RenameFile(string oldFilename, string newFileName)
        {
            this.EndRenameFile(this.BeginRenameFile(oldFilename, newFileName, null, null));
        }

        #endregion

        #region Remove Directory

        /// <summary>
        /// Begins the remove directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRemoveDirectory(string path, AsyncCallback asyncCallback, object state)
        {
            var cmd = new RemoveDirectoryCommand(this._sftpSession, path);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the remove directory.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void EndRemoveDirectory(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<RemoveDirectoryCommand>();

            cmd.EndExecute(sftpAsyncResult);
        }

        /// <summary>
        /// Removes the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RemoveDirectory(string path)
        {
            this.EndRemoveDirectory(this.BeginRemoveDirectory(path, null, null));
        }

        #endregion

        #region Create Directory

        /// <summary>
        /// Begins the create directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginCreateDirectory(string path, AsyncCallback asyncCallback, object state)
        {
            var cmd = new CreateDirectoryCommand(this._sftpSession, path);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the create directory.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void EndCreateDirectory(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<CreateDirectoryCommand>();

            cmd.EndExecute(sftpAsyncResult);
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        public void CreateDirectory(string path)
        {
            this.EndCreateDirectory(this.BeginCreateDirectory(path, null, null));
        }

        #endregion

        #region Remove File

        /// <summary>
        /// Begins the remove file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRemoveFile(string filename, AsyncCallback asyncCallback, object state)
        {
            var cmd = new RemoveFileCommand(this._sftpSession, filename);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the remove file.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void EndRemoveFile(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<RemoveFileCommand>();

            cmd.EndExecute(sftpAsyncResult);
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void RemoveFile(string filename)
        {
            this.EndRemoveFile(this.BeginRemoveFile(filename, null, null));
        }

        #endregion

        #region Upload File

        /// <summary>
        /// Begins the upload file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="input">The input.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginUploadFile(string filename, Stream input, AsyncCallback asyncCallback, object state)
        {
            var cmd = new UploadFileCommand(this._sftpSession, this.BufferSize, filename, input);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the upload file.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void EndUploadFile(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<UploadFileCommand>();

            cmd.EndExecute(sftpAsyncResult);
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="input">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void UploadFile(string filename, Stream input)
        {
            this.EndUploadFile(this.BeginUploadFile(filename, input, null, null));
        }

        #endregion

        #region Download File

        /// <summary>
        /// Begins the download.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownload(string filename, Stream output, AsyncCallback asyncCallback, object state)
        {
            var cmd = new DownloadFileCommand(this._sftpSession, this.BufferSize, filename, output);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the download.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        public void EndDownload(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<DownloadFileCommand>();

            cmd.EndExecute(sftpAsyncResult);
        }

        /// <summary>
        /// Downloads the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="output">The output.</param>
        public void Download(string filename, Stream output)
        {
            this.EndDownload(this.BeginDownload(filename, output, null, null));
        }

        #endregion

        #region Get Real Path

        /// <summary>
        /// Begins the get real path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
        public IAsyncResult BeginGetRealPath(string path, AsyncCallback asyncCallback, object state)
        {
            var cmd = new RealPathCommand(this._sftpSession, path);

            cmd.CommandTimeout = this.OperationTimeout;

            return cmd.BeginExecute(asyncCallback, state);
        }

        /// <summary>
        /// Ends the get real path.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous operation.</param>
        /// <returns></returns>
        public IEnumerable<SftpFile> EndGetRealPath(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            var cmd = sftpAsyncResult.GetCommand<RealPathCommand>();

            cmd.EndExecute(sftpAsyncResult);

            return cmd.Files;
        }

        /// <summary>
        /// Gets the real path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public IEnumerable<SftpFile> GetRealPath(string path)
        {
            return this.EndGetRealPath(this.BeginGetRealPath(path, null, null));
        }

        #endregion
    }
}