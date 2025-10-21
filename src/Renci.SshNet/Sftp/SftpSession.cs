using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Encoding _encoding;
        private System.Net.ArrayBuffer _buffer = new(32 * 1024);
        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(initialState: false);
        private IDictionary<string, string> _supportedExtensions;

        /// <inheritdoc/>
        public string WorkingDirectory { get; private set; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void ChangeDirectory(string path)
        {
            var fullPath = GetCanonicalPath(path);
            var handle = RequestOpenDir(fullPath);

            RequestClose(handle);
            WorkingDirectory = fullPath;
        }

        /// <inheritdoc/>
        public async Task ChangeDirectoryAsync(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = await GetCanonicalPathAsync(path, cancellationToken).ConfigureAwait(false);
            var handle = await RequestOpenDirAsync(fullPath, cancellationToken).ConfigureAwait(false);

            await RequestCloseAsync(handle, cancellationToken).ConfigureAwait(false);

            WorkingDirectory = fullPath;
        }

        internal void SendMessage(SftpMessage sftpMessage)
        {
            var data = sftpMessage.GetBytes();
            SendData(data);
        }

        /// <inheritdoc/>
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
#if NET
                fullPath.IndexOf('/', StringComparison.OrdinalIgnoreCase) < 0)
#else
                fullPath.IndexOf('/') < 0)
#endif
            {
                return fullPath;
            }

            var pathParts = fullPath.Split('/');

#if NET
            var partialFullPath = string.Join('/', pathParts, 0, pathParts.Length - 1);
#else
            var partialFullPath = string.Join("/", pathParts, 0, pathParts.Length - 1);
#endif

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

        /// <inheritdoc/>
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
#if NET
                fullPath.IndexOf('/', StringComparison.Ordinal) < 0)
#else
                fullPath.IndexOf('/') < 0)
#endif
            {
                return fullPath;
            }

            var pathParts = fullPath.Split('/');

#if NET
            var partialFullPath = string.Join('/', pathParts);
#else
            var partialFullPath = string.Join("/", pathParts);
#endif

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

        protected override void OnDataReceived(ArraySegment<byte> data)
        {
            // If the buffer is empty then skip a copy and read packets
            // directly out of the given data.
            if (_buffer.ActiveLength == 0)
            {
                while (data.Count >= 4)
                {
                    var packetLength = BinaryPrimitives.ReadInt32BigEndian(data);

                    if (data.Count - 4 < packetLength)
                    {
                        break;
                    }

                    if (!TryLoadSftpMessage(data.Slice(4, packetLength)))
                    {
                        // An error occured.
                        return;
                    }

                    data = data.Slice(4 + packetLength);
                }

                if (data.Count > 0)
                {
                    // Now buffer the remainder.
                    _buffer.EnsureAvailableSpace(data.Count);
                    data.AsSpan().CopyTo(_buffer.AvailableSpan);
                    _buffer.Commit(data.Count);
                }

                return;
            }

            // The buffer already had some data. Append the new data and
            // proceed with reading out packets.
            _buffer.EnsureAvailableSpace(data.Count);
            data.AsSpan().CopyTo(_buffer.AvailableSpan);
            _buffer.Commit(data.Count);

            while (_buffer.ActiveLength >= 4)
            {
                data = new ArraySegment<byte>(
                    _buffer.DangerousGetUnderlyingBuffer(),
                    _buffer.ActiveStartOffset,
                    _buffer.ActiveLength);

                var packetLength = BinaryPrimitives.ReadInt32BigEndian(data);

                if (data.Count - 4 < packetLength)
                {
                    break;
                }

                // Note: the packet data in the buffer is safe to read from
                // only for the duration of this load. If it needs to be stored,
                // callees should make their own copy.
                _ = TryLoadSftpMessage(data.Slice(4, packetLength));

                _buffer.Discard(4 + packetLength);
            }
        }

        private bool TryLoadSftpMessage(ArraySegment<byte> packetData)
        {
            // Create SFTP message
            var response = _sftpResponseFactory.Create(ProtocolVersion, packetData.Array[packetData.Offset], _encoding);

            // Load message data into it
            response.Load(packetData.Array, packetData.Offset + 1, packetData.Count - 1);

            try
            {
                if (response is SftpVersionResponse versionResponse)
                {
                    ProtocolVersion = versionResponse.Version;
                    _supportedExtensions = versionResponse.Extensions;

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

        /// <inheritdoc/>
        public byte[] RequestOpen(string path, Flags flags)
        {
            byte[] handle = null;
            SftpException exception = null;

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
                                                      wait.SetIgnoringObjectDisposed();
                                                  },
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response, path);
                                                      wait.SetIgnoringObjectDisposed();
                                                  });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }

            return handle;
        }

        /// <inheritdoc/>
        public Task<byte[]> RequestOpenAsync(string path, Flags flags, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpOpenRequest(ProtocolVersion,
                                                NextRequestId,
                                                path,
                                                _encoding,
                                                flags,
                                                response => tcs.TrySetResult(response.Handle),
                                                response => tcs.TrySetException(GetSftpException(response, path))));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public void RequestClose(byte[] handle)
        {
            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpCloseRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   handle,
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       wait.SetIgnoringObjectDisposed();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public Task RequestCloseAsync(byte[] handle, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpCloseRequest(ProtocolVersion,
                                             NextRequestId,
                                             handle,
                                             response =>
                                             {
                                                 if (response.StatusCode == StatusCode.Ok)
                                                 {
                                                     _ = tcs.TrySetResult(true);
                                                 }
                                                 else
                                                 {
                                                     _ = tcs.TrySetException(GetSftpException(response));
                                                 }
                                             }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public byte[] RequestRead(byte[] handle, ulong offset, uint length)
        {
            SftpException exception = null;

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
                                                      wait.SetIgnoringObjectDisposed();
                                                  },
                                                  response =>
                                                  {
                                                      if (response.StatusCode != StatusCode.Eof)
                                                      {
                                                          exception = GetSftpException(response);
                                                      }
                                                      else
                                                      {
                                                          data = Array.Empty<byte>();
                                                      }

                                                      wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public Task<byte[]> RequestReadAsync(byte[] handle, ulong offset, uint length, CancellationToken cancellationToken)
        {
            Debug.Assert(length > 0, "This implementation cannot distinguish between EOF and zero-length reads");

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpReadRequest(ProtocolVersion,
                                            NextRequestId,
                                            handle,
                                            offset,
                                            length,
                                            response => tcs.TrySetResult(response.Data),
                                            response =>
                                            {
                                                if (response.StatusCode == StatusCode.Eof)
                                                {
                                                    _ = tcs.TrySetResult(Array.Empty<byte>());
                                                }
                                                else
                                                {
                                                    _ = tcs.TrySetException(GetSftpException(response));
                                                }
                                            }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public void RequestWrite(byte[] handle,
                                 ulong serverOffset,
                                 byte[] data,
                                 int offset,
                                 int length,
                                 AutoResetEvent wait,
                                 Action<SftpStatusResponse> writeCompleted = null)
        {
            Debug.Assert((wait is null) != (writeCompleted is null), "Should have one parameter or the other.");

            SftpException exception = null;

            var request = new SftpWriteRequest(ProtocolVersion,
                                               NextRequestId,
                                               handle,
                                               serverOffset,
                                               data,
                                               offset,
                                               length,
                                               response =>
                                               {
                                                   if (writeCompleted is not null)
                                                   {
                                                       writeCompleted.Invoke(response);
                                                   }
                                                   else
                                                   {
                                                       exception = GetSftpException(response);
                                                       wait.SetIgnoringObjectDisposed();
                                                   }
                                               });

            SendRequest(request);

            if (wait is not null)
            {
                WaitOnHandle(wait, OperationTimeout);

                if (exception is not null)
                {
                    throw exception;
                }
            }
        }

        /// <inheritdoc/>
        public Task RequestWriteAsync(byte[] handle, ulong serverOffset, byte[] data, int offset, int length, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpWriteRequest(ProtocolVersion,
                                                NextRequestId,
                                                handle,
                                                serverOffset,
                                                data,
                                                offset,
                                                length,
                                                response =>
                                                {
                                                    if (response.StatusCode == StatusCode.Ok)
                                                    {
                                                        _ = tcs.TrySetResult(true);
                                                    }
                                                    else
                                                    {
                                                        _ = tcs.TrySetException(GetSftpException(response));
                                                    }
                                                }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public SftpFileAttributes RequestLStat(string path)
        {
            SftpException exception = null;

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
                                                       wait.SetIgnoringObjectDisposed();
                                                   },
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response, path);
                                                       wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public Task<SftpFileAttributes> RequestLStatAsync(string path, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<SftpFileAttributes>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<SftpFileAttributes>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpLStatRequest(ProtocolVersion,
                                                NextRequestId,
                                                path,
                                                _encoding,
                                                response => tcs.TrySetResult(response.Attributes),
                                                response => tcs.TrySetException(GetSftpException(response, path))));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public SftpFileAttributes RequestFStat(byte[] handle)
        {
            SftpException exception = null;
            SftpFileAttributes attributes = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpFStatRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   handle,
                                                   response =>
                                                   {
                                                       attributes = response.Attributes;
                                                       wait.SetIgnoringObjectDisposed();
                                                   },
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public Task<SftpFileAttributes> RequestFStatAsync(byte[] handle, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<SftpFileAttributes>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<SftpFileAttributes>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpFStatRequest(ProtocolVersion,
                                             NextRequestId,
                                             handle,
                                             response => tcs.TrySetResult(response.Attributes),
                                             response => tcs.TrySetException(GetSftpException(response))));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public void RequestSetStat(string path, SftpFileAttributes attributes)
        {
            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpSetStatRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     path,
                                                     _encoding,
                                                     attributes,
                                                     response =>
                                                     {
                                                         exception = GetSftpException(response, path);
                                                         wait.SetIgnoringObjectDisposed();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public void RequestFSetStat(byte[] handle, SftpFileAttributes attributes)
        {
            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpFSetStatRequest(ProtocolVersion,
                                                      NextRequestId,
                                                      handle,
                                                      attributes,
                                                      response =>
                                                      {
                                                          exception = GetSftpException(response);
                                                          wait.SetIgnoringObjectDisposed();
                                                      });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public byte[] RequestOpenDir(string path)
        {
            SftpException exception = null;

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
                                                         wait.SetIgnoringObjectDisposed();
                                                     },
                                                     response =>
                                                     {
                                                         exception = GetSftpException(response, path);
                                                         wait.SetIgnoringObjectDisposed();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }

            return handle;
        }

        /// <inheritdoc/>
        public Task<byte[]> RequestOpenDirAsync(string path, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpOpenDirRequest(ProtocolVersion,
                                               NextRequestId,
                                               path,
                                               _encoding,
                                               response => tcs.TrySetResult(response.Handle),
                                               response => tcs.TrySetException(GetSftpException(response, path))));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle)
        {
            SftpException exception = null;

            KeyValuePair<string, SftpFileAttributes>[] result = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpReadDirRequest(ProtocolVersion,
                                                     NextRequestId,
                                                     handle,
                                                     response =>
                                                     {
                                                         result = response.Files;
                                                         wait.SetIgnoringObjectDisposed();
                                                     },
                                                     response =>
                                                     {
                                                         if (response.StatusCode != StatusCode.Eof)
                                                         {
                                                             exception = GetSftpException(response);
                                                         }

                                                         wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public Task<KeyValuePair<string, SftpFileAttributes>[]> RequestReadDirAsync(byte[] handle, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<KeyValuePair<string, SftpFileAttributes>[]>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpReadDirRequest(ProtocolVersion,
                                               NextRequestId,
                                               handle,
                                               response => tcs.TrySetResult(response.Files),
                                               response =>
                                               {
                                                   if (response.StatusCode == StatusCode.Eof)
                                                   {
                                                       _ = tcs.TrySetResult(null);
                                                   }
                                                   else
                                                   {
                                                       _ = tcs.TrySetException(GetSftpException(response));
                                                   }
                                               }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public void RequestRemove(string path)
        {
            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpRemoveRequest(ProtocolVersion,
                                                    NextRequestId,
                                                    path,
                                                    _encoding,
                                                    response =>
                                                    {
                                                        exception = GetSftpException(response, path);
                                                        wait.SetIgnoringObjectDisposed();
                                                    });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public Task RequestRemoveAsync(string path, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpRemoveRequest(ProtocolVersion,
                                                NextRequestId,
                                                path,
                                                _encoding,
                                                response =>
                                                {
                                                    if (response.StatusCode == StatusCode.Ok)
                                                    {
                                                        _ = tcs.TrySetResult(true);
                                                    }
                                                    else
                                                    {
                                                        _ = tcs.TrySetException(GetSftpException(response, path));
                                                    }
                                                }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public void RequestMkDir(string path)
        {
            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpMkDirRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   path,
                                                   _encoding,
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response);
                                                       wait.SetIgnoringObjectDisposed();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public Task RequestMkDirAsync(string path, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpMkDirRequest(ProtocolVersion,
                                             NextRequestId,
                                             path,
                                             _encoding,
                                             response =>
                                                 {
                                                     if (response.StatusCode == StatusCode.Ok)
                                                     {
                                                         _ = tcs.TrySetResult(true);
                                                     }
                                                     else
                                                     {
                                                         _ = tcs.TrySetException(GetSftpException(response));
                                                     }
                                                 }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public void RequestRmDir(string path)
        {
            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new SftpRmDirRequest(ProtocolVersion,
                                                   NextRequestId,
                                                   path,
                                                   _encoding,
                                                   response =>
                                                   {
                                                       exception = GetSftpException(response, path);
                                                       wait.SetIgnoringObjectDisposed();
                                                   });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc />
        public Task RequestRmDirAsync(string path, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpRmDirRequest(ProtocolVersion,
                                             NextRequestId,
                                             path,
                                             _encoding,
                                             response =>
                                                 {
                                                     var exception = GetSftpException(response, path);
                                                     if (exception is not null)
                                                     {
                                                         _ = tcs.TrySetException(exception);
                                                     }
                                                     else
                                                     {
                                                         _ = tcs.TrySetResult(true);
                                                     }
                                                 }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
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
            SftpException exception = null;

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
                                                          wait.SetIgnoringObjectDisposed();
                                                      },
                                                      response =>
                                                      {
                                                          exception = GetSftpException(response, path);
                                                          wait.SetIgnoringObjectDisposed();
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

        internal Task<KeyValuePair<string, SftpFileAttributes>[]> RequestRealPathAsync(string path, bool nullOnError, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<KeyValuePair<string, SftpFileAttributes>[]>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<KeyValuePair<string, SftpFileAttributes>[]>(TaskCreationOptions.RunContinuationsAsynchronously);

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
                                                        _ = tcs.TrySetException(GetSftpException(response, path));
                                                    }
                                                }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
        }

        /// <inheritdoc/>
        public SftpFileAttributes RequestStat(string path)
        {
            SftpException exception = null;

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
                                                      wait.SetIgnoringObjectDisposed();
                                                  },
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response, path);
                                                      wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public void RequestRename(string oldPath, string newPath)
        {
            if (ProtocolVersion < 2)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_RENAME operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SftpException exception = null;

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
                                                        wait.SetIgnoringObjectDisposed();
                                                    });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public Task RequestRenameAsync(string oldPath, string newPath, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new SftpRenameRequest(ProtocolVersion,
                                                NextRequestId,
                                                oldPath,
                                                newPath,
                                                _encoding,
                                                response =>
                                                {
                                                    if (response.StatusCode == StatusCode.Ok)
                                                    {
                                                        _ = tcs.TrySetResult(true);
                                                    }
                                                    else
                                                    {
                                                        _ = tcs.TrySetException(GetSftpException(response));
                                                    }
                                                }));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
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

            SftpException exception = null;

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
                                                          wait.SetIgnoringObjectDisposed();
                                                      },
                                                      response =>
                                                      {
                                                          exception = GetSftpException(response, path);
                                                          wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public void RequestSymLink(string linkpath, string targetpath)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_SYMLINK operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SftpException exception = null;

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
                                                         wait.SetIgnoringObjectDisposed();
                                                     });

                SendRequest(request);

                WaitOnHandle(wait, OperationTimeout);
            }

            if (exception is not null)
            {
                throw exception;
            }
        }

        /// <inheritdoc/>
        public void RequestPosixRename(string oldPath, string newPath)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SftpException exception = null;

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
                                                         wait.SetIgnoringObjectDisposed();
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

        /// <inheritdoc/>
        public SftpFileSystemInformation RequestStatVfs(string path)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            SftpException exception = null;

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
                                                     wait.SetIgnoringObjectDisposed();
                                                 },
                                                 response =>
                                                 {
                                                     exception = GetSftpException(response, path);
                                                     wait.SetIgnoringObjectDisposed();
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

            Debug.Assert(information is not null);

            return information;
        }

        /// <inheritdoc/>
        public Task<SftpFileSystemInformation> RequestStatVfsAsync(string path, CancellationToken cancellationToken)
        {
            if (ProtocolVersion < 3)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", ProtocolVersion));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<SftpFileSystemInformation>(cancellationToken);
            }

            var tcs = new TaskCompletionSource<SftpFileSystemInformation>(TaskCreationOptions.RunContinuationsAsynchronously);

            SendRequest(new StatVfsRequest(ProtocolVersion,
                                            NextRequestId,
                                            path,
                                            _encoding,
                                            response => tcs.TrySetResult(response.GetReply<StatVfsReplyInfo>().Information),
                                            response => tcs.TrySetException(GetSftpException(response, path))));

            return WaitOnHandleAsync(tcs, OperationTimeout, cancellationToken);
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

            SftpException exception = null;

            SftpFileSystemInformation information = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new FStatVfsRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  handle,
                                                  response =>
                                                  {
                                                      information = response.GetReply<StatVfsReplyInfo>().Information;
                                                      wait.SetIgnoringObjectDisposed();
                                                  },
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response);
                                                      wait.SetIgnoringObjectDisposed();
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

            SftpException exception = null;

            using (var wait = new AutoResetEvent(initialState: false))
            {
                var request = new HardLinkRequest(ProtocolVersion,
                                                  NextRequestId,
                                                  oldPath,
                                                  newPath,
                                                  response =>
                                                  {
                                                      exception = GetSftpException(response);
                                                      wait.SetIgnoringObjectDisposed();
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

        internal static SftpException GetSftpException(SftpStatusResponse response, string path = null)
        {
#pragma warning disable IDE0010 // Add missing cases
            switch (response.StatusCode)
            {
                case StatusCode.Ok:
                    return null;
                case StatusCode.PermissionDenied:
                    return new SftpPermissionDeniedException(response.ErrorMessage);
                case StatusCode.NoSuchFile:

                    var message = response.ErrorMessage;

                    if (!string.IsNullOrEmpty(message) && path is not null)
                    {
                        message = $"{message}{(message[^1] == '.' ? " " : ". ")}Path: '{path}'.";
                    }

                    return new SftpPathNotFoundException(message, path);

                default:
                    return new SftpException(response.StatusCode, response.ErrorMessage);
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
