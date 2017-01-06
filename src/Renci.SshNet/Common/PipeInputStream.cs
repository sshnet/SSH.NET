namespace Renci.SshNet.Common 
{
    using System;
    using System.IO;

    internal class PipeInputStream : Stream
    {
        private LinkedListQueue<byte[]> _queue;
        private byte[] _current;
        private int _currentPosition;
        private bool _isDisposed;

        public PipeInputStream(LinkedListQueue<byte[]> queue)
        {
            _queue = queue;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            if (_isDisposed)
                throw CreateObjectDisposedException();

            if (_current == null || _currentPosition == _current.Length)
            {
                if (_queue.IsCompleted || !_queue.TryTake(out _current))
                    return 0;

                _currentPosition = 0;
            }

            var avail = _current.Length - _currentPosition;
            if (count > avail)
                count = avail;

            Buffer.BlockCopy(_current, _currentPosition, buffer, offset, count);

            _currentPosition += count;
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _isDisposed = true;
        }

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }
}
