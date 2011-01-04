using System;

namespace Renci.SshClient.Sftp.Messages
{
    internal class OpenMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Open; }
        }

        public string Filename { get; set; }

        public Flags Flags { get; set; }

        public SftpFileAttributes Attributes { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            throw new NotSupportedException();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Filename);
            this.Write((uint)this.Flags);
            this.Write(this.Attributes);
        }
    }
}
