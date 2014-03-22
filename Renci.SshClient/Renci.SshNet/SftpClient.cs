using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet.Sftp;
using System.Text;
using Renci.SshNet.Common;
using System.Globalization;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClient : BaseClient
    {
        /// <summary>
        /// Holds the <see cref="SftpSession"/> instance that used to communicate to the
        /// SFTP server.
        /// </summary>
        private SftpSession _sftpSession;

        /// <summary>
        /// Holds the operation timeout.
        /// </summary>
        private TimeSpan _operationTimeout;

        /// <summary>
        /// Holds the size of the buffer.
        /// </summary>
        private uint _bufferSize;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite time-out period.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public TimeSpan OperationTimeout
        {
            get
            {
                CheckDisposed();
                return _operationTimeout;
            }
            set
            {
                CheckDisposed();
                _operationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size of the buffer in bytes.
        /// </summary>
        /// <value>
        /// The size of the buffer. The default buffer size is 65536 bytes (64 KB).
        /// </value>
        /// <remarks>
        /// <para>
        /// For write operations, this limits the size of the payload for
        /// individual SSH_FXP_WRITE messages. The actual size is always
        /// capped at the maximum packet size supported by the peer
        /// (minus the size of protocol fields).
        /// </para>
        /// <para>
        /// For read operations, this controls the size of the payload which
        /// is requested from the peer in each SSH_FXP_READ message. The peer
        /// will send the requested number of bytes in one or more SSH_FXP_DATA
        /// messages. To optimize the size of the SSH packets sent by the peer,
        /// the actual requested size will take into account the size of the
        /// SSH_FXP_DATA protocol fields.
        /// </para>
        /// <para>
        /// The size of the each indivual SSH_FXP_DATA message is limited to the
        /// local maximum packet size of the channel, which is set to <c>64 KB</c>
        /// for SSH.NET. However, the peer can limit this even further.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public uint BufferSize
        {
            get
            {
                CheckDisposed();
                return _bufferSize;
            }
            set
            {
                CheckDisposed();
                _bufferSize = value;
            }
        }

        /// <summary>
        /// Gets remote working directory.
        /// </summary>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public string WorkingDirectory
        {
            get
            {
                CheckDisposed();
                if (_sftpSession == null)
                    throw new SshConnectionException("Client not connected.");
                return _sftpSession.WorkingDirectory;
            }
        }

        /// <summary>
        /// Gets sftp protocol version.
        /// </summary>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public int ProtocolVersion
        {
            get
            {
                CheckDisposed();
                if (_sftpSession == null)
                    throw new SshConnectionException("Client not connected.");
                return (int) _sftpSession.ProtocolVersion;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <b>null</b>.</exception>
        public SftpClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public SftpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is <b>null</b> contains whitespace characters.</exception>
        public SftpClient(string host, string username, string password)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is nu<b>null</b>ll or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public SftpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is <b>null</b> or contains whitespace characters.</exception>
        public SftpClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        private SftpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
            : base(connectionInfo, ownsConnectionInfo)
        {
            this.OperationTimeout = new TimeSpan(0, 0, 0, 0, -1);
            this.BufferSize = 1024 * 64;
        }

        #endregion

        /// <summary>
        /// Changes remote directory to path.
        /// </summary>
        /// <param name="path">New directory path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change directory denied by remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException">The path in <paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void ChangeDirectory(string path)
        {
            CheckDisposed();

            if (path == null)
                throw new ArgumentNullException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            this._sftpSession.ChangeDirectory(path);
        }

        /// <summary>
        /// Changes permissions of file(s) to specified mode.
        /// </summary>
        /// <param name="path">File(s) path, may match multiple files.</param>
        /// <param name="mode">The mode.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change permission on the path(s) was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException">The path in <paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void ChangePermissions(string path, short mode)
        {
            var file = this.Get(path);
            file.SetPermissions(mode);
        }

        /// <summary>
        /// Creates remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to create the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void CreateDirectory(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException(path);

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            this._sftpSession.RequestMkDir(fullPath);
        }

        /// <summary>
        /// Deletes remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory to be deleted path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to delete the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void DeleteDirectory(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            this._sftpSession.RequestRmDir(fullPath);
        }

        /// <summary>
        /// Deletes remote file specified by path.
        /// </summary>
        /// <param name="path">File to be deleted path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to delete the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void DeleteFile(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            this._sftpSession.RequestRemove(fullPath);
        }

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        /// <exception cref="ArgumentNullException"><paramref name="oldPath"/> is <b>null</b>. <para>-or-</para> or <paramref name="newPath"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to rename the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void RenameFile(string oldPath, string newPath)
        {
            this.RenameFile(oldPath, newPath, false);
        }

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        /// <param name="isPosix">if set to <c>true</c> then perform a posix rename.</param>
        /// <exception cref="ArgumentNullException"><paramref name="oldPath" /> is <b>null</b>. <para>-or-</para> or <paramref name="newPath" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to rename the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void RenameFile(string oldPath, string newPath, bool isPosix)
        {
            CheckDisposed();

            if (oldPath == null)
                throw new ArgumentNullException("oldPath");

            if (newPath == null)
                throw new ArgumentNullException("newPath");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var oldFullPath = this._sftpSession.GetCanonicalPath(oldPath);

            var newFullPath = this._sftpSession.GetCanonicalPath(newPath);

            if (isPosix)
            {
                this._sftpSession.RequestPosixRename(oldFullPath, newFullPath);
            }
            else
            {
                this._sftpSession.RequestRename(oldFullPath, newFullPath);
            }
        }

        /// <summary>
        /// Creates a symbolic link from old path to new path.
        /// </summary>
        /// <param name="path">The old path.</param>
        /// <param name="linkPath">The new path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="linkPath"/> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to create the symbolic link was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void SymbolicLink(string path, string linkPath)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (linkPath.IsNullOrWhiteSpace())
                throw new ArgumentException("linkPath");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var linkFullPath = this._sftpSession.GetCanonicalPath(linkPath);

            this._sftpSession.RequestSymLink(fullPath, linkFullPath);
        }

        /// <summary>
        /// Retrieves list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// List of directory entries
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public IEnumerable<SftpFile> ListDirectory(string path, Action<int> listCallback = null)
        {
            CheckDisposed();

            return InternalListDirectory(path, listCallback);
        }

        /// <summary>
        /// Begins an asynchronous operation of retrieving list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback, object state, Action<int> listCallback = null)
        {
            CheckDisposed();

            var asyncResult = new SftpListDirectoryAsyncResult(asyncCallback, state);

            this.ExecuteThread(() =>
            {
                try
                {
                    var result = this.InternalListDirectory(path, count =>
                    {
                        asyncResult.Update(count);

                        if (listCallback != null)
                        {
                            listCallback(count);
                        }
                    });

                    asyncResult.SetAsCompleted(result, false);
                }
                catch (Exception exp)
                {
                    asyncResult.SetAsCompleted(exp, false);
                }
            });

            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation of retrieving list of files in remote directory.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <returns>
        /// List of files
        /// </returns>
        /// <exception cref="ArgumentException">The IAsyncResult object (<paramref name="asyncResult"/>) did not come from the corresponding async method on this type. <para>-or-</para> EndExecute was called multiple times with the same IAsyncResult.</exception>
        public IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpListDirectoryAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            return ar.EndInvoke();
        }

        /// <summary>
        /// Gets reference to remote file or directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Reference to <see cref="Renci.SshNet.Sftp.SftpFile"/> file object.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFile Get(string path)
        {
            CheckDisposed();

            if (path == null)
                throw new ArgumentNullException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var attributes = this._sftpSession.RequestLStat(fullPath);

            return new SftpFile(this._sftpSession, fullPath, attributes);
        }

        /// <summary>
        /// Checks whether file pr directory exists;
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if directory or file exists; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public bool Exists(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetFullRemotePath(path);

            if (this._sftpSession.RequestRealPath(fullPath, true) == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Downloads remote file specified by the path into the stream.
        /// </summary>
        /// <param name="path">File to download.</param>
        /// <param name="output">Stream to write the file into.</param>
        /// <param name="downloadCallback">The download callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null)
        {
            CheckDisposed();

            this.InternalDownloadFile(path, output, null, downloadCallback);
        }

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginDownloadFile(string path, Stream output)
        {
            return this.BeginDownloadFile(path, output, null, null, null);
        }

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback)
        {
            return this.BeginDownloadFile(path, output, asyncCallback, null, null);
        }

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="downloadCallback">The download callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback, object state, Action<ulong> downloadCallback = null)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (output == null)
                throw new ArgumentNullException("output");

            var asyncResult = new SftpDownloadAsyncResult(asyncCallback, state);

            this.ExecuteThread(() =>
            {
                try
                {
                    this.InternalDownloadFile(path, output, asyncResult, offset =>
                    {
                        asyncResult.Update(offset);

                        if (downloadCallback != null)
                        {
                            downloadCallback(offset);
                        }
                    });

                    asyncResult.SetAsCompleted(null, false);
                }
                catch (Exception exp)
                {
                    asyncResult.SetAsCompleted(exp, false);
                }
            });

            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <exception cref="ArgumentException">The IAsyncResult object (<paramref name="asyncResult"/>) did not come from the corresponding async method on this type. <para>-or-</para> EndExecute was called multiple times with the same IAsyncResult.</exception>
        public void EndDownloadFile(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpDownloadAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
        }

        /// <summary>
        /// Uploads stream into remote file..
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null)
        {
            this.UploadFile(input, path, true, uploadCallback);
        }

        /// <summary>
        /// Uploads stream into remote file..
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">if set to <c>true</c> then existing file will be overwritten.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public void UploadFile(Stream input, string path, bool canOverride, Action<ulong> uploadCallback = null)
        {
            CheckDisposed();

            var flags = Flags.Write | Flags.Truncate;

            if (canOverride)
                flags |= Flags.CreateNewOrOpen;
            else
                flags |= Flags.CreateNew;

            this.InternalUploadFile(input, path, flags, null, uploadCallback);
        }

        /// <summary>
        /// Begins an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path)
        {
            return this.BeginUploadFile(input, path, true, null, null, null);
        }

        /// <summary>
        /// Begins an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback)
        {
            return this.BeginUploadFile(input, path, true, asyncCallback, null, null);
        }

        /// <summary>
        /// Begins an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null)
        {
            return this.BeginUploadFile(input, path, true, asyncCallback, state, uploadCallback);
        }

        /// <summary>
        /// Begins an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">if set to <c>true</c> then existing file will be overwritten.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">A SSH error where <see cref="P:System.Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path, bool canOverride, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null)
        {
            CheckDisposed();

            if (input == null)
                throw new ArgumentNullException("input");

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            var flags = Flags.Write | Flags.Truncate;

            if (canOverride)
                flags |= Flags.CreateNewOrOpen;
            else
                flags |= Flags.CreateNew;

            var asyncResult = new SftpUploadAsyncResult(asyncCallback, state);

            this.ExecuteThread(() =>
            {
                try
                {
                    this.InternalUploadFile(input, path, flags, asyncResult, offset =>
                    {
                        asyncResult.Update(offset);

                        if (uploadCallback != null)
                        {
                            uploadCallback(offset);
                        }

                    });

                    asyncResult.SetAsCompleted(null, false);
                }
                catch (Exception exp)
                {
                    asyncResult.SetAsCompleted(exp, false);
                }
            });

            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous uploading the steam into remote file.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <exception cref="System.ArgumentException">Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.</exception>
        /// <exception cref="ArgumentException">The IAsyncResult object (<paramref name="asyncResult" />) did not come from the corresponding async method on this type. <para>-or-</para> EndExecute was called multiple times with the same IAsyncResult.</exception>
        public void EndUploadFile(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpUploadAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
        }

        /// <summary>
        /// Gets status using statvfs@openssh.com request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Reference to <see cref="Renci.SshNet.Sftp.SftpFileSytemInformation"/> object that contains file status information.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileSytemInformation GetStatus(string path)
        {
            CheckDisposed();

            if (path == null)
                throw new ArgumentNullException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            return this._sftpSession.RequestStatVfs(fullPath);
        }

        #region File Methods

        /// <summary>
        /// Appends lines to a file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is<b>null</b> <para>-or-</para> <paramref name="contents"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            CheckDisposed();

            if (contents == null)
                throw new ArgumentNullException("contents");

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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            CheckDisposed();

            if (contents == null)
                throw new ArgumentNullException("contents");

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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamWriter AppendText(string path, Encoding encoding)
        {
            CheckDisposed();

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return new StreamWriter(new SftpFileStream(this._sftpSession, path, FileMode.Append, FileAccess.Write), encoding);
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream Create(string path)
        {
            CheckDisposed();

            return new SftpFileStream(this._sftpSession, path, FileMode.Create, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Creates or overwrites the specified file.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
        /// <returns>A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream Create(string path, int bufferSize)
        {
            CheckDisposed();

            return new SftpFileStream(this._sftpSession, path, FileMode.Create, FileAccess.ReadWrite, bufferSize);
        }

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>A <see cref="System.IO.StreamWriter"/> that writes to the specified file using UTF-8 encoding.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamWriter CreateText(string path)
        {
            return CreateText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns> A <see cref="System.IO.StreamWriter"/> that writes to the specified file using UTF-8 encoding. </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamWriter CreateText(string path, Encoding encoding)
        {
            CheckDisposed();

            return new StreamWriter(this.OpenWrite(path), encoding);
        }

        /// <summary>
        /// Deletes the specified file or directory. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">The name of the file or directory to be deleted. Wildcard characters are not supported.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void Delete(string path)
        {
            var file = this.Get(path);
            file.Delete();
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>A <see cref="System.DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public DateTime GetLastAccessTimeUtc(string path)
        {
            var lastAccessTime = GetLastAccessTime(path);
            return lastAccessTime.ToUniversalTime();
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>A <see cref="System.DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public DateTime GetLastWriteTimeUtc(string path)
        {
            var lastWriteTime = GetLastWriteTime(path);
            return lastWriteTime.ToUniversalTime();
        }

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path with read/write access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="System.IO.FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <returns>An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and access.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream Open(string path, FileMode mode)
        {
            return Open(path, mode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="System.IO.FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="System.IO.FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and access.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream Open(string path, FileMode mode, FileAccess access)
        {
            CheckDisposed();

            return new SftpFileStream(this._sftpSession, path, mode, access, (int) _bufferSize);
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A read-only System.IO.FileStream on the specified path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream OpenRead(string path)
        {
            return Open(path, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A <see cref="System.IO.StreamReader"/> on the specified path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamReader OpenText(string path)
        {
            return new StreamReader(this.OpenRead(path), Encoding.UTF8);
        }

        /// <summary>
        /// Opens an existing file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>An unshared <see cref="SftpFileStream"/> object on the specified path with <see cref="System.IO.FileAccess.Write"/> access.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream OpenWrite(string path)
        {
            CheckDisposed();

            return new SftpFileStream(this._sftpSession, path, FileMode.OpenOrCreate, FileAccess.Write,
                (int) _bufferSize);
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public string ReadAllText(string path, Encoding encoding)
        {
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            return this.ReadAllLines(path, encoding);
        }

        /// <summary>
        /// Sets the date and time the specified file was last accessed.
        /// </summary>
        /// <param name="path">The file for which to set the access date and time information.</param>
        /// <param name="lastAccessTime">A <see cref="System.DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in local time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last accessed.
        /// </summary>
        /// <param name="path">The file for which to set the access date and time information.</param>
        /// <param name="lastAccessTimeUtc">A <see cref="System.DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in UTC time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTime">A System.DateTime containing the value to set for the last write date and time of path. This value is expressed in local time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTimeUtc">A System.DateTime containing the value to set for the last write date and time of path. This value is expressed in UTC time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            this.WriteAllLines(path, contents, Encoding.UTF8);
        }

        /// <summary>
        /// Creates a new file, write the specified string array to the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileAttributes GetAttributes(string path)
        {
            CheckDisposed();

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            return this._sftpSession.RequestLStat(fullPath);
        }

        /// <summary>
        /// Sets the specified <see cref="SftpFileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">The desired <see cref="SftpFileAttributes"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void SetAttributes(string path, SftpFileAttributes fileAttributes)
        {
            CheckDisposed();

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            this._sftpSession.RequestSetStat(fullPath, fileAttributes);
        }

        // Please don't forget this when you implement these methods: <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        //public FileSecurity GetAccessControl(string path);
        //public FileSecurity GetAccessControl(string path, AccessControlSections includeSections);
        //public DateTime GetCreationTime(string path);
        //public DateTime GetCreationTimeUtc(string path);
        //public void SetAccessControl(string path, FileSecurity fileSecurity);
        //public void SetCreationTime(string path, DateTime creationTime);
        //public void SetCreationTimeUtc(string path, DateTime creationTimeUtc);

        #endregion

        /// <summary>
        /// Internals the list directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">path</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client not connected.</exception>
        private IEnumerable<SftpFile> InternalListDirectory(string path, Action<int> listCallback)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var handle = this._sftpSession.RequestOpenDir(fullPath);

            var basePath = fullPath;

            if (!basePath.EndsWith("/"))
                basePath = string.Format("{0}/", fullPath);

            var result = new List<SftpFile>();

            var files = this._sftpSession.RequestReadDir(handle);

            while (files != null)
            {
                result.AddRange(from f in files
                                select new SftpFile(this._sftpSession, string.Format(CultureInfo.InvariantCulture, "{0}{1}", basePath, f.Key), f.Value));

                //  Call callback to report number of files read
                if (listCallback != null)
                {
                    //  Execute callback on different thread
                    this.ExecuteThread(() => listCallback(result.Count));
                }

                files = this._sftpSession.RequestReadDir(handle);
            }

            this._sftpSession.RequestClose(handle);

            return result;
        }

        /// <summary>
        /// Internals the download file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous request.</param>
        /// <param name="downloadCallback">The download callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace.</exception>
        /// <exception cref="SshConnectionException">Client not connected.</exception>
        private void InternalDownloadFile(string path, Stream output, SftpDownloadAsyncResult asyncResult, Action<ulong> downloadCallback)
        {
            if (output == null)
                throw new ArgumentNullException("output");

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var handle = this._sftpSession.RequestOpen(fullPath, Flags.Read);

            ulong offset = 0;

            var optimalReadLength = _sftpSession.CalculateOptimalReadLength(_bufferSize);

            var data = this._sftpSession.RequestRead(handle, offset, optimalReadLength);

            //  Read data while available
            while (data.Length > 0)
            {
                //  Cancel download
                if (asyncResult != null && asyncResult.IsDownloadCanceled)
                    break;

                output.Write(data, 0, data.Length);

                output.Flush();

                offset += (ulong)data.Length;

                //  Call callback to report number of bytes read
                if (downloadCallback != null)
                {
                    //  Execute callback on different thread
                    this.ExecuteThread(() => { downloadCallback(offset); });
                }

                data = this._sftpSession.RequestRead(handle, offset, optimalReadLength);
            }

            this._sftpSession.RequestClose(handle);
        }

        /// <summary>
        /// Internals the upload file.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous request.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains whitespace.</exception>
        /// <exception cref="SshConnectionException">Client not connected.</exception>
        private void InternalUploadFile(Stream input, string path, Flags flags, SftpUploadAsyncResult asyncResult, Action<ulong> uploadCallback)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (this._sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = this._sftpSession.GetCanonicalPath(path);

            var handle = this._sftpSession.RequestOpen(fullPath, flags);

            ulong offset = 0;

            // create buffer of optimal length
            var buffer = new byte[_sftpSession.CalculateOptimalWriteLength(_bufferSize, handle)];

            var bytesRead = input.Read(buffer, 0, buffer.Length);
            var expectedResponses = 0;
            var responseReceivedWaitHandle = new AutoResetEvent(false);

            do
            {
                //  Cancel upload
                if (asyncResult != null && asyncResult.IsUploadCanceled)
                    break;

                if (bytesRead > 0)
                {
                    if (bytesRead < buffer.Length)
                    {
                        //  Replace buffer for last chunk of data
                        var data = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, data, 0, bytesRead);
                        buffer = data;
                    }

                    var writtenBytes = offset + (ulong)buffer.Length;
                    this._sftpSession.RequestWrite(handle, offset, buffer, null, s =>
                    {
                        if (s.StatusCode == StatusCodes.Ok)
                        {
                            Interlocked.Decrement(ref expectedResponses); 
                            responseReceivedWaitHandle.Set();

                            //  Call callback to report number of bytes written
                            if (uploadCallback != null)
                            {
                                //  Execute callback on different thread
                                this.ExecuteThread(() => uploadCallback(writtenBytes));
                            }
                        }
                    });
                    Interlocked.Increment(ref expectedResponses);

                    offset += (uint)bytesRead;

                    bytesRead = input.Read(buffer, 0, buffer.Length);
                }
                else if (expectedResponses > 0)
                {
                    //  Wait for expectedResponses to change
                    this._sftpSession.WaitOnHandle(responseReceivedWaitHandle, this.OperationTimeout);
                }
            } while (expectedResponses > 0 || bytesRead > 0);

            this._sftpSession.RequestClose(handle);
        }

        partial void ExecuteThread(Action action);

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            this._sftpSession = new SftpSession(this.Session, this.OperationTimeout, this.ConnectionInfo.Encoding);
            this._sftpSession.Connect();
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            // disconnect and dispose the SFTP session
            // the dispose is necessary since we create a new SFTP session
            // on each connect
            if (_sftpSession != null)
            {
                this._sftpSession.Disconnect();
                this._sftpSession.Dispose();
                this._sftpSession = null;
            }
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

            base.Dispose(disposing);
        }
    }
}
