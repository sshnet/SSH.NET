using System;
using System.Buffers.Binary;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal sealed class SftpWriteRequest : SftpRequest
    {
        private readonly SftpWriteRequestBuffer _buffer;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Write; }
        }

        public ReadOnlySpan<byte> Handle
        {
            get
            {
                return _buffer.Handle;
            }
        }

        /// <summary>
        /// Gets the zero-based offset (in bytes) relative to the beginning of the file that the write
        /// must start at.
        /// </summary>
        /// <value>
        /// The zero-based offset (in bytes) relative to the beginning of the file that the write must
        /// start at.
        /// </value>
        public ulong ServerFileOffset
        {
            get
            {
                return _buffer.ServerFileOffset;
            }
        }

        /// <summary>
        /// Gets the buffer holding the data to write.
        /// </summary>
        /// <value>
        /// The buffer holding the data to write.
        /// </value>
        public ReadOnlySpan<byte> Data
        {
            get
            {
                return _buffer.Data.AsSpan(0, _buffer.DataLength);
            }
        }

        protected override int BufferCapacity
        {
            get
            {
                return _buffer.ActiveBytes.Count;
            }
        }

        public SftpWriteRequest(uint protocolVersion,
                                SftpWriteRequestBuffer buffer,
                                Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, buffer.RequestId, statusAction)
        {
            _buffer = buffer;
        }

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }

        protected override void WriteBytes(SshDataStream stream)
        {
            var activeBuffer = GetBytes();

            stream.Write(activeBuffer.Array, activeBuffer.Offset, activeBuffer.Count);
        }

        public new ArraySegment<byte> GetBytes()
        {
            var activeBuffer = _buffer.ActiveBytes;

            // Write SFTP packet length.
            BinaryPrimitives.WriteInt32BigEndian(activeBuffer.AsSpan(), activeBuffer.Count - 4);

            return activeBuffer;
        }
    }
}
