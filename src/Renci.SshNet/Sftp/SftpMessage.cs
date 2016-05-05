using System;
using System.IO;
using Renci.SshNet.Common;
using System.Globalization;
using Renci.SshNet.Sftp.Responses;
using System.Text;

namespace Renci.SshNet.Sftp
{
    internal abstract class SftpMessage : SshData
    {
        public static SftpMessage Load(uint protocolVersion, byte[] data, Encoding encoding)
        {
            var messageType = (SftpMessageTypes) data[4]; // skip packet length bytes

            return Load(protocolVersion, data, messageType, encoding);
        }

        protected override int ZeroReaderIndex
        {
            get
            {
                // 4 bytes for the length of the SFTP data
                // 1 byte for the SFTP message type
                return 5;
            }
        }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get { return ZeroReaderIndex; }
        }

        public abstract SftpMessageTypes SftpMessageType { get; }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
            Write((byte) SftpMessageType);
        }

        /// <summary>
        /// Writes the current message to the specified <see cref="SshDataStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="SshDataStream"/> to write the message to.</param>
        protected override void WriteBytes(SshDataStream stream)
        {
            const int sizeOfDataLengthBytes = 4;

            var startPosition = stream.Position;

            // skip 4 bytes for the length of the SFTP message data
            stream.Seek(sizeOfDataLengthBytes, SeekOrigin.Current);

            // write the SFTP message data to the stream
            base.WriteBytes(stream);

            // save where we were positioned when we finished writing the SSH message data
            var endPosition = stream.Position;

            // determine the length of the SSH message data
            var dataLength = endPosition - startPosition - sizeOfDataLengthBytes;

            // write the length of the SFTP message where we were positioned before we started
            // writing the SFTP message data
            stream.Position = startPosition;
            stream.Write((uint) dataLength);

            // move back to we were positioned when we finished writing the SFTP message data
            stream.Position = endPosition;
        }

        protected SftpFileAttributes ReadAttributes()
        {
            return SftpFileAttributes.FromBytes(DataStream);
        }

        private static SftpMessage Load(uint protocolVersion, byte[] data, SftpMessageTypes messageType, Encoding encoding)
        {
            SftpMessage message;

            switch (messageType)
            {
                case SftpMessageTypes.Version:
                    message = new SftpVersionResponse();
                    break;
                case SftpMessageTypes.Status:
                    message = new SftpStatusResponse(protocolVersion);
                    break;
                case SftpMessageTypes.Data:
                    message = new SftpDataResponse(protocolVersion);
                    break;
                case SftpMessageTypes.Handle:
                    message = new SftpHandleResponse(protocolVersion);
                    break;
                case SftpMessageTypes.Name:
                    message = new SftpNameResponse(protocolVersion, encoding);
                    break;
                case SftpMessageTypes.Attrs:
                    message = new SftpAttrsResponse(protocolVersion);
                    break;
                case SftpMessageTypes.ExtendedReply:
                    message = new SftpExtendedReplyResponse(protocolVersion);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Message type '{0}' is not supported.", messageType));
            }

            message.Load(data);

            return message;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SFTP Message : {0}", SftpMessageType);
        }
    }
}
