
namespace Renci.SshClient.Sftp.Messages
{
    internal abstract class SftpRequestMessage : SftpMessage
    {
        public uint RequestId { get; set; }

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
