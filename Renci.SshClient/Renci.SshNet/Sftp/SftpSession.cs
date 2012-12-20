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
    internal class SftpSession : SubsystemSession
    {
        private Dictionary<uint, SftpRequest> _requests = new Dictionary<uint, SftpRequest>();

        private List<byte> _data = new List<byte>(16 * 1024);

        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(false);

        /// <summary>
        /// Gets remote working directory.
        /// </summary>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// Gets SFTP protocol version.
        /// </summary>
        public int ProtocolVersion { get; private set; }

        private long _requestId;
        /// <summary>
        /// Gets the next request id for sftp session.
        /// </summary>
        public uint NextRequestId
        {
            get
            {
#if WINDOWS_PHONE
                lock (this)
                {
                    this._requestId++;
                }

                return (uint)this._requestId;
#else
                return ((uint)Interlocked.Increment(ref this._requestId));
#endif
            }
        }

        public SftpSession(Session session, TimeSpan operationTimeout)
            : base(session, "sftp", operationTimeout)
        {
        }

        public void ChangeDirectory(string path)
        {
            var fullPath = this.GetCanonicalPath(path);

            var handle = this.RequestOpenDir(fullPath);

            this.RequestClose(handle);

            this.WorkingDirectory = fullPath;
        }

        internal void SendMessage(SftpMessage sftpMessage)
        {
            var messageData = sftpMessage.GetBytes();

            var data = new byte[4 + messageData.Length];

            ((uint)messageData.Length).GetBytes().CopyTo(data, 0);
            messageData.CopyTo(data, 4);

            this.SendData(data);
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
                canonizedPath = realPathFiles.First().Key;                
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

            realPathFiles = this.RequestRealPath(partialFullPath, true);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles.First().Key;
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

        protected override void OnChannelOpen()
        {
            this.SendMessage(new SftpInitRequest(3));

            this.WaitHandle(this._sftpVersionConfirmed, this._operationTimeout);

            this.ProtocolVersion = 3;

            //  Resolve current directory
            this.WorkingDirectory = this.RequestRealPath(".").First().Key;
        }

        protected override void OnDataReceived(uint dataTypeCode, byte[] data)
        {
            //  Add channel data to internal data holder
            this._data.AddRange(data);

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
                var response = SftpMessage.Load(packetData);

                try
                {
                    var versionResponse = response as SftpVersionResponse;
                    if (versionResponse != null)
                    {
                        if (versionResponse.Version == 3)
                        {
                            this._sftpVersionConfirmed.Set();
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Server SFTP version {0} is not supported.", versionResponse.Version));
                        }
                    }
                    else
                    {
                        this.HandleResponse(response as SftpResponse);
                    }
                }
                catch (Exception exp)
                {
                    this.RaiseError(exp);
                    break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this._sftpVersionConfirmed != null)
                {
                    this._sftpVersionConfirmed.Dispose();
                    this._sftpVersionConfirmed = null;
                }
            }
        }

        private void SendRequest(SftpRequest request)
        {
            lock (this._requests)
            {
                this._requests.Add(request.RequestId, request);
            }

            this.SendMessage(request);
            //this.SendData(new SftpDataMessage(this.ChannelNumber, request));

            //var messageData = request.GetBytes();

            //var data = new byte[4 + messageData.Length];

            //((uint)messageData.Length).GetBytes().CopyTo(data, 0);
            //messageData.CopyTo(data, 4);

            //this.SendData(data);

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
        /// <param name="data">The data to send.</param>
        /// <param name="wait">The wait event handle if needed.</param>
        internal void RequestWrite(byte[] handle, UInt64 offset, byte[] data, EventWaitHandle wait)
        {
            var maximumDataSize = 1024 * 16;

            if (data.Length < maximumDataSize + 1)
            {
                var request = new SftpWriteRequest(this.NextRequestId, handle, offset, data,
                    (response) =>
                    {
                        if (response.StatusCode == StatusCodes.Ok)
                        {
                            if (wait != null)
                                wait.Set();
                        }
                        else
                        {
                            this.ThrowSftpException(response);
                        }
                    });

                this.SendRequest(request);

                if (wait != null)
                    this.WaitHandle(wait, this._operationTimeout);
            }
            else
            {
                var block = data.Length / maximumDataSize + 1;

                for (int i = 0; i < block; i++)
                {
                    var blockBufferSize = Math.Min(data.Length - maximumDataSize * i, maximumDataSize);
                    var blockBuffer = new byte[blockBufferSize];

                    Buffer.BlockCopy(data, i * maximumDataSize, blockBuffer, 0, blockBufferSize);

                    var request = new SftpWriteRequest(this.NextRequestId, handle, offset + (ulong)(i * maximumDataSize), blockBuffer,
                        (response) =>
                        {
                            if (response.StatusCode == StatusCodes.Ok)
                            {
                                if (wait != null)
                                    wait.Set();
                            }
                            else
                            {
                                this.ThrowSftpException(response);
                            }
                        });

                    this.SendRequest(request);

                    if (wait != null)
                        this.WaitHandle(wait, this._operationTimeout);
                }
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
        internal KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle)
        {
            KeyValuePair<string, SftpFileAttributes>[] result = null;

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
        internal KeyValuePair<string, SftpFileAttributes>[] RequestRealPath(string path, bool nullOnError = false)
        {
            KeyValuePair<string, SftpFileAttributes>[] result = null;

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
        internal KeyValuePair<string, SftpFileAttributes>[] RequestReadLink(string path, bool nullOnError = false)
        {
            KeyValuePair<string, SftpFileAttributes>[] result = null;

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
    }
}
