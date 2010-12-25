using System;

namespace Renci.SshClient.Sftp.Messages
{
    internal class WriteMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Write; }
        }

        public string Handle { get; set; }

        public UInt64 Offset { get; set; }

        public string Data { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadString();
            this.Offset = this.ReadUInt64();
            this.Data = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Handle);
            this.Write(this.Offset);
            this.Write(this.Data);
        }
    }
}
