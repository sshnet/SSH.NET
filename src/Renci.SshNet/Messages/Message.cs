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

        internal byte[] GetPacket(byte paddingMultiplier, Compressor compressor, bool etm = false)
        {
            const int outboundPacketSequenceSize = 4;

            var messageLength = BufferCapacity;

            if (messageLength == -1 || compressor != null)
            {
                using (var sshDataStream = new SshDataStream(DefaultCapacity))
                {
                    // skip:
                    // * 4 bytes for the outbound packet sequence
                    // * 4 bytes for the packet data length
                    // * one byte for the packet padding length
                    _ = sshDataStream.Seek(outboundPacketSequenceSize + 4 + 1, SeekOrigin.Begin);

                    if (compressor != null)
                    {
                        // obtain uncompressed message payload
                        using (var uncompressedDataStream = new SshDataStream(messageLength != -1 ? messageLength : DefaultCapacity))
                        {
                            WriteBytes(uncompressedDataStream);

                            // compress message payload
                            var compressedMessageData = compressor.Compress(uncompressedDataStream.ToArray());

                            // add compressed message payload
                            sshDataStream.Write(compressedMessageData, 0, compressedMessageData.Length);
                        }
                    }
                    else
                    {
                        // add message payload
                        WriteBytes(sshDataStream);
                    }

                    messageLength = (int) sshDataStream.Length - (outboundPacketSequenceSize + 4 + 1);

                    var packetLength = messageLength + 4 + 1;

                    // determine the padding length
                    // in Encrypt-then-MAC mode, the length field is not encrypted, so we should keep it out of the
                    // padding length calculation
                    var paddingLength = GetPaddingLength(paddingMultiplier, etm ? packetLength - 4 : packetLength);

                    // add padding bytes
                    var paddingBytes = new byte[paddingLength];
                    CryptoAbstraction.GenerateRandom(paddingBytes);
                    sshDataStream.Write(paddingBytes, 0, paddingLength);

                    var packetDataLength = GetPacketDataLength(messageLength, paddingLength);

                    // skip bytes for outbound packet sequence
                    _ = sshDataStream.Seek(outboundPacketSequenceSize, SeekOrigin.Begin);

                    // add packet data length
                    sshDataStream.Write(packetDataLength);

                    // add packet padding length
                    sshDataStream.WriteByte(paddingLength);

                    return sshDataStream.ToArray();
                }
            }
            else
            {
                var packetLength = messageLength + 4 + 1;

                // determine the padding length
                // in Encrypt-then-MAC mode, the length field is not encrypted, so we should keep it out of the
                // padding length calculation
                var paddingLength = GetPaddingLength(paddingMultiplier, etm ? packetLength - 4 : packetLength);

                var packetDataLength = GetPacketDataLength(messageLength, paddingLength);

                // lets construct an SSH data stream of the exact size required
                using (var sshDataStream = new SshDataStream(packetLength + paddingLength + outboundPacketSequenceSize))
                {
                    // skip bytes for outbound packet sequenceSize
                    _ = sshDataStream.Seek(outboundPacketSequenceSize, SeekOrigin.Begin);

                    // add packet data length
                    sshDataStream.Write(packetDataLength);

                    // add packet padding length
                    sshDataStream.WriteByte(paddingLength);

                    // add message payload
                    WriteBytes(sshDataStream);

                    // add padding bytes
                    var paddingBytes = new byte[paddingLength];
                    CryptoAbstraction.GenerateRandom(paddingBytes);
                    sshDataStream.Write(paddingBytes, 0, paddingLength);

                    return sshDataStream.ToArray();
                }
            }
        }

        private static uint GetPacketDataLength(int messageLength, byte paddingLength)
        {
            return (uint) (messageLength + paddingLength + 1);
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
