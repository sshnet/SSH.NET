using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Renci.SshNet.Sftp
{
    internal class SftpFileReader : ISftpFileReader
    {
        private const int ReadAheadWaitTimeoutInMilliseconds = 1000;

        private readonly byte[] _handle;
        private readonly ISftpSession _sftpSession;
        private readonly uint _chunkSize;
        private ulong _offset;

        /// <summary>
        /// Holds the size of the file, when available.
        /// </summary>
        private readonly long? _fileSize;
        private readonly Dictionary<int, BufferedRead> _queue;
        private readonly WaitHandle[] _waitHandles;

        private int _readAheadChunkIndex;
        private ulong _readAheadOffset;
        private readonly ManualResetEvent _readAheadCompleted;
        private int _nextChunkIndex;

        /// <summary>
        /// Holds a value indicating whether EOF has already been signaled by the SSH server.
        /// </summary>
        private bool _endOfFileReceived;
        /// <summary>
        /// Holds a value indicating whether the client has read up to the end of the file.
        /// </summary>
        private bool _isEndOfFileRead;
        private readonly SemaphoreLight _semaphore;
        private readonly object _readLock;

        private readonly ManualResetEvent _disposingWaitHandle;
        private bool _disposingOrDisposed;

        private Exception _exception;

        /// <summary>
        /// Initializes a new <see cref="SftpFileReader"/> instance with the specified handle,
        /// <see cref="ISftpSession"/> and the maximum number of pending reads.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="sftpSession"></param>
        /// <param name="chunkSize">The size of a individual read-ahead chunk.</param>
        /// <param name="maxPendingReads">The maximum number of pending reads.</param>
        /// <param name="fileSize">The size of the file, if known; otherwise, <c>null</c>.</param>
        public SftpFileReader(byte[] handle, ISftpSession sftpSession, uint chunkSize, int maxPendingReads, long? fileSize)
        {
            _handle = handle;
            _sftpSession = sftpSession;
            _chunkSize = chunkSize;
            _fileSize = fileSize;
            _semaphore = new SemaphoreLight(maxPendingReads);
            _queue = new Dictionary<int, BufferedRead>(maxPendingReads);
            _readLock = new object();
            _readAheadCompleted = new ManualResetEvent(false);
            _disposingWaitHandle = new ManualResetEvent(false);
            _waitHandles = _sftpSession.CreateWaitHandleArray(_disposingWaitHandle, _semaphore.AvailableWaitHandle);

            StartReadAhead();
        }

        public byte[] Read()
        {
            if (_disposingOrDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (_exception != null)
                throw _exception;
            if (_isEndOfFileRead)
                throw new SshException("Attempting to read beyond the end of the file.");

            BufferedRead nextChunk;

            lock (_readLock)
            {
                // wait until either the next chunk is avalable, an exception has occurred or the current
                // instance is already disposed
                while (!_queue.TryGetValue(_nextChunkIndex, out nextChunk) && _exception == null)
                {
                    Monitor.Wait(_readLock);
                }

                // throw when exception occured in read-ahead, or the current instance is already disposed
                if (_exception != null)
                    throw _exception;

                var data = nextChunk.Data;

                if (nextChunk.Offset == _offset)
                {
                    // have we reached EOF?
                    if (data.Length == 0)
                    {
                        // PERF: we do not bother updating all of the internal state when we've reached EOF

                        _isEndOfFileRead = true;
                    }
                    else
                    {
                        // remove processed chunk
                        _queue.Remove(_nextChunkIndex);
                        // update offset
                        _offset += (ulong) data.Length;
                        // move to next chunk
                        _nextChunkIndex++;
                    }
                    // unblock wait in read-ahead
                    _semaphore.Release();

                    return data;
                }

                // when we received an EOF for the next chunk and the size of the file is known, then
                // we only complete the current chunk if we haven't already read up to the file size;
                // this way we save an extra round-trip to the server
                if (data.Length == 0 && _fileSize.HasValue && _offset == (ulong) _fileSize.Value)
                {
                    // avoid future reads
                    _isEndOfFileRead = true;
                    // unblock wait in read-ahead
                    _semaphore.Release();
                    // signal EOF to caller
                    return nextChunk.Data;
                }
            }

            // When the server returned less bytes than requested (for the previous chunk)
            // we'll synchronously request the remaining data.
            //
            // Due to the optimization above, we'll only get here in one of the following cases:
            // - an EOF situation for files for which we were unable to obtain the file size
            // - fewer bytes that requested were returned
            // 
            // According to the SSH specification, this last case should never happen for normal
            // disk files (but can happen for device files). In practice, OpenSSH - for example -
            // returns less bytes than requested when requesting more than 64 KB.
            //
            // Important:
            // To avoid a deadlock, this read must be done outside of the read lock

            var bytesToCatchUp = nextChunk.Offset - _offset;

            // TODO: break loop and interrupt blocking wait in case of exception

            var read = _sftpSession.RequestRead(_handle, _offset, (uint) bytesToCatchUp);
            if (read.Length == 0)
            {
                // process data in read lock to avoid ObjectDisposedException while releasing semaphore
                lock (_readLock)
                {
                    // a zero-length (EOF) response is only valid for the read-back when EOF has
                    // been signaled for the next read-ahead chunk
                    if (nextChunk.Data.Length == 0)
                    {
                        _isEndOfFileRead = true;
                        // ensure we've not yet disposed the current instance
                        if (!_disposingOrDisposed)
                        {
                            // unblock wait in read-ahead
                            _semaphore.Release();
                        }
                        // signal EOF to caller
                        return read;
                    }

                    // move reader to error state
                    _exception = new SshException("Unexpectedly reached end of file.");
                    // ensure we've not yet disposed the current instance
                    if (!_disposingOrDisposed)
                    {
                        // unblock wait in read-ahead
                        _semaphore.Release();
                    }
                    // notify caller of error
                    throw _exception;
                }
            }

            _offset += (uint) read.Length;

            return read;
        }

        ~SftpFileReader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (_disposingOrDisposed)
                return;

            // transition to disposing state
            _disposingOrDisposed = true;

            if (disposing)
            {
                // record exception to break prevent future Read()
                _exception = new ObjectDisposedException(GetType().FullName);

                // signal that we're disposing to interrupt wait in read-ahead
                _disposingWaitHandle.Set();

                // wait until the read-ahead thread has completed
                _readAheadCompleted.WaitOne();

                // unblock the Read()
                lock (_readLock)
                {
                    // dispose semaphore in read lock to ensure we don't run into an ObjectDisposedException
                    // in Read()
                    _semaphore.Dispose();
                    // awake Read
                    Monitor.PulseAll(_readLock);
                }

                _readAheadCompleted.Dispose();
                _disposingWaitHandle.Dispose();

                if (_sftpSession.IsOpen)
                {
                    try
                    {
                        var closeAsyncResult = _sftpSession.BeginClose(_handle, null, null);
                        _sftpSession.EndClose(closeAsyncResult);
                    }
                    catch (Exception ex)
                    {
                        DiagnosticAbstraction.Log("Failure closing handle: " + ex);
                    }
                }
            }
        }

        private void StartReadAhead()
        {
            ThreadAbstraction.ExecuteThread(() =>
            {
                while (!_endOfFileReceived && _exception == null)
                {
                    // check if we should continue with the read-ahead loop
                    // note that the EOF and exception check are not included
                    // in this check as they do not require Read() to be
                    // unblocked (or have already done this)
                    if (!ContinueReadAhead())
                    {
                        // unblock the Read()
                        lock (_readLock)
                        {
                            Monitor.PulseAll(_readLock);
                        }
                        // break the read-ahead loop
                        break;
                    }

                    // attempt to obtain the semaphore; this may time out when all semaphores are
                    // in use due to pending read-aheads (which in turn can happen when the server
                    // is slow to respond or when the session is broken)
                    if (!_semaphore.Wait(ReadAheadWaitTimeoutInMilliseconds))
                    {
                        // re-evaluate whether an exception occurred, and - if not - wait again
                        continue;
                    }

                    // don't bother reading any more chunks if we received EOF, an exception has occurred
                    // or the current instance is disposed
                    if (_endOfFileReceived || _exception != null)
                        break;

                    // start reading next chunk
                    var bufferedRead = new BufferedRead(_readAheadChunkIndex, _readAheadOffset);

                    try
                    {
                        // even if we know the size of the file and have read up to EOF, we still want
                        // to keep reading (ahead) until we receive zero bytes from the remote host as
                        // we do not want to rely purely on the reported file size
                        //
                        // if the offset of the read-ahead chunk is greater than that file size, then
                        // we can expect to be reading the last (zero-byte) chunk and switch to synchronous
                        // mode to avoid having multiple read-aheads that read beyond EOF
                        if (_fileSize != null && (long) _readAheadOffset > _fileSize.Value)
                        {
                            var asyncResult = _sftpSession.BeginRead(_handle, _readAheadOffset, _chunkSize, null,
                                                                     bufferedRead);
                            var data = _sftpSession.EndRead(asyncResult);
                            ReadCompletedCore(bufferedRead, data);
                        }
                        else
                        {
                            _sftpSession.BeginRead(_handle, _readAheadOffset, _chunkSize, ReadCompleted, bufferedRead);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleFailure(ex);
                        break;
                    }

                    // advance read-ahead offset
                    _readAheadOffset += _chunkSize;
                    // increment index of read-ahead chunk
                    _readAheadChunkIndex++;
                }

                _readAheadCompleted.Set();
            });
        }

        /// <summary>
        /// Returns a value indicating whether the read-ahead loop should be continued.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the read-ahead loop should be continued; otherwise, <c>false</c>.
        /// </returns>
        private bool ContinueReadAhead()
        {
            try
            {
                var waitResult = _sftpSession.WaitAny(_waitHandles, _sftpSession.OperationTimeout);
                switch (waitResult)
                {
                    case 0: // disposing
                        return false;
                    case 1: // semaphore available
                        return true;
                    default:
                        throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", waitResult));
                }
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref _exception, ex, null);
                return false;
            }
        }

        private void ReadCompleted(IAsyncResult result)
        {
            if (_disposingOrDisposed)
            {
                // skip further processing if we're disposing the current instance
                // to avoid accessing disposed members
                return;
            }

            var readAsyncResult = (SftpReadAsyncResult) result;

            byte[] data;

            try
            {
                data = readAsyncResult.EndInvoke();
            }
            catch (Exception ex)
            {
                HandleFailure(ex);
                return;
            }

            // a read that completes with a zero-byte result signals EOF
            // but there may be pending reads before that read
            var bufferedRead = (BufferedRead) readAsyncResult.AsyncState;
            ReadCompletedCore(bufferedRead, data);
        }

        private void ReadCompletedCore(BufferedRead bufferedRead, byte[] data)
        {
            bufferedRead.Complete(data);

            lock (_readLock)
            {
                // add item to queue
                _queue.Add(bufferedRead.ChunkIndex, bufferedRead);
                // signal that a chunk has been read or EOF has been reached;
                // in both cases, Read() will eventually also unblock the "read-ahead" thread
                Monitor.PulseAll(_readLock);
            }

            // check if server signaled EOF
            if (data.Length == 0)
            {
                // set a flag to stop read-aheads
                _endOfFileReceived = true;
            }
        }

        private void HandleFailure(Exception cause)
        {
            Interlocked.CompareExchange(ref _exception, cause, null);

            // unblock read-ahead
            _semaphore.Release();

            // unblock Read()
            lock (_readLock)
            {
                Monitor.PulseAll(_readLock);
            }
        }

        internal class BufferedRead
        {
            public int ChunkIndex { get; private set; }

            public byte[] Data { get; private set; }

            public ulong Offset { get; private set; }

            public BufferedRead(int chunkIndex, ulong offset)
            {
                ChunkIndex = chunkIndex;
                Offset = offset;
            }

            public void Complete(byte[] data)
            {
                Data = data;
            }
        }
    }
}
