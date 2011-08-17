using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Sftp.Requests;

namespace Renci.SshNet.Sftp
{
    internal class SftpSession : IDisposable
    {
        private Session _session;

        private ChannelSession _channel;

        private Dictionary<uint, SftpRequest> _requests = new Dictionary<uint, SftpRequest>();

        private List<byte> _data = new List<byte>(32 * 1024);

        private Exception _exception;

        private EventWaitHandle _errorOccuredWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(false);

        private TimeSpan _operationTimeout;

        public event EventHandler<ExceptionEventArgs> ErrorOccured;

        /// <summary>
        /// Gets remote working directory.
        /// </summary>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// Gets SFTP protocol version.
        /// </summary>
        public int ProtocolVersion { get; private set; }

        private uint _requestId;
        /// <summary>
        /// Gets the next request id for sftp session.
        /// </summary>
        public uint NextRequestId
        {
            get
            {
                return this._requestId++;
            }
        }

        #region SFTP messages

        //internal event EventHandler<MessageEventArgs<StatusMessage>> StatusMessageReceived;

        //internal event EventHandler<MessageEventArgs<DataMessage>> DataMessageReceived;

        //internal event EventHandler<MessageEventArgs<HandleMessage>> HandleMessageReceived;

        //internal event EventHandler<MessageEventArgs<NameMessage>> NameMessageReceived;

        //internal event EventHandler<MessageEventArgs<AttributesMessage>> AttributesMessageReceived;

        #endregion

        public SftpSession(Session session, TimeSpan operationTimeout)
        {
            this._session = session;
            this._operationTimeout = operationTimeout;
        }

        public void Connect()
        {
            this._channel = this._session.CreateChannel<ChannelSession>();

            this._session.ErrorOccured += Session_ErrorOccured;
            this._session.Disconnected += Session_Disconnected;
            this._channel.DataReceived += Channel_DataReceived;

            this._channel.Open();

            this._channel.SendSubsystemRequest("sftp");

            this.SendMessage(new SftpInitRequest(3));

            this.WaitHandle(this._sftpVersionConfirmed, this._operationTimeout);

            this.ProtocolVersion = 3;

            //  Resolve current directory
            this.WorkingDirectory = this.RequestRealPath(".").Keys.First();
        }

        public void Disconnect()
        {
            this.Dispose();
        }

        public void ChangeDirectory(string path)
        {
            var fullPath = this.GetCanonicalPath(path);

            var handle = this.RequestOpenDir(fullPath);

            this.RequestClose(handle);

            this.WorkingDirectory = fullPath;
        }

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            //  Add channel data to internal data holder
            this._data.AddRange(e.Data);

            while (this._data.Count > 4 + 1)
            {
                //  Extract packet length
                var packetLength = (this._data[0] << 24 | this._data[1] << 16 | this._data[2] << 8 | this._data[3]);

                //  Check if complete packet data is available
                if (this._data.Count < packetLength + 4)
                {
                    //  Wait for complete message to arrive first
                    break;
                }
                this._data.RemoveRange(0, 4);

                //  Create buffer to hold packet data
                var packetData = new byte[packetLength];

                //  Cope packet data to array
                this._data.CopyTo(0, packetData, 0, packetLength);

                //  Remove loaded data from _data holder
                this._data.RemoveRange(0, packetLength);

                //  Load SFTP Message and handle it
                dynamic response = SftpMessage.Load(packetData);

                try
                {
                    if (response is SftpVersionResponse)
                    {
                        if (response.Version == 3)
                        {
                            this._sftpVersionConfirmed.Set();
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Server SFTP version {0} is not supported.", response.Version));
                        }
                    }
                    else
                    {
                        this.HandleResponse(response);
                    }
                }
                catch (Exception exp)
                {
                    this.RaiseError(exp);
                    break;
                }
            }
        }

        private void SendRequest(SftpRequest request)
        {
            lock (this._requests)
            {
                this._requests.Add(request.RequestId, request);
            }

            this._session.SendMessage(new SftpDataMessage(this._channel.RemoteChannelNumber, request));
        }

        internal void SendMessage(SftpMessage sftpMessage)
        {
            this._session.SendMessage(new SftpDataMessage(this._channel.RemoteChannelNumber, sftpMessage));
        }

        internal void WaitHandle(WaitHandle waitHandle, TimeSpan operationTimeout)
        {
            var waitHandles = new WaitHandle[]
                {
                    this._errorOccuredWaitHandle,
                    waitHandle,
                };

            var index = EventWaitHandle.WaitAny(waitHandles, operationTimeout);

            if (index < 1)
            {
                throw this._exception;
            }
            else if (index > 1)
            {
                //  throw time out error
                throw new SshOperationTimeoutException(string.Format(CultureInfo.CurrentCulture, "Sftp operation has timed out."));
            }
        }

        /// <summary>
        /// Resolves path into absolute path on the server.
        /// </summary>
        /// <param name="path">Path to resolve.</param>
        /// <returns>Absolute path</returns>
        internal string GetCanonicalPath(string path)
        {
            var fullPath = path;

            if (!string.IsNullOrEmpty(path) && path[0] != '/' && this.WorkingDirectory != null)
            {
                if (this.WorkingDirectory[this.WorkingDirectory.Length - 1] == '/')
                {
                    fullPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.WorkingDirectory, path);
                }
                else
                {
                    fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.WorkingDirectory, path);
                }
            }

            var canonizedPath = string.Empty;

            var realPathFiles = this.RequestRealPath(fullPath, true);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles.Keys.First();
            }

            if (!string.IsNullOrEmpty(canonizedPath))
                return canonizedPath;

            //  Check for special cases
            if (fullPath.EndsWith("/.", StringComparison.InvariantCultureIgnoreCase) ||
                fullPath.EndsWith("/..", StringComparison.InvariantCultureIgnoreCase) ||
                fullPath.Equals("/", StringComparison.InvariantCultureIgnoreCase) ||
                fullPath.IndexOf('/') < 0)
                return fullPath;

            var pathParts = fullPath.Split(new char[] { '/' });

            var partialFullPath = string.Join("/", pathParts, 0, pathParts.Length - 1);

            if (string.IsNullOrEmpty(partialFullPath))
                partialFullPath = "/";

            //canonizedPath = this.RequestRealPath(partialFullPath).First().FullName;

            realPathFiles = this.RequestRealPath(partialFullPath, true);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles.Keys.First();
            }


            if (string.IsNullOrEmpty(canonizedPath))
            {
                return fullPath;
            }
            else
            {
                var slash = string.Empty;
                if (canonizedPath[canonizedPath.Length - 1] != '/')
                    slash = "/";
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", canonizedPath, slash, pathParts[pathParts.Length - 1]);
            }
        }

        internal bool FileExistsCommand(string path, Flags flags)
        {
            var handle = this.RequestOpen(path, flags, true);
            if (handle == null)
            {
                return false;
            }
            else
            {
                this.RequestClose(handle);

                return true;
            }
        }

        #region SFTP API functions

        //#define SSH_FXP_INIT                1
        //#define SSH_FXP_VERSION             2

        /// <summary>
        /// Performs SSH_FXP_OPEN request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns></returns>
        internal byte[] RequestOpen(string path, Flags flags, bool nullOnError = false)
        {
            byte[] handle = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpOpenRequest(this.NextRequestId, path, flags,
                    (response) =>
                    {
                        handle = response.Handle;
                        wait.Set();
                    },
                    (response) =>
                    {
                        if (nullOnError)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return handle;
        }

        /// <summary>
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        internal void RequestClose(byte[] handle)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpCloseRequest(this.NextRequestId, handle,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>data array; null if EOF</returns>
        internal byte[] RequestRead(byte[] handle, UInt64 offset, UInt32 length)
        {
            byte[] data = new byte[0];

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpReadRequest(this.NextRequestId, handle, offset, length,
                    (response) =>
                    {
                        data = response.Data;
                        wait.Set();
                    },
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Eof)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return data;
        }

        /// <summary>
        /// Performs SSH_FXP_WRITE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="data">The data.</param>
        internal void RequestWrite(byte[] handle, UInt64 offset, byte[] data)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpWriteRequest(this.NextRequestId, handle, offset, data,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        internal SftpFileAttributes RequestLStat(string path, bool nullOnError = false)
        {
            SftpFileAttributes attributes = null;
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpLStatRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        attributes = response.Attributes;
                        wait.Set();
                    },
                    (response) =>
                    {
                        this.ThrowSftpException(response);
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_FSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        internal SftpFileAttributes RequestFStat(byte[] handle, bool nullOnError = false)
        {
            SftpFileAttributes attributes = null;
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpFStatRequest(this.NextRequestId, handle,
                    (response) =>
                    {
                        attributes = response.Attributes;
                        wait.Set();
                    },
                    (response) =>
                    {
                        this.ThrowSftpException(response);
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_SETSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="attributes">The attributes.</param>
        internal void RequestSetStat(string path, SftpFileAttributes attributes)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpSetStatRequest(this.NextRequestId, path, attributes,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_FSETSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="attributes">The attributes.</param>
        internal void RequestFSetStat(byte[] handle, SftpFileAttributes attributes)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpFSetStatRequest(this.NextRequestId, handle, attributes,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_OPENDIR request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns></returns>
        internal byte[] RequestOpenDir(string path, bool nullOnError = false)
        {
            byte[] handle = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpOpenDirRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        handle = response.Handle;
                        wait.Set();
                    },
                    (response) =>
                    {
                        if (nullOnError)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return handle;
        }

        /// <summary>
        /// Performs SSH_FXP_READDIR request
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        internal IDictionary<string, SftpFileAttributes> RequestReadDir(byte[] handle)
        {
            IDictionary<string, SftpFileAttributes> result = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpReadDirRequest(this.NextRequestId, handle,
                    (response) =>
                    {
                        result = response.Files;
                        wait.Set();
                    },
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Eof)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return result;
        }

        /// <summary>
        /// Performs SSH_FXP_REMOVE request.
        /// </summary>
        /// <param name="path">The path.</param>
        internal void RequestRemove(string path)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRemoveRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_MKDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        internal void RequestMkDir(string path)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpMkDirRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_RMDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        internal void RequestRmDir(string path)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRmDirRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_REALPATH request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns></returns>
        internal IDictionary<string, SftpFileAttributes> RequestRealPath(string path, bool nullOnError = false)
        {
            IDictionary<string, SftpFileAttributes> result = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRealPathRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        result = response.Files;

                        wait.Set();
                    },
                    (response) =>
                    {
                        if (nullOnError)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return result;
        }

        /// <summary>
        /// Performs SSH_FXP_STAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        internal SftpFileAttributes RequestStat(string path, bool nullOnError = false)
        {
            SftpFileAttributes attributes = null;
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpStatRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        attributes = response.Attributes;
                        wait.Set();
                    },
                    (response) =>
                    {
                        if (nullOnError)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_RENAME request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        internal void RequestRename(string oldPath, string newPath)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRenameRequest(this.NextRequestId, oldPath, newPath,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_READLINK request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns></returns>
        internal IDictionary<string, SftpFileAttributes> RequestReadLink(string path, bool nullOnError = false)
        {
            IDictionary<string, SftpFileAttributes> result = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpReadLinkRequest(this.NextRequestId, path,
                    (response) =>
                    {
                        result = response.Files;

                        wait.Set();
                    },
                    (response) =>
                    {
                        if (nullOnError)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }

            return result;
        }

        /// <summary>
        /// Performs SSH_FXP_SYMLINK request.
        /// </summary>
        /// <param name="linkpath">The linkpath.</param>
        /// <param name="targetpath">The targetpath.</param>
        internal void RequestSymLink(string linkpath, string targetpath)
        {
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpSymLinkRequest(this.NextRequestId, linkpath, targetpath,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                this.WaitHandle(wait, this._operationTimeout);
            }
        }

        #endregion

        private void ThrowSftpException(SftpStatusResponse response)
        {
            if (response.StatusCode == StatusCodes.PermissionDenied)
            {
                throw new SftpPermissionDeniedException(response.ErrorMessage);
            }
            else if (response.StatusCode == StatusCodes.NoSuchFile)
            {
                throw new SftpPathNotFoundException(response.ErrorMessage);
            }
            else
            {
                throw new SshException(response.ErrorMessage);
            }
        }

        private void HandleResponse(SftpResponse response)
        {
            SftpRequest request = null;
            lock (this._requests)
            {
                this._requests.TryGetValue(response.ResponseId, out request);
                if (request != null)
                {
                    this._requests.Remove(response.ResponseId);
                }
            }

            if (request == null)
                throw new InvalidOperationException("Invalid response.");

            request.Complete(response);
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            this.RaiseError(new SshException("Connection was lost"));
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.RaiseError(e.Exception);
        }

        private void RaiseError(Exception error)
        {
            this._exception = error;

            this._errorOccuredWaitHandle.Set();

            if (this.ErrorOccured != null)
            {
                this.ErrorOccured(this, new ExceptionEventArgs(error));
            }
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                if (this._channel != null)
                {
                    this._channel.DataReceived -= Channel_DataReceived;

                    this._channel.Dispose();
                    this._channel = null;
                }

                this._session.ErrorOccured -= Session_ErrorOccured;
                this._session.Disconnected -= Session_Disconnected;

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._errorOccuredWaitHandle != null)
                    {
                        this._errorOccuredWaitHandle.Dispose();
                        this._errorOccuredWaitHandle = null;
                    }
                    if (this._sftpVersionConfirmed != null)
                    {
                        this._sftpVersionConfirmed.Dispose();
                        this._sftpVersionConfirmed = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SftpSession"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpSession()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
