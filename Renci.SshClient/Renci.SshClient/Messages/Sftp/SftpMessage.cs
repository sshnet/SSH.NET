using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Common;

namespace Renci.SshClient.Messages.Sftp
{
    internal abstract class SftpMessage : SshData
    {
        private delegate T LoadFunc<out T>(IEnumerable<byte> data);

        private static IDictionary<SftpMessageTypes, LoadFunc<SftpMessage>> _sftpMessageTypes = new Dictionary<SftpMessageTypes, LoadFunc<SftpMessage>>();

        public static SftpMessage Load(IEnumerable<byte> data)
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
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Init, new LoadFunc<SftpMessage>(Load<InitMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Version, new LoadFunc<SftpMessage>(Load<VersionMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Open, new LoadFunc<SftpMessage>(Load<OpenMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Close, new LoadFunc<SftpMessage>(Load<CloseMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Read, new LoadFunc<SftpMessage>(Load<ReadMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Write, new LoadFunc<SftpMessage>(Load<WriteMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.LStat, new LoadFunc<SftpMessage>(Load<LStatMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.FStat, new LoadFunc<SftpMessage>(Load<FStatMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.SetStat, new LoadFunc<SftpMessage>(Load<SetStatMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.FSetStat, new LoadFunc<SftpMessage>(Load<FSetStatMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.OpenDir, new LoadFunc<SftpMessage>(Load<OpenDirMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.ReadDir, new LoadFunc<SftpMessage>(Load<ReadDirMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Remove, new LoadFunc<SftpMessage>(Load<RemoveMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.MkDir, new LoadFunc<SftpMessage>(Load<MkDirMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.RmDir, new LoadFunc<SftpMessage>(Load<RmDirMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.RealPath, new LoadFunc<SftpMessage>(Load<RealPathMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Stat, new LoadFunc<SftpMessage>(Load<StatMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Rename, new LoadFunc<SftpMessage>(Load<RenameMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.ReadLink, new LoadFunc<SftpMessage>(Load<ReadLinkMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.SymLink, new LoadFunc<SftpMessage>(Load<SymLinkMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Status, new LoadFunc<SftpMessage>(Load<StatusMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Handle, new LoadFunc<SftpMessage>(Load<HandleMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Data, new LoadFunc<SftpMessage>(Load<DataMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Name, new LoadFunc<SftpMessage>(Load<NameMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Attrs, new LoadFunc<SftpMessage>(Load<AttrsMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.Extended, new LoadFunc<SftpMessage>(Load<ExtendedMessage>));
            SftpMessage._sftpMessageTypes.Add(SftpMessageTypes.ExtendedReply, new LoadFunc<SftpMessage>(Load<ExtendedReplyMessage>));
        }

        public abstract SftpMessageTypes SftpMessageType { get; }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
            this.Write((byte)this.SftpMessageType);
        }

        protected Attributes ReadAttributes()
        {
            var attributes = new Attributes();

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
                attributes.Extentions = this.ReadExtensionPair();
            }

            return attributes;
        }

        protected void Write(Attributes attributes)
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

                if (attributes.Permissions.HasValue)
                {
                    flag |= 0x00000004;
                }

                if (attributes.AccessTime.HasValue && attributes.ModifyTime.HasValue)
                {
                    flag |= 0x00000008;
                }

                if (attributes.Extentions != null)
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

                if (attributes.Permissions.HasValue)
                {
                    this.Write(attributes.Permissions.Value);
                }

                if (attributes.AccessTime.HasValue && attributes.ModifyTime.HasValue)
                {
                    uint time = (uint)(attributes.AccessTime.Value.ToFileTime() / 10000000 - 11644473600);
                    this.Write(time);
                    time = (uint)(attributes.ModifyTime.Value.ToFileTime() / 10000000 - 11644473600);
                    this.Write(time);
                }

                if (attributes.Extentions != null)
                {
                    this.Write(attributes.Extentions);
                }
            }
        }

        private static SftpMessage Load(IEnumerable<byte> data, SftpMessageTypes messageType)
        {
            if (SftpMessage._sftpMessageTypes.ContainsKey(messageType))
            {
                return SftpMessage._sftpMessageTypes[messageType](data);
            }
            else
            {
                throw new NotSupportedException(string.Format("Message type '{0}' is not registered.", messageType));
            }
        }

        private static T Load<T>(IEnumerable<byte> data) where T : SftpMessage, new()
        {
            var messageType = (SftpMessageTypes)data.FirstOrDefault();

            T message = new T();

            message.LoadBytes(data);

            message.ResetReader();

            message.LoadData();

            return message;
        }



    }
}
