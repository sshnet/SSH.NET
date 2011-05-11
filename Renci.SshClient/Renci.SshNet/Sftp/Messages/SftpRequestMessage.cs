
namespace Renci.SshNet.Sftp.Messages
{
    internal abstract class SftpRequestMessage : SftpMessage
    {
        public uint RequestId { get; private set; }

        public SftpRequestMessage()
        {

        }

        public SftpRequestMessage(uint requestId)
        {
            this.RequestId = requestId;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.RequestId = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.RequestId);
        }
    }
}
