using System.Collections.Generic;
using Renci.SshClient.Common;

namespace Renci.SshClient.Messages.Sftp
{
    internal class NameMessage : SftpRequestMessage
    {

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Name; }
        }

        public uint Count { get; set; }

        public IList<FtpFileInfo> Files { get; set; }

        public NameMessage()
        {
            this.Files = new List<FtpFileInfo>();
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Count = this.ReadUInt32();
            for (int i = 0; i < this.Count; i++)
            {
                var fileName = this.ReadString();
                var fullName = this.ReadString();
                var attribute = this.ReadAttributes();

                this.Files.Add(new FtpFileInfo
                {
                    Name = fileName,
                    FullName = fullName,
                    Size = attribute.Size,
                    UserId = attribute.UserId,
                    GroupId = attribute.GroupId,
                    LastAccessTime = attribute.AccessTime,
                    LastModifyTime = attribute.ModifyTime,
                    Extentions = attribute.Extentions,
                });
            }
        }

        protected override void SaveData()
        {
            base.SaveData();
        }
    }
}
