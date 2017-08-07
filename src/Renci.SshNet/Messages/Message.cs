using System.IO;
using Renci.SshNet.Common;
using System.Globalization;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Compression;

namespace Renci.SshNet.Messages
{
    /// <summary>
    /// Base class for all SSH protocol messages
    /// </summary>
    public abstract class Message : SshData
    {
        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                return 1; // Message type
            }
        }

        /// <summary>
        /// Writes the message to the specified <see cref="SshDataStream"/>.
        /// </summary>
        protected override void WriteBytes(SshDataStream stream)
        {
            var enumerator = GetType().GetCustomAttributes<MessageAttribute>(true).GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                {
                    throw new SshException(string.Format(CultureInfo.CurrentCulture, "Type '{0}' is not a valid message type.", GetType().AssemblyQualifiedName));
                }

                var messageAttribute = enumerator.Current;
                stream.WriteByte(messageAttribute.Number);
                base.WriteBytes(stream);
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        internal byte[] GetPacket(byte paddingMultiplier, Compressor compressor)
        {
            const int outboundPacketSequenceSize = 4;

            var messageLength = BufferCapacity;

            SshDataStream sshDataStream;

            if (messageLength == -1 || compressor != null)
            {
                sshDataStream = new SshDataStream(DefaultCapacity);

                // skip:
                // * 4 bytes for the outbound packet sequence
                // * 4 bytes for the packet data length
                // * one byte for the packet padding length
                sshDataStream.Seek(outboundPacketSequenceSize + 4 + 1, SeekOrigin.Begin);

                if (compressor != null)
                {
                    // obtain uncompressed message payload
                    var uncompressedDataStream = new SshDataStream(messageLength != -1 ? messageLength : DefaultCapacity);
                    WriteBytes(uncompressedDataStream);

                    // compress message payload
                    var compressedMessageData = compressor.Compress(uncompressedDataStream.ToArray());

                    // add compressed message payload
                    sshDataStream.Write(compressedMessageData, 0, compressedMessageData.Length);
                }
                else
                {
                    // add message payload
                    WriteBytes(sshDataStream);
                }

                messageLength = (int) sshDataStream.Length - (outboundPacketSequenceSize + 4 + 1);

                var packetLength = messageLength + 4 + 1;

                // determine the padding length
                var paddingLength = GetPaddingLength(paddingMultiplier, packetLength);

                // add padding bytes
                var paddingBytes = new byte[paddingLength];
                CryptoAbstraction.GenerateRandom(paddingBytes);
                sshDataStream.Write(paddingBytes, 0, paddingLength);

                var packetDataLength = GetPacketDataLength(messageLength, paddingLength);

                // skip bytes for outbound packet sequence
                sshDataStream.Seek(outboundPacketSequenceSize, SeekOrigin.Begin);

                // add packet data length
                sshDataStream.Write(packetDataLength);

                //  add packet padding length
                sshDataStream.WriteByte(paddingLength);
            }
            else
            {
                var packetLength = messageLength + 4 + 1;

                // determine the padding length
                var paddingLength = GetPaddingLength(paddingMultiplier, packetLength);

                var packetDataLength = GetPacketDataLength(messageLength, paddingLength);

                // lets construct an SSH data stream of the exact size required
                sshDataStream = new SshDataStream(packetLength + paddingLength + outboundPacketSequenceSize);

                // skip bytes for outbound packet sequenceSize
                sshDataStream.Seek(outboundPacketSequenceSize, SeekOrigin.Begin);

                // add packet data length
                sshDataStream.Write(packetDataLength);

                //  add packet padding length
                sshDataStream.WriteByte(paddingLength);

                // add message payload
                WriteBytes(sshDataStream);

                // add padding bytes
                var paddingBytes = new byte[paddingLength];
                CryptoAbstraction.GenerateRandom(paddingBytes);
                sshDataStream.Write(paddingBytes, 0, paddingLength);
            }

            return sshDataStream.ToArray();
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

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var enumerator = GetType().GetCustomAttributes<MessageAttribute>(true).GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                {
                    return string.Format(CultureInfo.CurrentCulture, "'{0}' without Message attribute.", GetType().FullName);
                }

                return enumerator.Current.Name;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        /// <summary>
        /// Process the current message for the specified <see cref="Session"/>.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> for which to process the current message.</param>
        internal abstract void Process(Session session);
    }
}