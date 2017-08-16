using System.IO;
using Renci.SshNet.Common;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
    internal abstract class SftpMessage : SshData
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
                // 4 bytes for the length of the SFTP data
                // 1 byte for the SFTP message type
                return 5;
            }
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

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SFTP Message : {0}", SftpMessageType);
        }
    }
}
