using System;

namespace Renci.SshClient.Messages.Sftp
{
    internal class OpenMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Open; }
        }

        public string Filename { get; set; }

        public Flags Flags { get; set; }

        public Attributes Attributes { get; set; }

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
