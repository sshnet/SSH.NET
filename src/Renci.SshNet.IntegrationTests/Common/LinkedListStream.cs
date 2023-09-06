namespace Renci.SshNet.IntegrationTests.Common
{
    internal class LinkedListStream : Stream
    {
        private PipeEntry _first;
        private PipeEntry _last;

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
            var totalBytesRead = 0;

            while (count > 0 && _first != null)
            {
                var bytesRead = _first.Read(buffer, offset, count);
                if (_first.IsEmpty)
                {
                    _first = _first.Next;
                }

                count -= bytesRead;
                totalBytesRead += bytesRead;
                offset += bytesRead;
            }

            return totalBytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var last = new PipeEntry(buffer, offset, count);
            if (_last == null)
            {
                _last = last;
            }
            else
            {
                _last = _last.Next = last;
            }

            _first ??= _last;
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
            get { return true; }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position { get; set; }
    }

    internal class PipeEntry
    {
        private readonly byte[] _data;
        public int Position;
        public int Length;

        public PipeEntry(byte[] data, int offset, int count)
        {
            _data = data;
            Position = offset;
            Length = count;
        }

        public int Read(byte[] dst, int offset, int count)
        {
            var bytesToCopy = count;
            var bytesAvailable = Length - Position;

            if (count > bytesAvailable)
            {
                bytesToCopy = bytesAvailable;
            }

            Buffer.BlockCopy(_data, Position, dst, offset, bytesToCopy);
            Position += bytesToCopy;
            return bytesToCopy;
        }

        public bool IsEmpty
        {
            get { return Position == Length; }
        }

        public PipeEntry Next { get; set; }
    }
}
