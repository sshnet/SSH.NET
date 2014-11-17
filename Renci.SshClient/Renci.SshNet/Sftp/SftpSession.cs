using System;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshNet.Common;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Sftp.Requests;

namespace Renci.SshNet.Sftp
{
    internal class SftpSession : SubsystemSession, ISftpSession
    {
        private const int MaximumSupportedVersion = 3;

        private const int MinimumSupportedVersion = 0;

        private readonly Dictionary<uint, SftpRequest> _requests = new Dictionary<uint, SftpRequest>();

        private readonly List<byte> _data = new List<byte>(16 * 1024);

        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(false);

        private IDictionary<string, string> _supportedExtensions;

        /// <summary>
        /// Gets the remote working directory.
        /// </summary>
        /// <value>
        /// The remote working directory.
        /// </value>
        public string WorkingDirectory { get; private set; }

        /// <summary>
        /// Gets the SFTP protocol version.
        /// </summary>
        /// <value>
        /// The SFTP protocol version.
        /// </value>
        public uint ProtocolVersion { get; private set; }

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
                return ((uint)Interlocked.Increment(ref _requestId));
#endif
            }
        }

        public SftpSession(ISession session, TimeSpan operationTimeout, Encoding encoding)
            : base(session, "sftp", operationTimeout, encoding)
        {
        }

        /// <summary>
        /// Changes the current working directory to the specified path.
        /// </summary>
        /// <param name="path">The new working directory.</param>
        public void ChangeDirectory(string path)
        {
            var fullPath = GetCanonicalPath(path);

            var handle = RequestOpenDir(fullPath);

            RequestClose(handle);

            WorkingDirectory = fullPath;
        }

        internal void SendMessage(SftpMessage sftpMessage)
        {
            var messageData = sftpMessage.GetBytes();

            var data = new byte[4 + messageData.Length];

            ((uint)messageData.Length).GetBytes().CopyTo(data, 0);
            messageData.CopyTo(data, 4);

            SendData(data);
        }

        /// <summary>
        /// Resolves a given path into an absolute path on the server.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>
        /// The absolute path.
        /// </returns>
        public string GetCanonicalPath(string path)
        {
            var fullPath = GetFullRemotePath(path);

            var canonizedPath = string.Empty;

            var realPathFiles = RequestRealPath(fullPath, true);

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

            var pathParts = fullPath.Split(new[] { '/' });

            var partialFullPath = string.Join("/", pathParts, 0, pathParts.Length - 1);

            if (string.IsNullOrEmpty(partialFullPath))
                partialFullPath = "/";

            realPathFiles = RequestRealPath(partialFullPath, true);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles.First().Key;
            }

            if (string.IsNullOrEmpty(canonizedPath))
            {
                return fullPath;
            }

            var slash = string.Empty;
            if (canonizedPath[canonizedPath.Length - 1] != '/')
                slash = "/";
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", canonizedPath, slash, pathParts[pathParts.Length - 1]);
        }

        internal string GetFullRemotePath(string path)
        {
            var fullPath = path;

            if (!string.IsNullOrEmpty(path) && path[0] != '/' && WorkingDirectory != null)
            {
                if (WorkingDirectory[WorkingDirectory.Length - 1] == '/')
                {
                    fullPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}", WorkingDirectory, path);
                }
                else
                {
                    fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", WorkingDirectory, path);
                }
            }
            return fullPath;
        }

        protected override void OnChannelOpen()
        {
            SendMessage(new SftpInitRequest(MaximumSupportedVersion));

            WaitOnHandle(_sftpVersionConfirmed, OperationTimeout);

            if (ProtocolVersion > MaximumSupportedVersion || ProtocolVersion < MinimumSupportedVersion)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Server SFTP version {0} is not supported.", ProtocolVersion));
            }

            //  Resolve current directory
            WorkingDirectory = RequestRealPath(".").First().Key;
        }

        protected override void OnDataReceived(uint dataTypeCode, byte[] data)
        {
            //  Add channel data to internal data holder
            _data.AddRange(data);

            while (_data.Count > 4 + 1)
            {
                //  Extract packet length
                var packetLength = (_data[0] << 24 | _data[1] << 16 | _data[2] << 8 | _data[3]);

                //  Check if complete packet data is available
                if (_data.Count < packetLength + 4)
                {
                    //  Wait for complete message to arrive first
                    break;
                }
                _data.RemoveRange(0, 4);

                //  Create buffer to hold packet data
                var packetData = new byte[packetLength];

                //  Cope packet data to array
                _data.CopyTo(0, packetData, 0, packetLength);

                //  Remove loaded data from _data holder
                _data.RemoveRange(0, packetLength);

                //  Load SFTP Message and handle it
                var response = SftpMessage.Load(ProtocolVersion, packetData, Encoding);

                try
                {
                    var versionResponse = response as SftpVersionResponse;
                    if (versionResponse != null)
                    {
                        ProtocolVersion = versionResponse.Version;
                        _supportedExtensions = versionResponse.Extentions;

                        _sftpVersionConfirmed.Set();
                    }
                    else
                    {
                        HandleResponse(response as SftpResponse);
                    }
                }
                catch (Exception exp)
                {
                    RaiseError(exp);
                    break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_sftpVersionConfirmed != null)
                {
                    _sftpVersionConfirmed.Dispose();
                    _sftpVersionConfirmed = null;
                }
            }
        }

        private void SendRequest(SftpRequest request)
        {
            lock (_requests)
            {
                _requests.Add(request.RequestId, request);
            }

            SendMessage(request);
        }

        #region SFTP API functions

        /// <summary>
        /// Performs SSH_FXP_OPEN request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns <c>null</c> instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        public byte[] RequestOpen(string path, Flags flags, bool nullOnError = false)
        {
            byte[] handle = null;
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpOpenRequest(ProtocolVersion, NextRequestId, path, Encoding, flags,
                    response =>
                        {
                            handle = response.Handle;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception != null)
            {
                throw exception;
            }

            return handle;
        }

        /// <summary>
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        public void RequestClose(byte[] handle)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpCloseRequest(ProtocolVersion, NextRequestId, handle,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>data array; null if EOF</returns>
        public byte[] RequestRead(byte[] handle, ulong offset, uint length)
        {
            SshException exception = null;

            var data = new byte[0];

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpReadRequest(ProtocolVersion, NextRequestId, handle, offset, length,
                    response =>
                        {
                            data = response.Data;
                            wait.Set();
                        },
                    response =>
                        {
                            if (response.StatusCode != StatusCodes.Eof)
                            {
                                exception = GetSftpException(response);
                            }
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
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
        /// <param name="writeCompleted">The callback to invoke when the write has completed.</param>
        public void RequestWrite(byte[] handle, ulong offset, byte[] data, AutoResetEvent wait, Action<SftpStatusResponse> writeCompleted = null)
        {
            SshException exception = null;

            var request = new SftpWriteRequest(ProtocolVersion, NextRequestId, handle, offset, data,
                response =>
                    {
                        if (writeCompleted != null)
                        {
                            writeCompleted(response);
                        }

                        exception = GetSftpException(response);
                        if (wait != null)
                            wait.Set();
                    });

            SendRequest(request);

            if (wait != null)
                WaitOnHandle(wait, OperationTimeout);

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        public SftpFileAttributes RequestLStat(string path)
        {
            SshException exception = null;

            SftpFileAttributes attributes = null;
            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpLStatRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            attributes = response.Attributes;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_FSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        public SftpFileAttributes RequestFStat(byte[] handle)
        {
            SshException exception = null;
            SftpFileAttributes attributes = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpFStatRequest(ProtocolVersion, NextRequestId, handle,
                    response =>
                        {
                            attributes = response.Attributes;
                            wait.Set();
                        },
                    (response) =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_SETSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="attributes">The attributes.</param>
        public void RequestSetStat(string path, SftpFileAttributes attributes)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpSetStatRequest(ProtocolVersion, NextRequestId, path, Encoding, attributes,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_FSETSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="attributes">The attributes.</param>
        public void RequestFSetStat(byte[] handle, SftpFileAttributes attributes)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpFSetStatRequest(ProtocolVersion, NextRequestId, handle, attributes,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_OPENDIR request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        public byte[] RequestOpenDir(string path, bool nullOnError = false)
        {
            SshException exception = null;

            byte[] handle = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpOpenDirRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            handle = response.Handle;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception != null)
            {
                throw exception;
            }

            return handle;
        }

        /// <summary>
        /// Performs SSH_FXP_READDIR request
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        public KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle)
        {
            SshException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpReadDirRequest(ProtocolVersion, NextRequestId, handle,
                    response =>
                        {
                            result = response.Files;
                            wait.Set();
                        },
                    response =>
                        {
                            if (response.StatusCode != StatusCodes.Eof)
                            {
                                exception = GetSftpException(response);
                            }
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        /// <summary>
        /// Performs SSH_FXP_REMOVE request.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RequestRemove(string path)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRemoveRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_MKDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RequestMkDir(string path)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpMkDirRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_RMDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RequestRmDir(string path)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRmDirRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
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
            SshException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRealPathRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            result = response.Files;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception != null)
            {
                throw exception;
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
            SshException exception = null;

            SftpFileAttributes attributes = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpStatRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            attributes = response.Attributes;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception != null)
            {
                throw exception;
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_RENAME request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        public void RequestRename(string oldPath, string newPath)
        {
            if (ProtocolVersion < 2)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_RENAME operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpRenameRequest(ProtocolVersion, NextRequestId, oldPath, newPath, Encoding,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
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
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_READLINK operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpReadLinkRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            result = response.Files;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception != null)
            {
                throw exception;
            }

            return result;
        }

        /// <summary>
        /// Performs SSH_FXP_SYMLINK request.
        /// </summary>
        /// <param name="linkpath">The linkpath.</param>
        /// <param name="targetpath">The targetpath.</param>
        public void RequestSymLink(string linkpath, string targetpath)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_SYMLINK operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new SftpSymLinkRequest(ProtocolVersion, NextRequestId, linkpath, targetpath, Encoding,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        #endregion

        #region SFTP Extended API functions

        /// <summary>
        /// Performs posix-rename@openssh.com extended request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        public void RequestPosixRename(string oldPath, string newPath)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new PosixRenameRequest(ProtocolVersion, NextRequestId, oldPath, newPath, Encoding,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                if (!_supportedExtensions.ContainsKey(request.Name))
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs statvfs@openssh.com extended request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> [null on error].</param>
        /// <returns></returns>
        public SftpFileSytemInformation RequestStatVfs(string path, bool nullOnError = false)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            SftpFileSytemInformation information = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new StatVfsRequest(ProtocolVersion, NextRequestId, path, Encoding,
                    response =>
                        {
                            information = response.GetReply<StatVfsReplyInfo>().Information;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                if (!_supportedExtensions.ContainsKey(request.Name))
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception != null)
            {
                throw exception;
            }

            return information;
        }

        /// <summary>
        /// Performs fstatvfs@openssh.com extended request.
        /// </summary>
        /// <param name="handle">The file handle.</param>
        /// <param name="nullOnError">if set to <c>true</c> [null on error].</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        internal SftpFileSytemInformation RequestFStatVfs(byte[] handle, bool nullOnError = false)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            SftpFileSytemInformation information = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new FStatVfsRequest(ProtocolVersion, NextRequestId, handle,
                    response =>
                        {
                            information = response.GetReply<StatVfsReplyInfo>().Information;
                            wait.Set();
                        },
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                if (!_supportedExtensions.ContainsKey(request.Name))
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }
            
            if (!nullOnError && exception != null)
            {
                throw exception;
            }

            return information;
        }

        /// <summary>
        /// Performs hardlink@openssh.com extended request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        internal void HardLink(string oldPath, string newPath)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            using (var wait = new AutoResetEvent(false))
            {
                var request = new HardLinkRequest(ProtocolVersion, NextRequestId, oldPath, newPath,
                    response =>
                        {
                            exception = GetSftpException(response);
                            wait.Set();
                        });

                if (!_supportedExtensions.ContainsKey(request.Name))
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        #endregion

        /// <summary>
        /// Calculates the optimal size of the buffer to read data from the channel.
        /// </summary>
        /// <param name="bufferSize">The buffer size configured on the client.</param>
        /// <returns>
        /// The optimal size of the buffer to read data from the channel.
        /// </returns>
        public uint CalculateOptimalReadLength(uint bufferSize)
        {
            // a SSH_FXP_DATA message has 13 bytes of protocol fields:
            // bytes 1 to 4: packet length
            // byte 5: message type
            // bytes 6 to 9: response id
            // bytes 10 to 13: length of payload‏
            //
            // most ssh servers limit the size of the payload of a SSH_MSG_CHANNEL_DATA
            // response to 16 KB; if we requested 16 KB of data, then the SSH_FXP_DATA
            // payload of the SSH_MSG_CHANNEL_DATA message would be too big (16 KB + 13 bytes), and
            // as a result, the ssh server would split this into two responses:
            // one containing 16384 bytes (13 bytes header, and 16371 bytes file data)
            // and one with the remaining 13 bytes of file data
            const uint lengthOfNonDataProtocolFields = 13u;
            var maximumPacketSize = Channel.LocalPacketSize;
            return Math.Min(bufferSize, maximumPacketSize) - lengthOfNonDataProtocolFields;
        }

        /// <summary>
        /// Calculates the optimal size of the buffer to write data on the channel.
        /// </summary>
        /// <param name="bufferSize">The buffer size configured on the client.</param>
        /// <param name="handle">The file handle.</param>
        /// <returns>
        /// The optimal size of the buffer to write data on the channel.
        /// </returns>
        /// <remarks>
        /// Currently, we do not take the remote window size into account.
        /// </remarks>
        public uint CalculateOptimalWriteLength(uint bufferSize, byte[] handle)
        {
            // 1-4: package length of SSH_FXP_WRITE message
            // 5: message type
            // 6-9: request id
            // 10-13: handle length
            // <handle>
            // 14-21: offset
            // 22-25: data length
            var lengthOfNonDataProtocolFields = 25u + (uint)handle.Length;
            var maximumPacketSize = Channel.RemotePacketSize;
            return Math.Min(bufferSize, maximumPacketSize) - lengthOfNonDataProtocolFields;
        }

        private SshException GetSftpException(SftpStatusResponse response)
        {
            if (response.StatusCode == StatusCodes.Ok)
            {
                return null;
            }
            if (response.StatusCode == StatusCodes.PermissionDenied)
            {
                return new SftpPermissionDeniedException(response.ErrorMessage);
            }
            if (response.StatusCode == StatusCodes.NoSuchFile)
            {
                return new SftpPathNotFoundException(response.ErrorMessage);
            }
            return new SshException(response.ErrorMessage);
        }

        private void HandleResponse(SftpResponse response)
        {
            SftpRequest request;
            lock (_requests)
            {
                _requests.TryGetValue(response.ResponseId, out request);
                if (request != null)
                {
                    _requests.Remove(response.ResponseId);
                }
            }

            if (request == null)
                throw new InvalidOperationException("Invalid response.");

            request.Complete(response);
        }
    }
}
