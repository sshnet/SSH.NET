using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Renci.SshNet.Sftp
{
    internal class SftpFileReader
    {
        private const int MaxReadAhead = 15;

        private readonly byte[] _handle;
        private readonly ISftpSession _sftpSession;
        private SemaphoreLight _semaphore;
        private bool _isCompleted;
        private uint _chunkLength;
        private int _readAheadChunkIndex;
        private int _nextChunkIndex;
        private ulong _readAheadOffset;
        private ulong _offset;
        private ulong _fileSize;
        private Exception _exception;
        private readonly IDictionary<int, BufferedRead> _queue;
        private readonly object _readLock;

        public SftpFileReader(byte[] handle, ISftpSession sftpSession)
        {
            _handle = handle;
            _sftpSession = sftpSession;
            _chunkLength = 32 * 1024; // TODO !
            _semaphore = new SemaphoreLight(MaxReadAhead);
            _queue = new Dictionary<int, BufferedRead>(MaxReadAhead);
            _readLock = new object();

            _fileSize = (ulong)_sftpSession.RequestFStat(_handle).Size;

            ThreadAbstraction.ExecuteThread(() =>
            {
                // read one chunk beyond the chunk in which we read "file size" bytes
                // to get an EOF

                while (_readAheadOffset <= (_fileSize + _chunkLength) && _exception == null)
                {
                    // TODO implement cancellation!?
                    // TODO implement IDisposable to cancel the Wait in case the client never completes reading to EOF
                    // TODO check if the BCL Semaphore unblocks wait on dispose (and mimick same behavior in our SemaphoreLight ?)
                    _semaphore.Wait();

                    // don't bother reading any more chunks if we reached EOF, or an exception has occurred
                    // while processing a chunk
                    if (_isCompleted || _exception != null)
                        break;

                    // TODO: catch exception, signal error to Read() and break loop

                    // start reading next chunk
                    _sftpSession.BeginRead(_handle, _readAheadOffset, _chunkLength, ReadCompleted,
                                           new BufferedRead(_readAheadChunkIndex, _readAheadOffset));

                    // advance read-ahead offset
                    _readAheadOffset += _chunkLength;

                    _readAheadChunkIndex++;
                }

                Console.WriteLine("Finished read-ahead");
            });
        }

        public byte[] Read()
        {
            if (_isCompleted)
                throw new SshException("Attempting to read beyond the end of the file.");

            lock (_readLock)
            {
                BufferedRead nextChunk;

                // TODO: break when we've reached file size and still haven't received an EOF ?

                // wait until either the next chunk is avalable or an exception has occurred
                while (!_queue.TryGetValue(_nextChunkIndex, out nextChunk) && _exception == null)
                    Monitor.Wait(_readLock);

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
                        _isCompleted = true;
                    }
                    // unblock wait in read-ahead
                    _semaphore.Release();
                    return data;
                }

                // when we received an EOF for the next chunk, then we only complete the current
                // chunk if we haven't already read up to the file size
                if (nextChunk.Data.Length == 0 && _offset == _fileSize)
                {
                    _isCompleted = true;

                    // unblock wait in read-ahead
                    _semaphore.Release();
                    // signal EOF to caller
                    return nextChunk.Data;
                }

                // when the server returned less bytes than requested (for the previous chunk)
                // we'll synchronously request the remaining data

                var catchUp = new byte[nextChunk.Offset - _offset];
                var bytesCaughtUp = 0L;

                while (bytesCaughtUp < catchUp.Length)
                {
                    // TODO: break loop and interrupt blocking wait in case of exception
                    var read = _sftpSession.RequestRead(_handle, _offset, (uint) catchUp.Length);
                    if (read.Length == 0)
                    {
                        // move reader to error state
                        _exception = new SshException("Unexpectedly reached end of file.");
                        // unblock wait in read-ahead
                        _semaphore.Release();
                        // notify caller of error
                        throw _exception;
                    }

                    bytesCaughtUp += read.Length;
                    _offset += (ulong) bytesCaughtUp;
                }

                return catchUp;
            }
        }

        private void ReadCompleted(IAsyncResult result)
        {
            var readAsyncResult = result as SftpReadAsyncResult;
            if (readAsyncResult == null)
                return;

            var data = readAsyncResult.EndInvoke();

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

        private class BufferedRead
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
