namespace Renci.SshNet.Common 
{
    using System;
    using System.IO;

    internal class PipeOutputStream : Stream
    {
        private LinkedListQueue<byte[]> _queue;
        private bool _isDisposed;

        public PipeOutputStream(LinkedListQueue<byte[]> queue)
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
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("offset", "offset or count is negative.");
            if (_isDisposed || _queue.IsAddingCompleted)
                throw CreateObjectDisposedException();

            byte[] tmp = new byte[count];
            Buffer.BlockCopy(buffer, offset, tmp, 0, count);
            _queue.Add(tmp);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !_isDisposed; }
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

        public override void Close()
        {
            if (!_queue.IsAddingCompleted)
                _queue.CompleteAdding();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_isDisposed)
            {
                if (!_queue.IsAddingCompleted)
                    _queue.CompleteAdding();
                _isDisposed = true;
            }
        }

        private ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(GetType().FullName);
        }
    }
}
