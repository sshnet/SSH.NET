using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public class SftpClient : BaseClient, ISftpClient
    {
        private static readonly Encoding Utf8NoBOM = new UTF8Encoding(false, true);

        /// <summary>
        /// Holds the <see cref="ISftpSession"/> instance that is used to communicate to the
        /// SFTP server.
        /// </summary>
        private ISftpSession _sftpSession;

        /// <summary>
        /// Holds the operation timeout.
        /// </summary>
        private int _operationTimeout;

        /// <summary>
        /// Holds the size of the buffer.
        /// </summary>
        private uint _bufferSize;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite timeout period.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> represents a value that is less than -1 or greater than <see cref="Int32.MaxValue"/> milliseconds.</exception>
        public TimeSpan OperationTimeout
        {
            get
            {
                CheckDisposed();

                return TimeSpan.FromMilliseconds(_operationTimeout);
            }
            set
            {
                CheckDisposed();

                var timeoutInMilliseconds = value.TotalMilliseconds;
                if (timeoutInMilliseconds < -1d || timeoutInMilliseconds > int.MaxValue)
                    throw new ArgumentOutOfRangeException("value", "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");

                _operationTimeout = (int) timeoutInMilliseconds;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size of the buffer in bytes.
        /// </summary>
        /// <value>
        /// The size of the buffer. The default buffer size is 32768 bytes (32 KB).
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
        /// is requested from the peer in a SSH_FXP_READ message. The peer
        /// will send the requested number of bytes in a SSH_FXP_DATA message,
        /// possibly split over multiple SSH_MSG_CHANNEL_DATA messages.
        /// </para>
        /// <para>
        /// To optimize the size of the SSH packets sent by the peer,
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

        /// <summary>
        /// Gets the current SFTP session.
        /// </summary>
        /// <value>
        /// The current SFTP session.
        /// </value>
        internal ISftpSession SftpSession
        {
            get { return _sftpSession; }
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
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
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
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is <b>null</b> contains only whitespace characters.</exception>
        public SftpClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password)
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
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is nu<b>null</b>ll or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
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
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid. <para>-or-</para> <paramref name="username"/> is <b>null</b> or contains only whitespace characters.</exception>
        public SftpClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, the connection info will be disposed when this
        /// instance is disposed.
        /// </remarks>
        private SftpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
            : this(connectionInfo, ownsConnectionInfo, new ServiceFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="serviceFactory">The factory to use for creating new services.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, the connection info will be disposed when this
        /// instance is disposed.
        /// </remarks>
        internal SftpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            _operationTimeout = SshNet.Session.Infinite;
            _bufferSize = 1024 * 32;
        }

        #endregion Constructors

        /// <summary>
        /// Changes remote directory to path.
        /// </summary>
        /// <param name="path">New directory path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change directory denied by remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void ChangeDirectory(string path)
        {
            CheckDisposed();

            if (path == null)
                throw new ArgumentNullException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            _sftpSession.ChangeDirectory(path);
        }

        /// <summary>
        /// Changes permissions of file(s) to specified mode.
        /// </summary>
        /// <param name="path">File(s) path, may match multiple files.</param>
        /// <param name="mode">The mode.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change permission on the path(s) was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void ChangePermissions(string path, short mode)
        {
            var file = Get(path);
            file.SetPermissions(mode);
        }

        /// <summary>
        /// Creates remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to create the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void CreateDirectory(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException(path);

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            _sftpSession.RequestMkDir(fullPath);
        }

        /// <summary>
        /// Deletes remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory to be deleted path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to delete the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void DeleteDirectory(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            _sftpSession.RequestRmDir(fullPath);
        }

        /// <summary>
        /// Deletes remote file specified by path.
        /// </summary>
        /// <param name="path">File to be deleted path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to delete the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void DeleteFile(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            _sftpSession.RequestRemove(fullPath);
        }

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        /// <exception cref="ArgumentNullException"><paramref name="oldPath"/> is <b>null</b>. <para>-or-</para> or <paramref name="newPath"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to rename the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void RenameFile(string oldPath, string newPath)
        {
            RenameFile(oldPath, newPath, false);
        }

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        /// <param name="isPosix">if set to <c>true</c> then perform a posix rename.</param>
        /// <exception cref="ArgumentNullException"><paramref name="oldPath" /> is <b>null</b>. <para>-or-</para> or <paramref name="newPath" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to rename the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void RenameFile(string oldPath, string newPath, bool isPosix)
        {
            CheckDisposed();

            if (oldPath == null)
                throw new ArgumentNullException("oldPath");

            if (newPath == null)
                throw new ArgumentNullException("newPath");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var oldFullPath = _sftpSession.GetCanonicalPath(oldPath);

            var newFullPath = _sftpSession.GetCanonicalPath(newPath);

            if (isPosix)
            {
                _sftpSession.RequestPosixRename(oldFullPath, newFullPath);
            }
            else
            {
                _sftpSession.RequestRename(oldFullPath, newFullPath);
            }
        }

        /// <summary>
        /// Creates a symbolic link from old path to new path.
        /// </summary>
        /// <param name="path">The old path.</param>
        /// <param name="linkPath">The new path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="linkPath"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to create the symbolic link was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void SymbolicLink(string path, string linkPath)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (linkPath.IsNullOrWhiteSpace())
                throw new ArgumentException("linkPath");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            var linkFullPath = _sftpSession.GetCanonicalPath(linkPath);

            _sftpSession.RequestSymLink(fullPath, linkFullPath);
        }

        /// <summary>
        /// Retrieves list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// A list of files.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
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

            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    var result = InternalListDirectory(path, count =>
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
        /// A list of files.
        /// </returns>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndListDirectory(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
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
        /// <returns>
        /// A reference to <see cref="SftpFile"/> file object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFile Get(string path)
        {
            CheckDisposed();

            if (path == null)
                throw new ArgumentNullException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            var attributes = _sftpSession.RequestLStat(fullPath);

            return new SftpFile(_sftpSession, fullPath, attributes);
        }

        /// <summary>
        /// Checks whether file or directory exists;
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// <c>true</c> if directory or file exists; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public bool Exists(string path)
        {
            CheckDisposed();

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            // using SSH_FXP_REALPATH is not an alternative as the SFTP specification has not always
            // been clear on how the server should respond when the specified path is not present on
            // the server:
            // 
            // SSH 1 to 4:
            // No mention of how the server should respond if the path is not present on the server.
            //
            // SSH 5:
            // The server SHOULD fail the request if the path is not present on the server.
            // 
            // SSH 6:
            // Draft 06: The server SHOULD fail the request if the path is not present on the server.
            // Draft 07 to 13: The server MUST NOT fail the request if the path does not exist.
            //
            // Note that SSH 6 (draft 06 and forward) allows for more control options, but we
            // currently only support up to v3.

            try
            {
                _sftpSession.RequestLStat(fullPath);
                return true;
            }
            catch (SftpPathNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Downloads remote file specified by the path into the stream.
        /// </summary>
        /// <param name="path">File to download.</param>
        /// <param name="output">Stream to write the file into.</param>
        /// <param name="downloadCallback">The download callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>/// 
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null)
        {
            CheckDisposed();

            InternalDownloadFile(path, output, null, downloadCallback);
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
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginDownloadFile(string path, Stream output)
        {
            return BeginDownloadFile(path, output, null, null);
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
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback)
        {
            return BeginDownloadFile(path, output, asyncCallback, null);
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
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
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

            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    InternalDownloadFile(path, output, asyncResult, offset =>
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
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndDownloadFile(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException">The path was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        public void EndDownloadFile(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpDownloadAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            ar.EndInvoke();
        }

        /// <summary>
        /// Uploads stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        public void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null)
        {
            UploadFile(input, path, true, uploadCallback);
        }

        /// <summary>
        /// Uploads stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">if set to <c>true</c> then existing file will be overwritten.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
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

            InternalUploadFile(input, path, flags, null, uploadCallback);
        }

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// If the remote file already exists, it is overwritten and truncated.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path)
        {
            return BeginUploadFile(input, path, true, null, null);
        }

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// If the remote file already exists, it is overwritten and truncated.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback)
        {
            return BeginUploadFile(input, path, true, asyncCallback, null);
        }

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
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
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// If the remote file already exists, it is overwritten and truncated.
        /// </para>
        /// </remarks>
        public IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null)
        {
            return BeginUploadFile(input, path, true, asyncCallback, state, uploadCallback);
        }

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">Specified whether an existing file can be overwritten.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// When <paramref name="path"/> refers to an existing file, set <paramref name="canOverride"/> to <c>true</c> to overwrite and truncate that file.
        /// If <paramref name="canOverride"/> is <c>false</c>, the upload will fail and <see cref="EndUploadFile(IAsyncResult)"/> will throw an
        /// <see cref="SshException"/>.
        /// </para>
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

            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    InternalUploadFile(input, path, flags, asyncResult, offset =>
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
        /// Ends an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndUploadFile(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The directory of the file was not found on the remote host.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
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
        /// <returns>
        /// A <see cref="SftpFileSytemInformation"/> instance that contains file status information.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileSytemInformation GetStatus(string path)
        {
            CheckDisposed();

            if (path == null)
                throw new ArgumentNullException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            return _sftpSession.RequestStatVfs(fullPath);
        }

        #region File Methods

        /// <summary>
        /// Appends lines to a file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is<b>null</b> <para>-or-</para> <paramref name="contents"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM)
        /// </remarks>
        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            CheckDisposed();

            if (contents == null)
                throw new ArgumentNullException("contents");

            using (var stream = AppendText(path))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Appends lines to a file by using a specified encoding, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            CheckDisposed();

            if (contents == null)
                throw new ArgumentNullException("contents");

            using (var stream = AppendText(path, encoding))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </remarks>
        public void AppendAllText(string path, string contents)
        {
            using (var stream = AppendText(path))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void AppendAllText(string path, string contents, Encoding encoding)
        {
            using (var stream = AppendText(path, encoding))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Creates a <see cref="StreamWriter"/> that appends UTF-8 encoded text to the specified file,
        /// creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that appends text to a file using UTF-8 encoding without a
        /// Byte-Order Mark (BOM).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamWriter AppendText(string path)
        {
            return AppendText(path, Utf8NoBOM);
        }

        /// <summary>
        /// Creates a <see cref="StreamWriter"/> that appends text to a file using the specified
        /// encoding, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that appends text to a file using the specified encoding.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamWriter AppendText(string path, Encoding encoding)
        {
            CheckDisposed();

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return new StreamWriter(new SftpFileStream(_sftpSession, path, FileMode.Append, FileAccess.Write, (int) _bufferSize), encoding);
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>
        /// A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// If the target file already exists, it is first truncated to zero bytes.
        /// </remarks>
        public SftpFileStream Create(string path)
        {
            CheckDisposed();

            return new SftpFileStream(_sftpSession, path, FileMode.Create, FileAccess.ReadWrite, (int) _bufferSize);
        }

        /// <summary>
        /// Creates or overwrites the specified file.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="bufferSize">The maximum number of bytes buffered for reads and writes to the file.</param>
        /// <returns>
        /// A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// If the target file already exists, it is first truncated to zero bytes.
        /// </remarks>
        public SftpFileStream Create(string path, int bufferSize)
        {
            CheckDisposed();

            return new SftpFileStream(_sftpSession, path, FileMode.Create, FileAccess.ReadWrite, bufferSize);
        }

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that writes text to a file using UTF-8 encoding without
        /// a Byte-Order Mark (BOM).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public StreamWriter CreateText(string path)
        {
            return CreateText(path, Utf8NoBOM);
        }

        /// <summary>
        /// Creates or opens a file for writing text using the specified encoding.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that writes to a file using the specified encoding.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public StreamWriter CreateText(string path, Encoding encoding)
        {
            CheckDisposed();

            return new StreamWriter(OpenWrite(path), encoding);
        }

        /// <summary>
        /// Deletes the specified file or directory.
        /// </summary>
        /// <param name="path">The name of the file or directory to be deleted. Wildcard characters are not supported.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void Delete(string path)
        {
            var file = Get(path);
            file.Delete();
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed.
        /// This value is expressed in local time.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public DateTime GetLastAccessTime(string path)
        {
            var file = Get(path);
            return file.LastAccessTime;
        }

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed.
        /// This value is expressed in UTC time.
        /// </returns>
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
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in local time.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public DateTime GetLastWriteTime(string path)
        {
            var file = Get(path);
            return file.LastWriteTime;
        }

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in UTC time.
        /// </returns>
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
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <returns>
        /// An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and read/write access.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream Open(string path, FileMode mode)
        {
            return Open(path, mode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>
        /// An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and access.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream Open(string path, FileMode mode, FileAccess access)
        {
            CheckDisposed();

            return new SftpFileStream(_sftpSession, path, mode, access, (int) _bufferSize);
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>
        /// A read-only <see cref="SftpFileStream"/> on the specified path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileStream OpenRead(string path)
        {
            return Open(path, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>
        /// A <see cref="StreamReader"/> on the specified path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public StreamReader OpenText(string path)
        {
            return new StreamReader(OpenRead(path), Encoding.UTF8);
        }

        /// <summary>
        /// Opens a file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>
        /// An unshared <see cref="SftpFileStream"/> object on the specified path with <see cref="FileAccess.Write"/> access.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// If the file does not exist, it is created.
        /// </remarks>
        public SftpFileStream OpenWrite(string path)
        {
            CheckDisposed();

            return new SftpFileStream(_sftpSession, path, FileMode.OpenOrCreate, FileAccess.Write, (int) _bufferSize);
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>
        /// A byte array containing the contents of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public byte[] ReadAllBytes(string path)
        {
            using (var stream = OpenRead(path))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file using UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public string[] ReadAllLines(string path)
        {
            return ReadAllLines(path, Encoding.UTF8);
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public string[] ReadAllLines(string path, Encoding encoding)
        {
            // we use the default buffer size for StreamReader - which is 1024 bytes - and the configured buffer size
            // for the SftpFileStream; may want to revisit this later

            var lines = new List<string>();
            using (var stream = new StreamReader(OpenRead(path), encoding))
            {
                while (!stream.EndOfStream)
                {
                    lines.Add(stream.ReadLine());
                }
            }
            return lines.ToArray();
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file with the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>
        /// A string containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public string ReadAllText(string path)
        {
            return ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>
        /// A string containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public string ReadAllText(string path, Encoding encoding)
        {
            // we use the default buffer size for StreamReader - which is 1024 bytes - and the configured buffer size
            // for the SftpFileStream; may want to revisit this later

            using (var stream = new StreamReader(OpenRead(path), encoding))
            {
                return stream.ReadToEnd();
            }
        }

        /// <summary>
        /// Reads the lines of a file with the UTF-8 encoding.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <returns>
        /// The lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public IEnumerable<string> ReadLines(string path)
        {
            return ReadAllLines(path);
        }

        /// <summary>
        /// Read the lines of a file that has a specified encoding.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
        /// <returns>
        /// The lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            return ReadAllLines(path, encoding);
        }

        /// <summary>
        /// Sets the date and time the specified file was last accessed.
        /// </summary>
        /// <param name="path">The file for which to set the access date and time information.</param>
        /// <param name="lastAccessTime">A <see cref="DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in local time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last accessed.
        /// </summary>
        /// <param name="path">The file for which to set the access date and time information.</param>
        /// <param name="lastAccessTimeUtc">A <see cref="DateTime"/> containing the value to set for the last access date and time of path. This value is expressed in UTC time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTime">A <see cref="DateTime"/> containing the value to set for the last write date and time of path. This value is expressed in local time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">The file for which to set the date and time information.</param>
        /// <param name="lastWriteTimeUtc">A <see cref="DateTime"/> containing the value to set for the last write date and time of path. This value is expressed in UTC time.</param>
        [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the specified byte array to the specified file, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllBytes(string path, byte[] bytes)
        {
            using (var stream = OpenWrite(path))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Writes a collection of strings to the file using the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </para>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            WriteAllLines(path, contents, Utf8NoBOM);
        }

        /// <summary>
        /// Write the specified string array to the file using the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </para>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllLines(string path, string[] contents)
        {
            WriteAllLines(path, contents, Utf8NoBOM);
        }

        /// <summary>
        /// Writes a collection of strings to the file using the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            using (var stream = CreateText(path, encoding))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Writes the specified string array to the file by using the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <param name="encoding">An <see cref="Encoding"/> object that represents the character encoding applied to the string array.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            using (var stream = CreateText(path, encoding))
            {
                foreach (var line in contents)
                {
                    stream.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Writes the specified string to the file using the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </para>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllText(string path, string contents)
        {
            using (var stream = CreateText(path))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Writes the specified string to the file using the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            using (var stream = CreateText(path, encoding))
            {
                stream.Write(contents);
            }
        }

        /// <summary>
        /// Gets the <see cref="SftpFileAttributes"/> of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// The <see cref="SftpFileAttributes"/> of the file on the path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public SftpFileAttributes GetAttributes(string path)
        {
            CheckDisposed();

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            return _sftpSession.RequestLStat(fullPath);
        }

        /// <summary>
        /// Sets the specified <see cref="SftpFileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">The desired <see cref="SftpFileAttributes"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void SetAttributes(string path, SftpFileAttributes fileAttributes)
        {
            CheckDisposed();

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            _sftpSession.RequestSetStat(fullPath, fileAttributes);
        }

        // Please don't forget this when you implement these methods: <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
        //public FileSecurity GetAccessControl(string path);
        //public FileSecurity GetAccessControl(string path, AccessControlSections includeSections);
        //public DateTime GetCreationTime(string path);
        //public DateTime GetCreationTimeUtc(string path);
        //public void SetAccessControl(string path, FileSecurity fileSecurity);
        //public void SetCreationTime(string path, DateTime creationTime);
        //public void SetCreationTimeUtc(string path, DateTime creationTimeUtc);

        #endregion // File Methods

        #region SynchronizeDirectories

        /// <summary>
        /// Synchronizes the directories.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns>
        /// A list of uploaded files.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationPath"/> is <c>null</c> or contains only whitespace.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="destinationPath"/> was not found on the remote host.</exception>
        public IEnumerable<FileInfo> SynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern)
        {
            if (sourcePath == null)
                throw new ArgumentNullException("sourcePath");
            if (destinationPath.IsNullOrWhiteSpace())
                throw new ArgumentException("destinationPath");

            return InternalSynchronizeDirectories(sourcePath, destinationPath, searchPattern, null);
        }

        /// <summary>
        /// Begins the synchronize directories.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that represents the asynchronous directory synchronization.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationPath"/> is <c>null</c> or contains only whitespace.</exception>
        public IAsyncResult BeginSynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern, AsyncCallback asyncCallback, object state)
        {
            if (sourcePath == null)
                throw new ArgumentNullException("sourcePath");
            if (destinationPath.IsNullOrWhiteSpace())
                throw new ArgumentException("destDir");

            var asyncResult = new SftpSynchronizeDirectoriesAsyncResult(asyncCallback, state);

            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    var result = InternalSynchronizeDirectories(sourcePath, destinationPath, searchPattern, asyncResult);

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
        /// Ends the synchronize directories.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        /// <returns>
        /// A list of uploaded files.
        /// </returns>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndSynchronizeDirectories(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SftpPathNotFoundException">The destination path was not found on the remote host.</exception>
        public IEnumerable<FileInfo> EndSynchronizeDirectories(IAsyncResult asyncResult)
        {
            var ar = asyncResult as SftpSynchronizeDirectoriesAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            return ar.EndInvoke();
        }

        private IEnumerable<FileInfo> InternalSynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern, SftpSynchronizeDirectoriesAsyncResult asynchResult)
        {
            if (!Directory.Exists(sourcePath))
                throw new FileNotFoundException(string.Format("Source directory not found: {0}", sourcePath));

            var uploadedFiles = new List<FileInfo>();

            var sourceDirectory = new DirectoryInfo(sourcePath);

            var sourceFiles = FileSystemAbstraction.EnumerateFiles(sourceDirectory, searchPattern).ToList();
            if (sourceFiles.Count == 0)
                return uploadedFiles;

            #region Existing Files at The Destination

            var destFiles = InternalListDirectory(destinationPath, null);
            var destDict = new Dictionary<string, SftpFile>();
            foreach (var destFile in destFiles)
            {
                if (destFile.IsDirectory)
                    continue;
                destDict.Add(destFile.Name, destFile);
            }

            #endregion

            #region Upload the difference

            const Flags uploadFlag = Flags.Write | Flags.Truncate | Flags.CreateNewOrOpen;
            foreach (var localFile in sourceFiles)
            {
                var isDifferent = !destDict.ContainsKey(localFile.Name);

                if (!isDifferent)
                {
                    var temp = destDict[localFile.Name];
                    //  TODO:   Use md5 to detect a difference
                    //ltang: File exists at the destination => Using filesize to detect the difference
                    isDifferent = localFile.Length != temp.Length;
                }

                if (isDifferent)
                {
                    var remoteFileName = string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", destinationPath, localFile.Name);
                    try
                    {
                        using (var file = File.OpenRead(localFile.FullName))
                        {
                            InternalUploadFile(file, remoteFileName, uploadFlag, null, null);
                        }

                        uploadedFiles.Add(localFile);

                        if (asynchResult != null)
                        {
                            asynchResult.Update(uploadedFiles.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Failed to upload {0} to {1}", localFile.FullName, remoteFileName), ex);
                    }
                }
            }

            #endregion

            return uploadedFiles;
        }

        #endregion

        /// <summary>
        /// Internals the list directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// A list of files in the specfied directory.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client not connected.</exception>
        private IEnumerable<SftpFile> InternalListDirectory(string path, Action<int> listCallback)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            var handle = _sftpSession.RequestOpenDir(fullPath);

            var basePath = fullPath;

            if (!basePath.EndsWith("/"))
                basePath = string.Format("{0}/", fullPath);

            var result = new List<SftpFile>();

            var files = _sftpSession.RequestReadDir(handle);

            while (files != null)
            {
                result.AddRange(from f in files
                                select new SftpFile(_sftpSession, string.Format(CultureInfo.InvariantCulture, "{0}{1}", basePath, f.Key), f.Value));

                //  Call callback to report number of files read
                if (listCallback != null)
                {
                    //  Execute callback on different thread
                    ThreadAbstraction.ExecuteThread(() => listCallback(result.Count));
                }

                files = _sftpSession.RequestReadDir(handle);
            }

            _sftpSession.RequestClose(handle);

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

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            using (var fileReader = ServiceFactory.CreateSftpFileReader(fullPath, _sftpSession, _bufferSize))
            {
                var totalBytesRead = 0UL;

                while (true)
                {
                    //  Cancel download
                    if (asyncResult != null && asyncResult.IsDownloadCanceled)
                        break;

                    var data = fileReader.Read();
                    if (data.Length == 0)
                        break;

                    output.Write(data, 0, data.Length);

                    totalBytesRead += (ulong) data.Length;

                    if (downloadCallback != null)
                    {
                        // copy offset to ensure it's not modified between now and execution of callback
                        var downloadOffset = totalBytesRead;

                        //  Execute callback on different thread
                        ThreadAbstraction.ExecuteThread(() => { downloadCallback(downloadOffset); });
                    }
                }
            }
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

            if (_sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            var fullPath = _sftpSession.GetCanonicalPath(path);

            var handle = _sftpSession.RequestOpen(fullPath, flags);

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
                    var writtenBytes = offset + (ulong) bytesRead;

                    _sftpSession.RequestWrite(handle, offset, buffer, 0, bytesRead, null, s =>
                        {
                            if (s.StatusCode == StatusCodes.Ok)
                            {
                                Interlocked.Decrement(ref expectedResponses);
                                responseReceivedWaitHandle.Set();

                                //  Call callback to report number of bytes written
                                if (uploadCallback != null)
                                {
                                    //  Execute callback on different thread
                                    ThreadAbstraction.ExecuteThread(() => uploadCallback(writtenBytes));
                                }
                            }
                        });
                    Interlocked.Increment(ref expectedResponses);

                    offset += (ulong) bytesRead;

                    bytesRead = input.Read(buffer, 0, buffer.Length);
                }
                else if (expectedResponses > 0)
                {
                    //  Wait for expectedResponses to change
                    _sftpSession.WaitOnHandle(responseReceivedWaitHandle, _operationTimeout);
                }
            } while (expectedResponses > 0 || bytesRead > 0);

            _sftpSession.RequestClose(handle);
        }

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            _sftpSession = CreateAndConnectToSftpSession();
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            // disconnect, dispose and dereference the SFTP session since we create a new SFTP session
            // on each connect
            var sftpSession = _sftpSession;
            if (sftpSession != null)
            {
                _sftpSession = null;
                sftpSession.Dispose();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                var sftpSession = _sftpSession;
                if (sftpSession != null)
                {
                    _sftpSession = null;
                    sftpSession.Dispose();
                }
            }
        }

        private ISftpSession CreateAndConnectToSftpSession()
        {
            var sftpSession = ServiceFactory.CreateSftpSession(Session,
                                                               _operationTimeout,
                                                               ConnectionInfo.Encoding,
                                                               ServiceFactory.CreateSftpResponseFactory());
            try
            {
                sftpSession.Connect();
                return sftpSession;
            }
            catch
            {
                sftpSession.Dispose();
                throw;
            }
        }
    }
}
