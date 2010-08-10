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

        public uint? RequestId { get; set; }

        protected override void LoadData()
        {
            //  SSH_FXP_INIT and SSH_FXP_VERSION doesnt have RequestID, all other messaages do
            if (!(this.SftpMessageType == SftpMessageTypes.Init || this.SftpMessageType == SftpMessageTypes.Version))
            {
                this.RequestId = this.ReadUInt32();
            }
        }

        protected override void SaveData()
        {
            this.Write((byte)this.SftpMessageType);
            if (this.RequestId.HasValue)
                this.Write(this.RequestId.Value);
        }

        protected Attributes ReadAttributes()
        {
            var attributes = new Attributes();
            attributes.Flag = this.ReadUInt32();

            var isSize = (attributes.Flag & 0x00000001) == 0x00000001;  //SSH_FILEXFER_ATTR_SIZE              0x00000001
            var isUidGid = (attributes.Flag & 0x00000002) == 0x00000002;  //SSH_FILEXFER_ATTR_UIDGID              0x00000002
            var isPermissions = (attributes.Flag & 0x00000004) == 0x00000004;  //SSH_FILEXFER_ATTR_PERMISSIONS       0x00000004
            var isAccessModifyTime = (attributes.Flag & 0x00000008) == 0x00000008;  //SSH_FILEXFER_ATTR_ACMODTIME        0x00000008

            var isExtended = (attributes.Flag & 0x80000000) == 0x80000000;  //SSH_FILEXFER_ATTR_EXTENDED          0x80000000

            if (isSize)
            {
                attributes.Size = this.ReadUInt64();
            }

            if (isUidGid)
            {
                attributes.UserId = this.ReadUInt32();

                attributes.GroupId = this.ReadUInt32();
            }

            if (isPermissions)
            {
                attributes.Permissions = this.ReadUInt32();
            }

            if (isAccessModifyTime)
            {
                var time = this.ReadUInt32();
                attributes.AccessTime = DateTime.FromFileTime((time + 11644473600) * 10000000);
                time = this.ReadUInt32();
                attributes.ModifyTime = DateTime.FromFileTime((time + 11644473600) * 10000000);
            }

            if (isExtended)
            {
                var extendedCount = this.ReadUInt32();
                attributes.Extentions = this.ReadExtensionPair();
            }

            return attributes;
        }

        protected void Write(Attributes attributes)
        {
            //  TODO:   Complete attribute serialization, at this point we pass no attributes
            if (attributes == null)
            {
                this.Write((uint)0);
                return;
            }
            else
            {
                //  TODO:   Need to be tested
                throw new NotImplementedException();

                var isSize = (attributes.Flag & 0x00000001) == 0x00000001;  //SSH_FILEXFER_ATTR_SIZE              0x00000001
                var isUidGid = (attributes.Flag & 0x00000002) == 0x00000002;  //SSH_FILEXFER_ATTR_UIDGID              0x00000002
                var isPermissions = (attributes.Flag & 0x00000004) == 0x00000004;  //SSH_FILEXFER_ATTR_PERMISSIONS       0x00000004
                var isAccessModifyTime = (attributes.Flag & 0x00000008) == 0x00000008;  //SSH_FILEXFER_ATTR_ACMODTIME        0x00000008

                var isExtended = (attributes.Flag & 0x80000000) == 0x80000000;  //SSH_FILEXFER_ATTR_EXTENDED          0x80000000

                if (isSize)
                {
                    this.Write(attributes.Size);
                }

                if (isUidGid)
                {
                    this.Write(attributes.UserId);

                    this.Write(attributes.GroupId);
                }

                if (isPermissions)
                {
                    this.Write(attributes.Permissions);
                }

                if (isAccessModifyTime)
                {
                    uint time = (uint)(attributes.AccessTime.ToFileTime() - 11644473600) / 10000000;
                    this.Write(time);
                    time = (uint)(attributes.ModifyTime.ToFileTime() - 11644473600) / 10000000;
                    this.Write(time);
                }

                if (isExtended)
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
