
using System;
namespace Renci.SshClient.Sftp.Messages
{
    internal class ReadMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Read; }
        }

        public string Handle { get; set; }

        public UInt64 Offset { get; set; }

        public UInt32 Length { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadString();
            this.Offset = this.ReadUInt64();
            this.Length = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Handle);
            this.Write(this.Offset);
            this.Write(this.Length);
        }
    }
}
