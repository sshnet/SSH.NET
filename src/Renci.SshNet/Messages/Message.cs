#nullable enable
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;

namespace Renci.SshNet.Messages
{
    /// <summary>
    /// Base class for all SSH protocol messages.
    /// </summary>
    public abstract class Message : SshData
    {
        /// <summary>
        /// Gets the message name as defined in RFC 4250.
        /// </summary>
        public abstract string MessageName { get; }

        /// <summary>
        /// Gets the message number as defined in RFC 4250.
        /// </summary>
        public abstract byte MessageNumber { get; }

        /// <inheritdoc />
        protected override int BufferCapacity
        {
            get
            {
                return 1; // Message type
            }
        }

        /// <inheritdoc />
        protected override void WriteBytes(SshDataStream stream)
        {
            stream.WriteByte(MessageNumber);
            base.WriteBytes(stream);
        }

        /// <returns>The number of bytes occupied by the packet in <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// [4 bytes] || packet_len || padding_len || payload || padding || [macLength bytes].
        /// </remarks>
        internal int GetPacket(ref byte[] buffer, byte paddingMultiplier, Compressor? compressor, bool excludePacketLengthFieldWhenPadding, int macLength)
        {
            const int outboundPacketSequenceSize = 4;

            var messageLength = BufferCapacity;

            ArraySegment<byte> payload = default;

            if (messageLength == -1 || compressor != null)
            {
                using (var sshDataStream = new SshDataStream(messageLength != -1 ? messageLength : DefaultCapacity))
                {
                    WriteBytes(sshDataStream);

                    var success = sshDataStream.TryGetBuffer(out payload);

                    Debug.Assert(success);
                }

                if (compressor != null)
                {
                    payload = new(compressor.Compress(payload.Array, payload.Offset, payload.Count));
                }

                messageLength = payload.Count;
            }

            // determine the padding length
            // in Encrypt-then-MAC mode or AEAD, the length field is not encrypted, so we should keep it out of the
            // padding length calculation
            var paddingLength = GetPaddingLength(
                paddingMultiplier, (excludePacketLengthFieldWhenPadding ? 0 : 4) + 1 + messageLength);

            var packetLength = 1 + messageLength + paddingLength;

            var bytesRequired = 4 + 4 + packetLength + macLength;

            if ((uint)bytesRequired > (uint)Session.MaximumSshPacketSize)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Packet is too big. Maximum packet size is {0} bytes.", Session.MaximumSshPacketSize));
            }

            if (buffer.Length < bytesRequired)
            {
                Array.Resize(ref buffer, Math.Max(bytesRequired, 2 * buffer.Length));
            }

            using (var sshDataStream = new SshDataStream(buffer))
            {
                // skip bytes for outbound packet sequenceSize
                _ = sshDataStream.Seek(outboundPacketSequenceSize, SeekOrigin.Begin);

                // add packet length
                sshDataStream.Write((uint)packetLength);

                // add padding length
                sshDataStream.WriteByte(paddingLength);

                // add message payload
                if (payload != default)
                {
                    sshDataStream.Write(payload.Array!, payload.Offset, payload.Count);
                }
                else
                {
                    WriteBytes(sshDataStream);
                }

                Debug.Assert(sshDataStream.Position == bytesRequired - macLength - paddingLength);

                // add padding bytes
                CryptoAbstraction.Randomizer.GetBytes(buffer, (int)sshDataStream.Position, paddingLength);
            }

            return bytesRequired;
        }

        private static byte GetPaddingLength(byte paddingMultiplier, long packetLength)
        {
            var paddingLength = (byte)((-packetLength) & (paddingMultiplier - 1));

            if (paddingLength < paddingMultiplier)
            {
                paddingLength += paddingMultiplier;
            }

            return paddingLength;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return MessageName;
        }

        /// <summary>
        /// Process the current message for the specified <see cref="Session"/>.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> for which to process the current message.</param>
        internal abstract void Process(Session session);
    }
}
