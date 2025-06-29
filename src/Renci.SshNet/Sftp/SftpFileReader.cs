#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

#if !NET
using Renci.SshNet.Common;
#endif

namespace Renci.SshNet.Sftp
{
    public sealed partial class SftpFileStream
    {
        private sealed class SftpFileReader : IDisposable
        {
            private readonly byte[] _handle;
            private readonly ISftpSession _sftpSession;
            private readonly int _maxPendingReads;
            private readonly ulong? _fileSize;
            private readonly Dictionary<ulong, Request> _requests = [];
            private readonly CancellationTokenSource _cts;

            private uint _chunkSize;

            private ulong _offset;
            private ulong _readAheadOffset;
            private int _currentMaxRequests;
            private ExceptionDispatchInfo? _exception;

            /// <summary>
            /// Initializes a new instance of the <see cref="SftpFileReader"/> class with the specified handle,
            /// <see cref="ISftpSession"/> and the maximum number of pending reads.
            /// </summary>
            /// <param name="handle">The file handle.</param>
            /// <param name="sftpSession">The SFTP session.</param>
            /// <param name="chunkSize">The size of a individual read-ahead chunk.</param>
            /// <param name="position">The starting offset in the file.</param>
            /// <param name="maxPendingReads">The maximum number of pending reads.</param>
            /// <param name="fileSize">The size of the file, if known; otherwise, <see langword="null"/>.</param>
            /// <param name="initialMaxRequests">The initial number of pending reads.</param>
            public SftpFileReader(byte[] handle, ISftpSession sftpSession, int chunkSize, long position, int maxPendingReads, ulong? fileSize = null, int initialMaxRequests = 1)
            {
                Debug.Assert(chunkSize > 0);
                Debug.Assert(position >= 0);
                Debug.Assert(initialMaxRequests >= 1);

                _handle = handle;
                _sftpSession = sftpSession;
                _chunkSize = (uint)chunkSize;
                _offset = _readAheadOffset = (ulong)position;
                _maxPendingReads = maxPendingReads;
                _fileSize = fileSize;
                _currentMaxRequests = initialMaxRequests;

                _cts = new CancellationTokenSource();
            }

            public async Task<byte[]> ReadAsync(CancellationToken cancellationToken)
            {
                _exception?.Throw();

                try
                {
                    // Fill up the requests buffer with as many requests as we currently allow.
                    // On the first call to Read, that number is 1. On the second it is 2 etc.
                    while (_requests.Count < _currentMaxRequests)
                    {
                        AddRequest(_readAheadOffset, _chunkSize);

                        _readAheadOffset += _chunkSize;
                    }

                    var request = _requests[_offset];

                    var data = await request.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

                    if (data.Length == 0)
                    {
                        // EOF. We effectively disable this instance - further reads will
                        // continue to return EOF.
                        _currentMaxRequests = 0;
                        return data;
                    }

                    _ = _requests.Remove(_offset);

                    _offset += (ulong)data.Length;

                    if (data.Length < request.Count)
                    {
                        // We didn't receive all the data we requested.
                        // Add another request to fill in the gap.
                        AddRequest(_offset, request.Count - (uint)data.Length);

                        if (data.Length < _chunkSize)
                        {
                            // Right-size the buffer to match the amount that the server
                            // is willing to return.
                            // Note that this also happens near EOF.
                            _chunkSize = Math.Max(512, (uint)data.Length);
                        }
                    }

                    if (_currentMaxRequests > 0)
                    {
                        if (_readAheadOffset > _fileSize)
                        {
                            _currentMaxRequests = 1;
                        }
                        else if (_currentMaxRequests < _maxPendingReads)
                        {
                            _currentMaxRequests++;
                        }
                    }

                    return data;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken))
                {
                    // If the wait was cancelled then we will allow subsequent reads as normal.
                    // For any other errors, we prevent further read requests, effectively disabling
                    // this instance.
                    _currentMaxRequests = 0;
                    _exception = ExceptionDispatchInfo.Capture(ex);
                    throw;
                }
            }

            private void AddRequest(ulong offset, uint count)
            {
                _requests.Add(
                    offset,
                    new Request(
                        offset,
                        count,
                        _sftpSession.RequestReadAsync(_handle, offset, count, _cts.Token)));
            }

            public void Dispose()
            {
                _exception ??= ExceptionDispatchInfo.Capture(new ObjectDisposedException(GetType().FullName));

                if (_requests.Count > 0)
                {
                    // Cancel outstanding requests and observe the exception on them
                    // as an effort to prevent unhandled exceptions.

                    _cts.Cancel();

                    foreach (var request in _requests.Values)
                    {
                        _ = request.Task.Exception;
                    }

                    _requests.Clear();
                }

                _cts.Dispose();
            }

            private sealed class Request
            {
                public Request(ulong offset, uint count, Task<byte[]> task)
                {
                    Offset = offset;
                    Count = count;
                    Task = task;
                }

                public ulong Offset { get; }
                public uint Count { get; }
                public Task<byte[]> Task { get; }
            }
        }
    }
}
