using System;

namespace Renci.SshNet.Sftp.Messages
{
    internal class OpenMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Open; }
        }

        public string Filename { get; private set; }

        public Flags Flags { get; private set; }

        public SftpFileAttributes Attributes { get; private set; }

        public OpenMessage()
        {

        }

        public OpenMessage(uint requestId, string fileName, Flags flags)
            : base(requestId)
        {
            this.Filename = fileName;
            this.Flags = flags;
            this.Attributes = new SftpFileAttributes();
        }

        public OpenMessage(uint requestId, string fileName, Flags flags, SftpFileAttributes attributes)
            : base(requestId)
        {
            this.Filename = fileName;
            this.Flags = flags;
            this.Attributes = attributes;
        }

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
