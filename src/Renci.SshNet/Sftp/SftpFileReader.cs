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
                while (!_isCompleted)
                {
                    // we reach one chunk beyond the file size to get an EOF
                    if (_readAheadOffset > _fileSize)
                        break;

                    // TODO implement cancellation!?
                    _semaphore.Wait();

                    // start reading next chunk
                    _sftpSession.BeginRead(_handle, _readAheadOffset, _chunkLength, ReadCompleted,
                                           new BufferedRead(_readAheadChunkIndex, _readAheadOffset));

                    // advance read-ahead offset
                    _readAheadOffset += _chunkLength;

                    _readAheadChunkIndex++;
                }
            });
        }

        public byte[] Read()
        {
            lock (_readLock)
            {
                BufferedRead nextChunk;

                while (!_queue.TryGetValue(_nextChunkIndex, out nextChunk) && !_isCompleted)
                    Monitor.Wait(_readLock);

                if (_isCompleted)
                    return new byte[0];

                if (nextChunk.Offset == _offset)
                {
                    var data = nextChunk.Data;
                    _offset += (ulong) data.Length;

                    // remove processed chunk
                    _queue.Remove(_nextChunkIndex);
                    // move to next chunk
                    _nextChunkIndex++;
                    // allow read-ahead of a new chunk
                    _semaphore.Release();
                    return data;
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
                        // break in loop in "read-ahead" thread (once a blocking wait is interrupted)
                        _isCompleted = true;
                        // interrupt blocking wait in "read-ahead" thread
                        lock (_readLock)
                            Monitor.PulseAll(_readLock);
                        // signal failure
                        throw new SshException("Unexpectedly reached end of file.");
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
            if (data.Length == 0)
            {
                _isCompleted = true;
            }
            else
            {
                var bufferedRead = (BufferedRead)readAsyncResult.AsyncState;
                bufferedRead.Complete(data);
                _queue.Add(bufferedRead.ChunkIndex, bufferedRead);
            }

            // signal that a chunk has been read or EOF has been reached;
            // in both cases, we want to unblock the "read-ahead" thread
            lock (_readLock)
            {
                Monitor.Pulse(_readLock);
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
