using System;

namespace Renci.SshNet.Sftp.Messages
{
    internal class ReadMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Read; }
        }

        public byte[] Handle { get; private set; }

        public UInt64 Offset { get; private set; }

        public UInt32 Length { get; private set; }

        public ReadMessage()
        {

        }

        public ReadMessage(uint requestId, byte[] handle, UInt64 offset, UInt32 length)
            : base(requestId)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
            this.Offset = this.ReadUInt64();
            this.Length = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
            this.Write(this.Offset);
            this.Write(this.Length);
        }
    }
}
