using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Renci.SshNet.Sftp
{
    internal class SftpFileReader : IDisposable
    {
        private readonly byte[] _handle;
        private readonly ISftpSession _sftpSession;
        private uint _chunkLength;
        private ulong _offset;
        private ulong _fileSize;
        private readonly IDictionary<int, BufferedRead> _queue;

        private int _readAheadChunkIndex;
        private ulong _readAheadOffset;
        private ManualResetEvent _readAheadCompleted;
        private int _nextChunkIndex;

        private bool _isEndOfFile;
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
        /// <param name="maxReadHead">The maximum number of pending reads.</param>
        public SftpFileReader(byte[] handle, ISftpSession sftpSession, int maxReadHead)
        {
            _handle = handle;
            _sftpSession = sftpSession;
            _chunkLength = 32 * 1024 - 13; // TODO !
            _semaphore = new SemaphoreLight(maxReadHead);
            _queue = new Dictionary<int, BufferedRead>(maxReadHead);
            _readLock = new object();
            _readAheadCompleted = new ManualResetEvent(false);

            _fileSize = (ulong)_sftpSession.RequestFStat(_handle).Size;

            StartReadAhead();
        }

        public byte[] Read()
        {
            if (_exception != null || _disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (_isEndOfFile)
                throw new SshException("Attempting to read beyond the end of the file.");

            lock (_readLock)
            {
                BufferedRead nextChunk;

                // TODO: break when we've reached file size and still haven't received an EOF ?

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
                    _offset += (ulong) data.Length;

                    // remove processed chunk
                    _queue.Remove(_nextChunkIndex);
                    // move to next chunk
                    _nextChunkIndex++;
                    // have we reached EOF?
                    if (data.Length == 0)
                    {
                        _isEndOfFile = true;
                    }
                    // unblock wait in read-ahead
                    _semaphore.Release();
                    return data;
                }

                // when we received an EOF for the next chunk, then we only complete the current
                // chunk if we haven't already read up to the file size
                if (nextChunk.Data.Length == 0 && _offset == _fileSize)
                {
                    _isEndOfFile = true;

                    // unblock wait in read-ahead
                    _semaphore.Release();
                    // signal EOF to caller
                    return nextChunk.Data;
                }

                // when the server returned less bytes than requested (for the previous chunk)
                // we'll synchronously request the remaining data

                var bytesToCatchUp = nextChunk.Offset - _offset;

                // TODO: break loop and interrupt blocking wait in case of exception
                var read = _sftpSession.RequestRead(_handle, _offset, (uint) bytesToCatchUp);
                if (read.Length == 0)
                {
                    // move reader to error state
                    _exception = new SshException("Unexpectedly reached end of file.");
                    // unblock wait in read-ahead
                    _semaphore.Release();
                    // notify caller of error
                    throw _exception;
                }

                _offset += (uint) read.Length;

                return read;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                var readAheadCompleted = _readAheadCompleted;
                if (readAheadCompleted != null)
                {
                    _readAheadCompleted = null;
                    if (!readAheadCompleted.WaitOne(TimeSpan.FromSeconds(1)))
                    {
                        DiagnosticAbstraction.Log("Read-ahead thread did not complete within time-out.");
                    }
                    readAheadCompleted.Dispose();
                }

                _disposed = true;
            }
        }

        private void StartReadAhead()
        {
            ThreadAbstraction.ExecuteThread(() =>
            {
                while (_exception == null)
                {
                    // TODO implement cancellation!?
                    // TODO implement IDisposable to cancel the Wait in case the client never completes reading to EOF
                    // TODO check if the BCL Semaphore unblocks wait on dispose (and mimick same behavior in our SemaphoreLight ?)
                    _semaphore.Wait();

                    // don't bother reading any more chunks if we reached EOF, or an exception has occurred
                    // while processing a chunk
                    if (_isEndOfFile || _exception != null)
                        break;

                    // start reading next chunk
                    try
                    {
                        _sftpSession.BeginRead(_handle, _readAheadOffset, _chunkLength, ReadCompleted,
                                               new BufferedRead(_readAheadChunkIndex, _readAheadOffset));
                    }
                    catch (Exception ex)
                    {
                        HandleFailure(ex);
                        break;
                    }

                    if (_readAheadOffset >= _fileSize)
                    {
                        // read one chunk beyond the chunk in which we read "file size" bytes
                        // to get an EOF
                        break;
                    }

                    // advance read-ahead offset
                    _readAheadOffset += _chunkLength;

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

            byte[] data = null;

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

            // signal that a chunk has been read or EOF has been reached;
            // in both cases, we want to unblock the "read-ahead" thread
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
