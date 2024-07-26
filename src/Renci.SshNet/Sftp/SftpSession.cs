﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents an SFTP session.
    /// </summary>
    internal sealed class SftpSession : SubsystemSession, ISftpSession
    {
        internal const int MaximumSupportedVersion = 3;
        private const int MinimumSupportedVersion = 0;

        private readonly Dictionary<uint, SftpRequest> _requests = new Dictionary<uint, SftpRequest>();
        private readonly ISftpResponseFactory _sftpResponseFactory;
        private readonly List<byte> _data = new List<byte>(32 * 1024);
        private readonly Encoding _encoding;
        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(initialState: false);
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
                return (uint)Interlocked.Increment(ref _requestId);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpSession"/> class.
        /// </summary>
        /// <param name="session">The SSH session.</param>
        /// <param name="operationTimeout">The operation timeout.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="sftpResponseFactory">The factory to create SFTP responses.</param>
        public SftpSession(ISession session, int operationTimeout, Encoding encoding, ISftpResponseFactory sftpResponseFactory)
            : base(session, "sftp", operationTimeout)
        {
            _encoding = encoding;
            _sftpResponseFactory = sftpResponseFactory;
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
            var data = sftpMessage.GetBytes();
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

            var realPathFiles = RequestRealPath(fullPath, nullOnError: true);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles[0].Key;
            }

            if (!string.IsNullOrEmpty(canonizedPath))
            {
                return canonizedPath;
            }

            // Check for special cases
            if (fullPath.EndsWith("/.", StringComparison.OrdinalIgnoreCase) ||
                fullPath.EndsWith("/..", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Equals("/", StringComparison.OrdinalIgnoreCase) ||
#if NET || NETSTANDARD2_1_OR_GREATER
                fullPath.IndexOf('/', StringComparison.OrdinalIgnoreCase) < 0)
#else
                fullPath.IndexOf('/') < 0)
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                return fullPath;
            }

            var pathParts = fullPath.Split('/');

#if NET || NETSTANDARD2_1_OR_GREATER
            var partialFullPath = string.Join('/', pathParts, 0, pathParts.Length - 1);
#else
            var partialFullPath = string.Join("/", pathParts, 0, pathParts.Length - 1);
#endif // NET || NETSTANDARD2_1_OR_GREATER

            if (string.IsNullOrEmpty(partialFullPath))
            {
                partialFullPath = "/";
            }

            realPathFiles = RequestRealPath(partialFullPath, nullOnError: true);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles[0].Key;
            }

            if (string.IsNullOrEmpty(canonizedPath))
            {
                return fullPath;
            }

            var slash = string.Empty;
            if (canonizedPath[canonizedPath.Length - 1] != '/')
            {
                slash = "/";
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", canonizedPath, slash, pathParts[pathParts.Length - 1]);
        }

        /// <summary>
        /// Asynchronously resolves a given path into an absolute path on the server.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the absolute path.
        /// </returns>
        public async Task<string> GetCanonicalPathAsync(string path, CancellationToken cancellationToken)
        {
            var fullPath = GetFullRemotePath(path);

            var canonizedPath = string.Empty;
            var realPathFiles = await RequestRealPathAsync(fullPath, nullOnError: true, cancellationToken).ConfigureAwait(false);
            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles[0].Key;
            }

            if (!string.IsNullOrEmpty(canonizedPath))
            {
                return canonizedPath;
            }

            // Check for special cases
            if (fullPath.EndsWith("/.", StringComparison.Ordinal) ||
                fullPath.EndsWith("/..", StringComparison.Ordinal) ||
                fullPath.Equals("/", StringComparison.Ordinal) ||
#if NET || NETSTANDARD2_1_OR_GREATER
                fullPath.IndexOf('/', StringComparison.Ordinal) < 0)
#else
                fullPath.IndexOf('/') < 0)
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                return fullPath;
            }

            var pathParts = fullPath.Split('/');

#if NET || NETSTANDARD2_1_OR_GREATER
            var partialFullPath = string.Join('/', pathParts);
#else
            var partialFullPath = string.Join("/", pathParts);
#endif // NET || NETSTANDARD2_1_OR_GREATER

            if (string.IsNullOrEmpty(partialFullPath))
            {
                partialFullPath = "/";
            }

            realPathFiles = await RequestRealPathAsync(partialFullPath, nullOnError: true, cancellationToken).ConfigureAwait(false);

            if (realPathFiles != null)
            {
                canonizedPath = realPathFiles[0].Key;
            }

            if (string.IsNullOrEmpty(canonizedPath))
            {
                return fullPath;
            }

            var slash = string.Empty;
            if (canonizedPath[canonizedPath.Length - 1] != '/')
            {
                slash = "/";
            }

            return canonizedPath + slash + pathParts[pathParts.Length - 1];
        }

        /// <summary>
        /// Creates an <see cref="ISftpFileReader"/> for reading the content of the file represented by a given <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The handle of the file to read.</param>
        /// <param name="sftpSession">The SFTP session.</param>
        /// <param name="chunkSize">The maximum number of bytes to read with each chunk.</param>
        /// <param name="maxPendingReads">The maximum number of pending reads.</param>
        /// <param name="fileSize">The size of the file or <see langword="null"/> when the size could not be determined.</param>
        /// <returns>
        /// An <see cref="ISftpFileReader"/> for reading the content of the file represented by the
        /// specified <paramref name="handle"/>.
        /// </returns>
        public ISftpFileReader CreateFileReader(byte[] handle, ISftpSession sftpSession, uint chunkSize, int maxPendingReads, long? fileSize)
        {
            return new SftpFileReader(handle, sftpSession, chunkSize, maxPendingReads, fileSize);
        }

        internal string GetFullRemotePath(string path)
        {
            var fullPath = path;

            if (!string.IsNullOrEmpty(path) && path[0] != '/' && WorkingDirectory != null)
            {
                if (WorkingDirectory[WorkingDirectory.Length - 1] == '/')
                {
                    fullPath = WorkingDirectory + path;
                }
                else
                {
                    fullPath = WorkingDirectory + '/' + path;
                }
            }

            return fullPath;
        }

        protected override void OnChannelOpen()
        {
            SendMessage(new SftpInitRequest(MaximumSupportedVersion));

            WaitOnHandle(_sftpVersionConfirmed, OperationTimeout);

            if (ProtocolVersion is > MaximumSupportedVersion or < MinimumSupportedVersion)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Server SFTP version {0} is not supported.", ProtocolVersion));
            }

            // Resolve current directory
            WorkingDirectory = RequestRealPath(".")[0].Key;
        }

        protected override void OnDataReceived(byte[] data)
        {
            const int packetLengthByteCount = 4;
            const int sftpMessageTypeByteCount = 1;
            const int minimumChannelDataLength = packetLengthByteCount + sftpMessageTypeByteCount;

            var offset = 0;
            var count = data.Length;

            // improve performance and reduce GC pressure by not buffering channel data if the received
            // chunk contains the complete packet data.
            //
            // for this, the buffer should be empty and the chunk should contain at least the packet length
            // and the type of the SFTP message
            if (_data.Count == 0)
            {
                while (count >= minimumChannelDataLength)
                {
                    // extract packet length
                    var packetDataLength = data[offset] << 24 | data[offset + 1] << 16 | data[offset + 2] << 8 |
                                           data[offset + 3];

                    var packetTotalLength = packetDataLength + packetLengthByteCount;

                    // check if complete packet data (or more) is available
                    if (count >= packetTotalLength)
                    {
                        // load and process SFTP message
                        if (!TryLoadSftpMessage(data, offset + packetLengthByteCount, packetDataLength))
                        {
                            return;
                        }

                        // remove processed bytes from the number of bytes to process as the channel
                        // data we received may contain (part of) another message
                        count -= packetTotalLength;

                        // move offset beyond bytes we just processed
                        offset += packetTotalLength;
                    }
                    else
                    {
                        // we don't have a complete message
                        break;
                    }
                }

                // check if there is channel data left to process or buffer
                if (count == 0)
                {
                    return;
                }

                // check if we processed part of the channel data we received
                if (offset > 0)
                {
                    // add (remaining) channel data to internal data holder
                    var remainingChannelData = new byte[count];
                    Buffer.BlockCopy(data, offset, remainingChannelData, 0, count);
                    _data.AddRange(remainingChannelData);
                }
                else
                {
                    // add (remaining) channel data to internal data holder
                    _data.AddRange(data);
                }

                // skip further processing as we'll need a new chunk to complete the message
                return;
            }

            // add (remaining) channel data to internal data holder
            _data.AddRange(data);

            while (_data.Count >= minimumChannelDataLength)
            {
                // extract packet length
                var packetDataLength = _data[0] << 24 | _data[1] << 16 | _data[2] << 8 | _data[3];

                var packetTotalLength = packetDataLength + packetLengthByteCount;

                // check if complete packet data is available
                if (_data.Count < packetTotalLength)
                {
                    // wait for complete message to arrive first
                    break;
                }

                // create buffer to hold packet data
                var packetData = new byte[packetDataLength];

                // copy packet data and bytes for length to array
                _data.CopyTo(packetLengthByteCount, packetData, 0, packetDataLength);

                // remove loaded data and bytes for length from _data holder
                if (_data.Count == packetTotalLength)
                {
                    // the only buffered data is the data we're processing
                    _data.Clear();
                }
                else
                {
                    // remove only the data we're processing
                    _data.RemoveRange(0, packetTotalLength);
                }

                // load and process SFTP message
                if (!TryLoadSftpMessage(packetData, 0, packetDataLength))
                {
                    break;
                }
            }
        }

        private bool TryLoadSftpMessage(byte[] packetData, int offset, int count)
        {
            // Create SFTP message
            var response = _sftpResponseFactory.Create(ProtocolVersion, packetData[offset], _encoding);

            // Load message data into it
            response.Load(packetData, offset + 1, count - 1);

            try
            {
                if (response is SftpVersionResponse versionResponse)
                {
                    ProtocolVersion = versionResponse.Version;
                    _supportedExtensions = versionResponse.Extentions;

                    _ = _sftpVersionConfirmed.Set();
                }
                else
                {
                    HandleResponse(response as SftpResponse);
                }

                return true;
            }
            catch (Exception exp)
            {
                RaiseError(exp);
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                var sftpVersionConfirmed = _sftpVersionConfirmed;
                if (sftpVersionConfirmed != null)
                {
                    _sftpVersionConfirmed = null;
                    sftpVersionConfirmed.Dispose();
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

        /// <summary>
        /// Performs SSH_FXP_OPEN request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="nullOnError">If set to <see langword="true"/> returns <see langword="null"/> instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        public byte[] RequestOpen(string path, Flags flags, bool nullOnError = false)
        {
            byte[] handle = null;
            SshException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpOpenRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  path,
                                                  _encoding,
                                                  flags,
                                                  response =>
                                                  {
                                                      handle = response.Handle;
                                                      _ = wait.Set();
                                                  },
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response);
                                                      _ = wait.Set();
                                                  });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
            {
                throw exception;
            }

            return handle;
        }

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_OPEN</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_OPEN</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the file handle of the specified path.
        /// </returns>
        public async Task<byte[]> RequestOpenAsync(string path, Flags flags, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<byte[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<byte[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpOpenRequest(ProtocolVersion,
                                                    NextRequestId,
                                                    path,
                                                    _encoding,
                                                    flags,
                                                    response => tcs.TrySetResult(response.Handle),
                                                    response => tcs.TrySetException(GetSftpException(response))));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_OPEN request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginOpen(string, Flags, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpOpenAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        public SftpOpenAsyncResult BeginOpen(string path, Flags flags, AsyncCallback callback, object state)
        {
            var asyncResult = new SftpOpenAsyncResult(callback, state);

            var request = new SftpOpenRequest(ProtocolVersion,
                                              NextRequestId,
                                              path,
                                              _encoding,
                                              flags,
                                              response =>
                                              {
                                                  asyncResult.SetAsCompleted(response.Handle, completedSynchronously: false);
                                              },
                                              response =>
                                              {
                                                  asyncResult.SetAsCompleted(GetSftpException(response), completedSynchronously: false);
                                              });

            SendRequest(request);

            return asyncResult;
        }

        /// <summary>
        /// Handles the end of an asynchronous open.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpOpenAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// A <see cref="byte"/> array representing a file handle.
        /// </returns>
        /// <remarks>
        /// If all available data has been read, the <see cref="EndOpen(SftpOpenAsyncResult)"/> method completes
        /// immediately and returns zero bytes.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public byte[] EndOpen(SftpOpenAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult.EndInvokeCalled)
            {
                throw new InvalidOperationException("EndOpen has already been called.");
            }

            if (asyncResult.IsCompleted)
            {
                return asyncResult.EndInvoke();
            }

            using (var waitHandle = asyncResult.AsyncWaitHandle)
            {
                WaitOnHandle(waitHandle, OperationTimeout);
                return asyncResult.EndInvoke();
            }
        }

        /// <summary>
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        public void RequestClose(byte[] handle)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpCloseRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   handle,
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       _ = wait.Set();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs a <c>SSH_FXP_CLOSE</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_CLOSE</c> request.
        /// </returns>
        public async Task RequestCloseAsync(byte[] handle, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpCloseRequest(ProtocolVersion,
                                             NextRequestId,
                                             handle,
                                             response =>
                                             {
                                                 if (response.StatusCode == StatusCodes.Ok)
                                                 {
                                                     _ = tcs.TrySetResult(true);
                                                 }
                                                 else
                                                 {
                                                     _ = tcs.TrySetException(GetSftpException(response));
                                                 }
                                             }));

            // Only check for cancellation after the SftpCloseRequest was sent
            cancellationToken.ThrowIfCancellationRequested();

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                _ = await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginClose(byte[], AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpCloseAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        public SftpCloseAsyncResult BeginClose(byte[] handle, AsyncCallback callback, object state)
        {
            var asyncResult = new SftpCloseAsyncResult(callback, state);

            var request = new SftpCloseRequest(ProtocolVersion,
                                               NextRequestId,
                                               handle,
                                               response =>
                                               {
                                                   asyncResult.SetAsCompleted(GetSftpException(response), completedSynchronously: false);
                                               });
            SendRequest(request);

            return asyncResult;
        }

        /// <summary>
        /// Handles the end of an asynchronous close.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpCloseAsyncResult"/> that represents an asynchronous call.</param>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public void EndClose(SftpCloseAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult.EndInvokeCalled)
            {
                throw new InvalidOperationException("EndClose has already been called.");
            }

            if (asyncResult.IsCompleted)
            {
                asyncResult.EndInvoke();
            }
            else
            {
                using (var waitHandle = asyncResult.AsyncWaitHandle)
                {
                    WaitOnHandle(waitHandle, OperationTimeout);
                    asyncResult.EndInvoke();
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous read using a SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle to the file to read from.</param>
        /// <param name="offset">The offset in the file to start reading from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginRead(byte[], ulong, uint, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpReadAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        public SftpReadAsyncResult BeginRead(byte[] handle, ulong offset, uint length, AsyncCallback callback, object state)
        {
            var asyncResult = new SftpReadAsyncResult(callback, state);

            var request = new SftpReadRequest(ProtocolVersion,
                                              NextRequestId,
                                              handle,
                                              offset,
                                              length,
                                              response =>
                                              {
                                                  asyncResult.SetAsCompleted(response.Data, completedSynchronously: false);
                                              },
                                              response =>
                                              {
                                                  if (response.StatusCode != StatusCodes.Eof)
                                                  {
                                                      asyncResult.SetAsCompleted(GetSftpException(response), completedSynchronously: false);
                                                  }
                                                  else
                                                  {
                                                      asyncResult.SetAsCompleted(Array.Empty<byte>(), completedSynchronously: false);
                                                  }
                                              });
            SendRequest(request);

            return asyncResult;
        }

        /// <summary>
        /// Handles the end of an asynchronous read.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpReadAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// A <see cref="byte"/> array representing the data read.
        /// </returns>
        /// <remarks>
        /// If all available data has been read, the <see cref="EndRead(SftpReadAsyncResult)"/> method completes
        /// immediately and returns zero bytes.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public byte[] EndRead(SftpReadAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult.EndInvokeCalled)
            {
                throw new InvalidOperationException("EndRead has already been called.");
            }

            if (asyncResult.IsCompleted)
            {
                return asyncResult.EndInvoke();
            }

            using (var waitHandle = asyncResult.AsyncWaitHandle)
            {
                WaitOnHandle(waitHandle, OperationTimeout);
                return asyncResult.EndInvoke();
            }
        }

        /// <summary>
        /// Performs SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// The data that was read, or an empty array when the end of the file was reached.
        /// </returns>
        public byte[] RequestRead(byte[] handle, ulong offset, uint length)
        {
            SshException exception = null;

            byte[] data = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpReadRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  handle,
                                                  offset,
                                                  length,
                                                  response =>
                                                  {
                                                      data = response.Data;
                                                      _ = wait.Set();
                                                  },
                                                  response =>
                                                  {
                                                      if (response.StatusCode != StatusCodes.Eof)
                                                      {
                                                          exception = GetSftpException(response);
                                                      }
                                                      else
                                                      {
                                                          data = Array.Empty<byte>();
                                                      }

                                                      _ = wait.Set();
                                                  });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }

            return data;
        }

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_READ</c> request.
        /// </summary>
        /// <param name="handle">The handle to the file to read from.</param>
        /// <param name="offset">The offset in the file to start reading from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_READ</c> request. The value of
        /// its <see cref="Task{Task}.Result"/> contains the data read from the file, or an empty
        /// array when the end of the file is reached.
        /// </returns>
        public async Task<byte[]> RequestReadAsync(byte[] handle, ulong offset, uint length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<byte[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<byte[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpReadRequest(ProtocolVersion,
                                                NextRequestId,
                                                handle,
                                                offset,
                                                length,
                                                response => tcs.TrySetResult(response.Data),
                                                response =>
                                                {
                                                    if (response.StatusCode == StatusCodes.Eof)
                                                    {
                                                        _ = tcs.TrySetResult(Array.Empty<byte>());
                                                    }
                                                    else
                                                    {
                                                        _ = tcs.TrySetException(GetSftpException(response));
                                                    }
                                                }));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_WRITE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="serverOffset">The the zero-based offset (in bytes) relative to the beginning of the file that the write must start at.</param>
        /// <param name="data">The buffer holding the data to write.</param>
        /// <param name="offset">the zero-based offset in <paramref name="data" /> at which to begin taking bytes to write.</param>
        /// <param name="length">The length (in bytes) of the data to write.</param>
        /// <param name="wait">The wait event handle if needed.</param>
        /// <param name="writeCompleted">The callback to invoke when the write has completed.</param>
        public void RequestWrite(byte[] handle,
                                 ulong serverOffset,
                                 byte[] data,
                                 int offset,
                                 int length,
                                 AutoResetEvent wait,
                                 Action<SftpStatusResponse> writeCompleted = null)
        {
            SshException exception = null;

            var request = new SftpWriteRequest(ProtocolVersion,
                                               NextRequestId,
                                               handle,
                                               serverOffset,
                                               data,
                                               offset,
                                               length,
                                               response =>
                                               {
                                                   writeCompleted?.Invoke(response);

                                                   exception = GetSftpException(response);
                                                   if (wait != null)
                                                   {
                                                       _ = wait.Set();
                                                   }
                                               });

            SendRequest(request);

            if (wait is not null)
            {
                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Asynchronouly performs a <c>SSH_FXP_WRITE</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="serverOffset">The the zero-based offset (in bytes) relative to the beginning of the file that the write must start at.</param>
        /// <param name="data">The buffer holding the data to write.</param>
        /// <param name="offset">the zero-based offset in <paramref name="data" /> at which to begin taking bytes to write.</param>
        /// <param name="length">The length (in bytes) of the data to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_WRITE</c> request.
        /// </returns>
        public async Task RequestWriteAsync(byte[] handle, ulong serverOffset, byte[] data, int offset, int length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpWriteRequest(ProtocolVersion,
                                                 NextRequestId,
                                                 handle,
                                                 serverOffset,
                                                 data,
                                                 offset,
                                                 length,
                                                 response =>
                                                 {
                                                     if (response.StatusCode == StatusCodes.Ok)
                                                     {
                                                         _ = tcs.TrySetResult(true);
                                                     }
                                                     else
                                                     {
                                                         _ = tcs.TrySetException(GetSftpException(response));
                                                     }
                                                 }));

                _ = await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// File attributes.
        /// </returns>
        public SftpFileAttributes RequestLStat(string path)
        {
            SshException exception = null;

            SftpFileAttributes attributes = null;
            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpLStatRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   path,
                                                   _encoding,
                                                   response =>
                                                   {
                                                       attributes = response.Attributes;
                                                       _ = wait.Set();
                                                   },
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       _ = wait.Set();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginLStat(string, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SFtpStatAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        public SFtpStatAsyncResult BeginLStat(string path, AsyncCallback callback, object state)
        {
            var asyncResult = new SFtpStatAsyncResult(callback, state);

            var request = new SftpLStatRequest(ProtocolVersion,
                                               NextRequestId,
                                               path,
                                               _encoding,
                                               response =>
                                               {
                                                   asyncResult.SetAsCompleted(response.Attributes, completedSynchronously: false);
                                               },
                                               response =>
                                               {
                                                   asyncResult.SetAsCompleted(GetSftpException(response), completedSynchronously: false);
                                               });
            SendRequest(request);

            return asyncResult;
        }

        /// <summary>
        /// Handles the end of an asynchronous SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SFtpStatAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// The file attributes.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public SftpFileAttributes EndLStat(SFtpStatAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult.EndInvokeCalled)
            {
                throw new InvalidOperationException("EndLStat has already been called.");
            }

            if (asyncResult.IsCompleted)
            {
                return asyncResult.EndInvoke();
            }

            using (var waitHandle = asyncResult.AsyncWaitHandle)
            {
                WaitOnHandle(waitHandle, OperationTimeout);
                return asyncResult.EndInvoke();
            }
        }

        /// <summary>
        /// Performs SSH_FXP_FSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="nullOnError">If set to <see langword="true"/>, returns <see langword="null"/> instead of throwing an exception.</param>
        /// <returns>
        /// File attributes.
        /// </returns>
        public SftpFileAttributes RequestFStat(byte[] handle, bool nullOnError)
        {
            SshException exception = null;
            SftpFileAttributes attributes = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpFStatRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   handle,
                                                   response =>
                                                   {
                                                       attributes = response.Attributes;
                                                       _ = wait.Set();
                                                   },
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       _ = wait.Set();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
            {
                throw exception;
            }

            return attributes;
        }

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_FSTAT</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_FSTAT</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the file attributes of the specified handle.
        /// </returns>
        public async Task<SftpFileAttributes> RequestFStatAsync(byte[] handle, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<SftpFileAttributes>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<SftpFileAttributes>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<SftpFileAttributes>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpFStatRequest(ProtocolVersion,
                                                 NextRequestId,
                                                 handle,
                                                 response => tcs.TrySetResult(response.Attributes),
                                                 response => tcs.TrySetException(GetSftpException(response))));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_SETSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="attributes">The attributes.</param>
        public void RequestSetStat(string path, SftpFileAttributes attributes)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpSetStatRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     path,
                                                     _encoding,
                                                     attributes,
                                                     response =>
                                                     {
                                                         exception = GetSftpException(response);
                                                         _ = wait.Set();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
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

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpFSetStatRequest(ProtocolVersion,
                                                      NextRequestId,
                                                      handle,
                                                      attributes,
                                                      response =>
                                                      {
                                                          exception = GetSftpException(response);
                                                          _ = wait.Set();
                                                      });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_OPENDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">If set to <see langword="true"/>, returns <see langword="null"/> instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        public byte[] RequestOpenDir(string path, bool nullOnError = false)
        {
            SshException exception = null;

            byte[] handle = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpOpenDirRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     path,
                                                     _encoding,
                                                     response =>
                                                     {
                                                         handle = response.Handle;
                                                         _ = wait.Set();
                                                     },
                                                     response =>
                                                     {
                                                         exception = GetSftpException(response);
                                                         _ = wait.Set();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
            {
                throw exception;
            }

            return handle;
        }

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_OPENDIR</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_OPENDIR</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the handle of the specified path.
        /// </returns>
        public async Task<byte[]> RequestOpenDirAsync(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<byte[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<byte[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpOpenDirRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   path,
                                                   _encoding,
                                                   response => tcs.TrySetResult(response.Handle),
                                                   response => tcs.TrySetException(GetSftpException(response))));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_READDIR request.
        /// </summary>
        /// <param name="handle">The handle of the directory to read.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey,TValue}"/> where the <c>key</c> is the name of a file in
        /// the directory and the <c>value</c> is the <see cref="SftpFileAttributes"/> of the file.
        /// </returns>
        public KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle)
        {
            SshException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpReadDirRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     handle,
                                                     response =>
                                                     {
                                                         result = response.Files;
                                                         _ = wait.Set();
                                                     },
                                                     response =>
                                                     {
                                                         if (response.StatusCode != StatusCodes.Eof)
                                                         {
                                                             exception = GetSftpException(response);
                                                         }

                                                         _ = wait.Set();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }

            return result;
        }

        /// <summary>
        /// Performs a <c>SSH_FXP_READDIR</c> request.
        /// </summary>
        /// <param name="handle">The handle of the directory to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_READDIR</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains a <see cref="Dictionary{TKey,TValue}"/> where the
        /// <c>key</c> is the name of a file in the directory and the <c>value</c> is the <see cref="SftpFileAttributes"/>
        /// of the file.
        /// </returns>
        public async Task<KeyValuePair<string, SftpFileAttributes>[]> RequestReadDirAsync(byte[] handle, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpReadDirRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   handle,
                                                   response => tcs.TrySetResult(response.Files),
                                                   response =>
                                                   {
                                                       if (response.StatusCode == StatusCodes.Eof)
                                                       {
                                                           _ = tcs.TrySetResult(null);
                                                       }
                                                       else
                                                       {
                                                           _ = tcs.TrySetException(GetSftpException(response));
                                                       }
                                                   }));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_REMOVE request.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RequestRemove(string path)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpRemoveRequest(ProtocolVersion,
                                                    NextRequestId,
                                                    path,
                                                    _encoding,
                                                    response =>
                                                    {
                                                        exception = GetSftpException(response);
                                                        _ = wait.Set();
                                                    });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_REMOVE</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_REMOVE</c> request.
        /// </returns>
        public async Task RequestRemoveAsync(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpRemoveRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  path,
                                                  _encoding,
                                                  response =>
                                                  {
                                                      if (response.StatusCode == StatusCodes.Ok)
                                                      {
                                                          _ = tcs.TrySetResult(true);
                                                      }
                                                      else
                                                      {
                                                          _ = tcs.TrySetException(GetSftpException(response));
                                                      }
                                                  }));

                _ = await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_MKDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RequestMkDir(string path)
        {
            SshException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpMkDirRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   path,
                                                   _encoding,
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       _ = wait.Set();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
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

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpRmDirRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   path,
                                                   _encoding,
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       _ = wait.Set();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs SSH_FXP_REALPATH request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <see langword="true"/> returns null instead of throwing an exception.</param>
        /// <returns>
        /// The absolute path.
        /// </returns>
        internal KeyValuePair<string, SftpFileAttributes>[] RequestRealPath(string path, bool nullOnError = false)
        {
            SshException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpRealPathRequest(ProtocolVersion,
                                                      NextRequestId,
                                                      path,
                                                      _encoding,
                                                      response =>
                                                      {
                                                          result = response.Files;
                                                          _ = wait.Set();
                                                      },
                                                      response =>
                                                      {
                                                          exception = GetSftpException(response);
                                                          _ = wait.Set();
                                                      });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
            {
                throw exception;
            }

            return result;
        }

        internal async Task<KeyValuePair<string, SftpFileAttributes>[]> RequestRealPathAsync(string path, bool nullOnError, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpRealPathRequest(ProtocolVersion,
                                                    NextRequestId,
                                                    path,
                                                    _encoding,
                                                    response => tcs.TrySetResult(response.Files),
                                                    response =>
                                                    {
                                                        if (nullOnError)
                                                        {
                                                            _ = tcs.TrySetResult(null);
                                                        }
                                                        else
                                                        {
                                                            _ = tcs.TrySetException(GetSftpException(response));
                                                        }
                                                    }));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_REALPATH request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginRealPath(string, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpRealPathAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        public SftpRealPathAsyncResult BeginRealPath(string path, AsyncCallback callback, object state)
        {
            var asyncResult = new SftpRealPathAsyncResult(callback, state);

            var request = new SftpRealPathRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  path,
                                                  _encoding,
                                                  response => asyncResult.SetAsCompleted(response.Files[0].Key, completedSynchronously: false),
                                                  response => asyncResult.SetAsCompleted(GetSftpException(response), completedSynchronously: false));
            SendRequest(request);

            return asyncResult;
        }

        /// <summary>
        /// Handles the end of an asynchronous SSH_FXP_REALPATH request.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpRealPathAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// The absolute path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public string EndRealPath(SftpRealPathAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult.EndInvokeCalled)
            {
                throw new InvalidOperationException("EndRealPath has already been called.");
            }

            if (asyncResult.IsCompleted)
            {
                return asyncResult.EndInvoke();
            }

            using (var waitHandle = asyncResult.AsyncWaitHandle)
            {
                WaitOnHandle(waitHandle, OperationTimeout);
                return asyncResult.EndInvoke();
            }
        }

        /// <summary>
        /// Performs SSH_FXP_STAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <see langword="true"/> returns null instead of throwing an exception.</param>
        /// <returns>
        /// File attributes.
        /// </returns>
        public SftpFileAttributes RequestStat(string path, bool nullOnError = false)
        {
            SshException exception = null;

            SftpFileAttributes attributes = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpStatRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  path,
                                                  _encoding,
                                                  response =>
                                                  {
                                                      attributes = response.Attributes;
                                                      _ = wait.Set();
                                                  },
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response);
                                                      _ = wait.Set();
                                                  });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
            {
                throw exception;
            }

            return attributes;
        }

        /// <summary>
        /// Performs SSH_FXP_STAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginStat(string, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SFtpStatAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        public SFtpStatAsyncResult BeginStat(string path, AsyncCallback callback, object state)
        {
            var asyncResult = new SFtpStatAsyncResult(callback, state);

            var request = new SftpStatRequest(ProtocolVersion,
                                              NextRequestId,
                                              path,
                                              _encoding,
                                              response => asyncResult.SetAsCompleted(response.Attributes, completedSynchronously: false),
                                              response => asyncResult.SetAsCompleted(GetSftpException(response), completedSynchronously: false));
            SendRequest(request);

            return asyncResult;
        }

        /// <summary>
        /// Handles the end of an asynchronous stat.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SFtpStatAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// The file attributes.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public SftpFileAttributes EndStat(SFtpStatAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult.EndInvokeCalled)
            {
                throw new InvalidOperationException("EndStat has already been called.");
            }

            if (asyncResult.IsCompleted)
            {
                return asyncResult.EndInvoke();
            }

            using (var waitHandle = asyncResult.AsyncWaitHandle)
            {
                WaitOnHandle(waitHandle, OperationTimeout);
                return asyncResult.EndInvoke();
            }
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

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpRenameRequest(ProtocolVersion,
                                                    NextRequestId,
                                                    oldPath,
                                                    newPath,
                                                    _encoding,
                                                    response =>
                                                    {
                                                        exception = GetSftpException(response);
                                                        _ = wait.Set();
                                                    });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_RENAME</c> request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_RENAME</c> request.
        /// </returns>
        public async Task RequestRenameAsync(string oldPath, string newPath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new SftpRenameRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  oldPath,
                                                  newPath,
                                                  _encoding,
                                                  response =>
                                                  {
                                                      if (response.StatusCode == StatusCodes.Ok)
                                                      {
                                                          _ = tcs.TrySetResult(true);
                                                      }
                                                      else
                                                      {
                                                          _ = tcs.TrySetException(GetSftpException(response));
                                                      }
                                                  }));

                _ = await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs SSH_FXP_READLINK request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <see langword="true"/> returns <see langword="null"/> instead of throwing an exception.</param>
        /// <returns>
        /// An array of <see cref="KeyValuePair{TKey,TValue}"/> where the <c>key</c> is the name of
        /// a file and the <c>value</c> is the <see cref="SftpFileAttributes"/> of the file.
        /// </returns>
        internal KeyValuePair<string, SftpFileAttributes>[] RequestReadLink(string path, bool nullOnError = false)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_READLINK operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpReadLinkRequest(ProtocolVersion,
                                                      NextRequestId,
                                                      path,
                                                      _encoding,
                                                      response =>
                                                      {
                                                          result = response.Files;
                                                          _ = wait.Set();
                                                      },
                                                      response =>
                                                      {
                                                          exception = GetSftpException(response);
                                                          _ = wait.Set();
                                                      });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
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

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpSymLinkRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     linkpath,
                                                     targetpath,
                                                     _encoding,
                                                     response =>
                                                     {
                                                         exception = GetSftpException(response);
                                                         _ = wait.Set();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

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

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new PosixRenameRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     oldPath,
                                                     newPath,
                                                     _encoding,
                                                     response =>
                                                     {
                                                         exception = GetSftpException(response);
                                                         _ = wait.Set();
                                                     });

                if (!_supportedExtensions.ContainsKey(request.Name))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));
                }

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Performs statvfs@openssh.com extended request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <see langword="true"/> [null on error].</param>
        /// <returns>
        /// A <see cref="SftpFileSystemInformation"/> for the specified path.
        /// </returns>
        public SftpFileSystemInformation RequestStatVfs(string path, bool nullOnError = false)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            SftpFileSystemInformation information = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new StatVfsRequest(ProtocolVersion,
                                                 NextRequestId,
                                                 path,
                                                 _encoding,
                                                 response =>
                                                 {
                                                     information = response.GetReply<StatVfsReplyInfo>().Information;
                                                     _ = wait.Set();
                                                 },
                                                 response =>
                                                 {
                                                     exception = GetSftpException(response);
                                                     _ = wait.Set();
                                                 });

                if (!_supportedExtensions.ContainsKey(request.Name))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));
                }

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
            {
                throw exception;
            }

            return information;
        }

        /// <summary>
        /// Asynchronously performs a <c>statvfs@openssh.com</c> extended request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the <c>statvfs@openssh.com</c> extended request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the file system information for the specified
        /// path.
        /// </returns>
        public async Task<SftpFileSystemInformation> RequestStatVfsAsync(string path, CancellationToken cancellationToken)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<SftpFileSystemInformation>(TaskCreationOptions.RunContinuationsAsynchronously);

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(s => ((TaskCompletionSource<SftpFileSystemInformation>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(s => ((TaskCompletionSource<SftpFileSystemInformation>)s).TrySetCanceled(cancellationToken), tcs, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
                SendRequest(new StatVfsRequest(ProtocolVersion,
                                               NextRequestId,
                                               path,
                                               _encoding,
                                               response => tcs.TrySetResult(response.GetReply<StatVfsReplyInfo>().Information),
                                               response => tcs.TrySetException(GetSftpException(response))));

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs fstatvfs@openssh.com extended request.
        /// </summary>
        /// <param name="handle">The file handle.</param>
        /// <param name="nullOnError">if set to <see langword="true"/> [null on error].</param>
        /// <returns>
        /// A <see cref="SftpFileSystemInformation"/> for the specified path.
        /// </returns>
        /// <exception cref="NotSupportedException">This operation is not supported for the current SFTP protocol version.</exception>
        internal SftpFileSystemInformation RequestFStatVfs(byte[] handle, bool nullOnError = false)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SshException exception = null;

            SftpFileSystemInformation information = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new FStatVfsRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  handle,
                                                  response =>
                                                  {
                                                      information = response.GetReply<StatVfsReplyInfo>().Information;
                                                      _ = wait.Set();
                                                  },
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response);
                                                      _ = wait.Set();
                                                  });

                if (!_supportedExtensions.ContainsKey(request.Name))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));
                }

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (!nullOnError && exception is not null)
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

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new HardLinkRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  oldPath,
                                                  newPath,
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response);
                                                      _ = wait.Set();
                                                  });

                if (!_supportedExtensions.ContainsKey(request.Name))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", request.Name));
                }

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

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
            // bytes 10 to 13: length of payload
            //
            // WinSCP uses a payload length of 32755 bytes
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

            /*
             * Putty uses data length of 4096 bytes
             * WinSCP uses data length of 32739 bytes (total 32768 bytes; 32739 + 25 + 4 bytes for handle)
             */

            var lengthOfNonDataProtocolFields = 25u + (uint)handle.Length;
            var maximumPacketSize = Channel.RemotePacketSize;
            return Math.Min(bufferSize, maximumPacketSize) - lengthOfNonDataProtocolFields;
        }

        private static SshException GetSftpException(SftpStatusResponse response)
        {
#pragma warning disable IDE0010 // Add missing cases
            switch (response.StatusCode)
            {
                case StatusCodes.Ok:
                    return null;
                case StatusCodes.PermissionDenied:
                    return new SftpPermissionDeniedException(response.ErrorMessage);
                case StatusCodes.NoSuchFile:
                    return new SftpPathNotFoundException(response.ErrorMessage);
                default:
                    return new SshException(response.ErrorMessage);
            }
#pragma warning restore IDE0010 // Add missing cases
        }

        private void HandleResponse(SftpResponse response)
        {
            SftpRequest request;
            lock (_requests)
            {
                _ = _requests.TryGetValue(response.ResponseId, out request);
                if (request is not null)
                {
                    _ = _requests.Remove(response.ResponseId);
                }
            }

            if (request is null)
            {
                throw new InvalidOperationException("Invalid response.");
            }

            request.Complete(response);
        }
    }
}
