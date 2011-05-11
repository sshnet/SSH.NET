using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet.Common;
using System.Globalization;

namespace Renci.SshNet.Sftp.Messages
{
    internal abstract class SftpMessage : SshData
    {
        private delegate T LoadFunc<out T>(byte[] data);

        private static IDictionary<SftpMessageTypes, LoadFunc<SftpMessage>> _sftpMessageTypes = new Dictionary<SftpMessageTypes, LoadFunc<SftpMessage>>();

        public new static SftpMessage Load(byte[] data)
        {
            var messageType = (SftpMessageTypes)data.FirstOrDefault();

            return Load(data, messageType);
        }

        protected override int ZeroReaderIndex
        {
            get
            {
                return 1;
            }
        }

        static SftpMessage()
        {
            //  Register only messages that can be received by the client
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Version, new LoadFunc<SftpMessage>(Load<VersionMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Status, new LoadFunc<SftpMessage>(Load<StatusMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Data, new LoadFunc<SftpMessage>(Load<DataMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Handle, new LoadFunc<SftpMessage>(Load<HandleMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Name, new LoadFunc<SftpMessage>(Load<NameMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Attrs, new LoadFunc<SftpMessage>(Load<AttributesMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Extended, new LoadFunc<SftpMessage>(Load<ExtendedMessage>));
        }

        public abstract SftpMessageTypes SftpMessageType { get; }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
            this.Write((byte)this.SftpMessageType);
        }

        protected SftpFileAttributes ReadAttributes()
        {
            var attributes = new SftpFileAttributes();

            var flag = this.ReadUInt32();

            if ((flag & 0x00000001) == 0x00000001)   //  SSH_FILEXFER_ATTR_SIZE
            {
                attributes.Size = this.ReadUInt64();
            }

            if ((flag & 0x00000002) == 0x00000002)   //  SSH_FILEXFER_ATTR_UIDGID
            {
                attributes.UserId = this.ReadUInt32();

                attributes.GroupId = this.ReadUInt32();
            }

            if ((flag & 0x00000004) == 0x00000004)   //  SSH_FILEXFER_ATTR_PERMISSIONS
            {
                attributes.Permissions = this.ReadUInt32();
            }

            if ((flag & 0x00000008) == 0x00000008)   //  SSH_FILEXFER_ATTR_ACMODTIME
            {
                var time = this.ReadUInt32();
                attributes.AccessTime = DateTime.FromFileTime((time + 11644473600) * 10000000);
                time = this.ReadUInt32();
                attributes.ModifyTime = DateTime.FromFileTime((time + 11644473600) * 10000000);
            }

            if ((flag & 0x80000000) == 0x80000000)   //  SSH_FILEXFER_ATTR_ACMODTIME
            {
                var extendedCount = this.ReadUInt32();
                attributes.Extensions = this.ReadExtensionPair();
            }

            return attributes;
        }

        protected void Write(SftpFileAttributes attributes)
        {
            if (attributes == null)
            {
                this.Write((uint)0);
                return;
            }
            else
            {
                UInt32 flag = 0;

                if (attributes.Size.HasValue)
                {
                    flag |= 0x00000001;
                }

                if (attributes.UserId.HasValue && attributes.GroupId.HasValue)
                {
                    flag |= 0x00000002;
                }

                if (attributes.Permissions > 0)
                {
                    flag |= 0x00000004;
                }

                if (attributes.AccessTime.HasValue && attributes.ModifyTime.HasValue)
                {
                    flag |= 0x00000008;
                }

                if (attributes.Extensions != null)
                {
                    flag |= 0x80000000;
                }

                this.Write(flag);

                if (attributes.Size.HasValue)
                {
                    this.Write(attributes.Size.Value);
                }

                if (attributes.UserId.HasValue && attributes.GroupId.HasValue)
                {
                    this.Write(attributes.UserId.Value);
                    this.Write(attributes.GroupId.Value);
                }

                if (attributes.Permissions  > 0)
                {
                    this.Write(attributes.Permissions);
                }

                if (attributes.AccessTime.HasValue && attributes.ModifyTime.HasValue)
                {
                    uint time = (uint)(attributes.AccessTime.Value.ToFileTime() / 10000000 - 11644473600);
                    this.Write(time);
                    time = (uint)(attributes.ModifyTime.Value.ToFileTime() / 10000000 - 11644473600);
                    this.Write(time);
                }

                if (attributes.Extensions != null)
                {
                    this.Write(attributes.Extensions);
                }
            }
        }

        private static SftpMessage Load(byte[] data, SftpMessageTypes messageType)
        {
            if (SftpMessage._sftpMessageTypes.ContainsKey(messageType))
            {
                return SftpMessage._sftpMessageTypes[messageType](data);
            }
            else
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Message type '{0}' is not registered.", messageType));
            }
        }

        private static T Load<T>(byte[] data) where T : SftpMessage, new()
        {
            T message = new T();

            message.LoadBytes(data);

            message.ResetReader();

            message.LoadData();

            return message;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SFTP Message : {0}", this.SftpMessageType);
        }
    }
}
