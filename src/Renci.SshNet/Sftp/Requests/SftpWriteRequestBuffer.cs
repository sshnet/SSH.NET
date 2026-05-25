#nullable enable
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

namespace Renci.SshNet.Sftp.Requests
{
    /// <summary>
    /// A helper type that wraps a buffer for SFTP write requests.
    /// </summary>
    /// <remarks>
    /// [Sftp packet length, SftpMessageType, RequestId, Handle length, Handle, Server offset, data length, data].
    /// [                 4,               1,         4,             4,      ?,             8,           4,    ?].
    /// </remarks>
    internal sealed class SftpWriteRequestBuffer : IDisposable
    {
        private const int MessageTypeOffset = 4;
        private const int RequestIdOffset = MessageTypeOffset + 1;
        private const int HandleLengthOffset = RequestIdOffset + 4;
        private const int HandleOffset = HandleLengthOffset + 4;

        private readonly bool _usePool;
        private byte[] _buffer;

        public ArraySegment<byte> ActiveBytes
        {
            get
            {
                return new(_buffer, 0, HandleOffset + HandleLength + 8 + 4 + DataLength);
            }
        }

        public SftpWriteRequestBuffer(ReadOnlySpan<byte> handle, int dataCapacity, bool usePool = false)
        {
            Debug.Assert(dataCapacity >= 0);

            var totalCapacity = HandleOffset + handle.Length + 8 + 4 + dataCapacity;

            _usePool = usePool;

            _buffer = usePool
                ? ArrayPool<byte>.Shared.Rent(totalCapacity)
                : new byte[totalCapacity];

            _buffer[MessageTypeOffset] = (byte)SftpMessageTypes.Write;

            HandleLength = handle.Length;

            handle.CopyTo(_buffer.AsSpan(HandleOffset));
        }

        public SftpWriteRequestBuffer(ReadOnlySpan<byte> handle, ulong serverFileOffset, ReadOnlySpan<byte> data, bool usePool = false)
            : this(handle, data.Length, usePool)
        {
            ServerFileOffset = serverFileOffset;

            DataLength = data.Length;

            data.CopyTo(Data);
        }

        public uint RequestId
        {
            get
            {
                return BinaryPrimitives.ReadUInt32BigEndian(_buffer.AsSpan(RequestIdOffset));
            }
            set
            {
                BinaryPrimitives.WriteUInt32BigEndian(_buffer.AsSpan(RequestIdOffset), value);
            }
        }

        public int HandleLength
        {
            get
            {
                return BinaryPrimitives.ReadInt32BigEndian(_buffer.AsSpan(HandleLengthOffset));
            }
            private init
            {
                Debug.Assert(value >= 0);
                BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(HandleLengthOffset), value);
            }
        }

        public ReadOnlySpan<byte> Handle
        {
            get
            {
                return _buffer.AsSpan(HandleOffset, HandleLength);
            }
        }

        public ulong ServerFileOffset
        {
            get
            {
                return BinaryPrimitives.ReadUInt64BigEndian(_buffer.AsSpan(HandleOffset + HandleLength));
            }
            set
            {
                BinaryPrimitives.WriteUInt64BigEndian(_buffer.AsSpan(HandleOffset + HandleLength), value);
            }
        }

        public int DataLength
        {
            get
            {
                return BinaryPrimitives.ReadInt32BigEndian(_buffer.AsSpan(HandleOffset + HandleLength + 8));
            }
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _buffer.Length - (HandleOffset + HandleLength + 8 + 4));

                BinaryPrimitives.WriteInt32BigEndian(_buffer.AsSpan(HandleOffset + HandleLength + 8), value);
            }
        }

        /// <summary>
        /// Gets the space available to write as file data. Does not consider <see cref="DataLength"/>.
        /// </summary>
        public ArraySegment<byte> Data
        {
            get
            {
                var offset = HandleOffset + HandleLength + 8 + 4;
                return new ArraySegment<byte>(_buffer, offset, _buffer.Length - offset);
            }
        }

        public void Dispose()
        {
            if (_usePool)
            {
                var buffer = _buffer;
                _buffer = null!;

                if (buffer is not null)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}
