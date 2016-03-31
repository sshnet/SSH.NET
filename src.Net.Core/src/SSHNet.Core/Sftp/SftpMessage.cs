using System;
#if false //old  !TUNING
using System.Collections.Generic;
#endif
using System.IO;
#if false //old  !TUNING
using System.Linq;
#endif
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
#if true //old TUNING
            var messageType = (SftpMessageTypes) data[4]; // skip packet length bytes
#else
            var messageType = (SftpMessageTypes)data.FirstOrDefault();
#endif

            return Load(protocolVersion, data, messageType, encoding);
        }

        protected override int ZeroReaderIndex
        {
            get
            {
#if true //old TUNING
                // 4 bytes for the length of the SFTP data
                // 1 byte for the SFTP message type
                return 5;
#else
                return 1;
#endif
            }
        }

#if true //old TUNING
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
#endif

        public abstract SftpMessageTypes SftpMessageType { get; }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
            Write((byte) SftpMessageType);
        }

#if true //old TUNING
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
#endif

        protected SftpFileAttributes ReadAttributes()
        {
#if true //old TUNING
            return SftpFileAttributes.FromBytes(DataStream);
#else
            var flag = ReadUInt32();

            long size = -1;
            var userId = -1;
            var groupId = -1;
            uint permissions = 0;
            var accessTime = DateTime.MinValue;
            var modifyTime = DateTime.MinValue;
            IDictionary<string, string> extensions = null;

            if ((flag & 0x00000001) == 0x00000001)   //  SSH_FILEXFER_ATTR_SIZE
            {
                size = (long)ReadUInt64();
            }

            if ((flag & 0x00000002) == 0x00000002)   //  SSH_FILEXFER_ATTR_UIDGID
            {
                userId = (int)ReadUInt32();

                groupId = (int)ReadUInt32();
            }

            if ((flag & 0x00000004) == 0x00000004)   //  SSH_FILEXFER_ATTR_PERMISSIONS
            {
                permissions = ReadUInt32();
            }

            if ((flag & 0x00000008) == 0x00000008)   //  SSH_FILEXFER_ATTR_ACMODTIME
            {
                var time = ReadUInt32();
                accessTime = DateTime.FromFileTime((time + 11644473600) * 10000000);
                time = ReadUInt32();
                modifyTime = DateTime.FromFileTime((time + 11644473600) * 10000000);
            }

            if ((flag & 0x80000000) == 0x80000000)   //  SSH_FILEXFER_ATTR_ACMODTIME
            {
                var extendedCount = ReadUInt32();
                extensions = ReadExtensionPair();
            }
            var attributes = new SftpFileAttributes(accessTime, modifyTime, size, userId, groupId, permissions, extensions);

            return attributes;
#endif
        }

#if false //old  !TUNING
        protected void Write(SftpFileAttributes attributes)
        {
            if (attributes == null)
            {
                Write((uint)0);
                return;
            }

            UInt32 flag = 0;

            if (attributes.IsSizeChanged && attributes.IsRegularFile)
            {
                flag |= 0x00000001;
            }

            if (attributes.IsUserIdChanged|| attributes.IsGroupIdChanged)
            {
                flag |= 0x00000002;
            }

            if (attributes.IsPermissionsChanged)
            {
                flag |= 0x00000004;
            }

            if (attributes.IsLastAccessTimeChanged || attributes.IsLastWriteTimeChanged)
            {
                flag |= 0x00000008;
            }

            if (attributes.IsExtensionsChanged)
            {
                flag |= 0x80000000;
            }

            Write(flag);

            if (attributes.IsSizeChanged && attributes.IsRegularFile)
            {
                Write((UInt64)attributes.Size);
            }

            if (attributes.IsUserIdChanged|| attributes.IsGroupIdChanged)
            {
                Write((UInt32)attributes.UserId);
                Write((UInt32)attributes.GroupId);
            }

            if (attributes.IsPermissionsChanged)
            {
                Write(attributes.Permissions);
            }

            if (attributes.IsLastAccessTimeChanged || attributes.IsLastWriteTimeChanged)
            {
                var time = (uint)(attributes.LastAccessTime.ToFileTime() / 10000000 - 11644473600);
                Write(time);
                time = (uint)(attributes.LastWriteTime.ToFileTime() / 10000000 - 11644473600);
                Write(time);
            }

            if (attributes.IsExtensionsChanged)
            {
                Write(attributes.Extensions);
            }
        }
#endif

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

#if true //old TUNING
            message.Load(data);
#else
            message.LoadBytes(data);

            message.ResetReader();

            message.LoadData();
#endif

            return message;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SFTP Message : {0}", SftpMessageType);
        }
    }
}
