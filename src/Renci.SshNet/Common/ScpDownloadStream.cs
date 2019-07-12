using Renci.SshNet.Channels;
using System;
using System.IO;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Forward, read-only stream used to download a file via SCP. 
    /// </summary>
    public class ScpDownloadStream : Stream
    {
        private readonly ScpClient _client;
        private readonly IChannelSession _channelSession;
        private readonly Stream _scpStream;
        private readonly string _fileName;
        private readonly long _length;
        private long _needToRead;

        internal ScpDownloadStream(ScpClient client, IChannelSession channelSession, Stream scpStream, string fileName, long length)
        {
            _client = client;
            _channelSession = channelSession;
            _scpStream = scpStream;
            _fileName = fileName;
            _length = length;
            _needToRead = length;
        }

        /// <inheritdoc />
        public override bool CanRead
        {
            get { return true; }
        }
        /// <inheritdoc />
        public override bool CanSeek
        {
            get { return false; }
        }
        /// <inheritdoc />
        public override bool CanWrite
        {
            get { return false; }
        }
        /// <inheritdoc />
        public override long Length
        {
            get { return _length; }
        }

        /// <inheritdoc />
        public override long Position
        {
            get { return _scpStream.Position; }
            set { }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _scpStream.Dispose();
            _channelSession.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            throw new NotSupportedException("Flush is not supported. Forward, read-only stream");
        }
        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek is not supported. Forward, read-only stream");
        }
        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLength is not supported. Forward, read-only stream");
        }
        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _scpStream.Read(buffer, offset, (int)Math.Min(_needToRead, count));
            _needToRead -= read;

            _client.RaiseDownloadingEvent(_fileName, _length, _length - _needToRead);

            if (_needToRead == 0)
            {
                //  Send confirmation byte after last data byte was read
                ScpClient.SendSuccessConfirmation(_channelSession);

                _client.CheckReturnCode(_scpStream);
            }

            return read;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Write is not supported. Forward, read-only stream");
        }
    }
}
