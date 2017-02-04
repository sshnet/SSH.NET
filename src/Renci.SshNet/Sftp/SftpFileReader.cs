using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
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
        private long? _fileSize;
        private readonly IDictionary<int, BufferedRead> _queue;

        private int _readAheadChunkIndex;
        private ulong _readAheadOffset;
        private ManualResetEvent _readAheadCompleted;
        private int _nextChunkIndex;

        /// <summary>
        /// Holds a value indicating whether EOF has already been signaled by the SSH server.
        /// </summary>
        private bool _endOfFileReceived;
        /// <summary>
        /// Holds a value indicating whether the client has read up to the end of the file.
        /// </summary>
        private bool _isEndOfFileRead;
        private SemaphoreLight _semaphore;
        private readonly object _readLock;

        private Exception _exception;
        private bool _disposed;

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

            StartReadAhead();
        }

        public byte[] Read()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (_exception != null)
                throw _exception;
            if (_isEndOfFileRead)
                throw new SshException("Attempting to read beyond the end of the file.");

            BufferedRead nextChunk;

            lock (_readLock)
            {
                // wait until either the next chunk is avalable or an exception has occurred
                while (!_queue.TryGetValue(_nextChunkIndex, out nextChunk) && _exception == null)
                {
                    Monitor.Wait(_readLock);
                }

                if (_exception != null)
                    throw _exception;

                if (nextChunk.Offset == _offset)
                {
                    var data = nextChunk.Data;

                    // have we reached EOF?
                    if (data.Length == 0)
                    {
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
                if (nextChunk.Data.Length == 0 && _fileSize.HasValue && _offset == (ulong)_fileSize.Value)
                {
                    _isEndOfFileRead = true;

                    // unblock wait in read-ahead
                    _semaphore.Release();
                    // signal EOF to caller
                    return nextChunk.Data;
                }
            }


            // when the server returned less bytes than requested (for the previous chunk)
            // we'll synchronously request the remaining data
            //
            // due to the optimization above, we'll only get here in one of the following cases:
            // - an EOF situation for files for which we were unable to obtain the file size
            // - fewer bytes that requested were returned
            // 
            // according to the SSH specification, this last case should never happen for normal
            // disk files (but can happen for device files).
            //
            // Important:
            // to avoid a deadlock, this read must be done outside of the read lock

            var bytesToCatchUp = nextChunk.Offset - _offset;

            // TODO: break loop and interrupt blocking wait in case of exception

            var read = _sftpSession.RequestRead(_handle, _offset, (uint) bytesToCatchUp);
            if (read.Length == 0)
            {
                lock (_readLock)
                {
                    // a zero-length (EOF) response is only valid for the read-back when EOF has
                    // been signaled for the next read-ahead chunk
                    if (nextChunk.Data.Length == 0)
                    {
                        _isEndOfFileRead = true;

                        // unblock wait in read-ahead
                        _semaphore.Release();
                        // signal EOF to caller
                        return read;
                    }

                    // move reader to error state
                    _exception = new SshException("Unexpectedly reached end of file.");
                    // ensure we've not yet disposed the current instance
                    if (_semaphore != null)
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

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Note that we will break the read-ahead loop, but will not interrupt the blocking wait
        /// in that loop.
        /// </remarks>
        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // use this to break the read-ahead loop
                _exception = new ObjectDisposedException(GetType().FullName);

                var readAheadCompleted = _readAheadCompleted;
                if (readAheadCompleted != null)
                {
                    readAheadCompleted.WaitOne();
                    readAheadCompleted.Dispose();
                    _readAheadCompleted = null;
                }

                // dispose semaphore in read lock to ensure we don't run into an ObjectDisposedException
                // in Read()
                lock (_readLock)
                {
                    if (_semaphore != null)
                    {
                        _semaphore.Dispose();
                        _semaphore = null;
                    }
                }

                _sftpSession.RequestClose(_handle);
            }

            _disposed = true;
        }

        private void StartReadAhead()
        {
            ThreadAbstraction.ExecuteThread(() =>
            {
                while (!_endOfFileReceived && _exception == null)
                {
                    // wait on either a release on the semaphore, or signaling of a broken session/connection
                    try
                    {
                        _sftpSession.WaitOnHandle(_semaphore.AvailableWaitHandle, _sftpSession.OperationTimeout);
                    }
                    catch (Exception ex)
                    {
                        _exception = ex;

                        // unblock the Read()
                        lock (_readLock)
                        {
                            Monitor.Pulse(_readLock);
                        }

                        // break the read-ahead loop
                        break;
                    }

                    // attempt to obtain the semaphore; this may time out when all semaphores are
                    // in use due to pending read-aheads
                    //
                    // in general this only happens when the session is broken
                    if (!_semaphore.Wait(ReadAheadWaitTimeoutInMilliseconds))
                    {
                        // unblock the Read()
                        lock (_readLock)
                        {
                            Monitor.Pulse(_readLock);
                        }
                        // re-evaluate whether an exception occurred, and - if not - wait again
                        continue;
                    }

                    // don't bother reading any more chunks if we received EOF, or an exception has occurred
                    // while processing a chunk
                    if (_endOfFileReceived || _exception != null)
                        break;

                    // start reading next chunk
                    try
                    {
                        _sftpSession.BeginRead(_handle, _readAheadOffset, _chunkSize, ReadCompleted,
                                               new BufferedRead(_readAheadChunkIndex, _readAheadOffset));
                    }
                    catch (Exception ex)
                    {
                        HandleFailure(ex);
                        break;
                    }

                    // advance read-ahead offset
                    _readAheadOffset += _chunkSize;

                    _readAheadChunkIndex++;
                }

                _readAheadCompleted.Set();
            });
        }

        private void ReadCompleted(IAsyncResult result)
        {
            var readAsyncResult = result as SftpReadAsyncResult;
            if (readAsyncResult == null)
                return;

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
            var bufferedRead = (BufferedRead)readAsyncResult.AsyncState;
            bufferedRead.Complete(data);
            _queue.Add(bufferedRead.ChunkIndex, bufferedRead);

            // check if server signaled EOF
            if (data.Length == 0)
            {
                // set a flag to stop read-aheads
                _endOfFileReceived = true;
            }

            // signal that a chunk has been read or EOF has been reached;
            // in both cases, Read() will eventually also unblock the "read-ahead" thread
            lock (_readLock)
            {
                Monitor.PulseAll(_readLock);
            }
        }

        private void HandleFailure(Exception cause)
        {
            _exception = cause;

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
