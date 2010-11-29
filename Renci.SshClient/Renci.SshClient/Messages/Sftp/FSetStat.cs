
namespace Renci.SshClient.Messages.Sftp
{
    internal class FSetStat : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FSetStat; }
        }

        public string Handle { get; set; }

        public Attributes Attributes { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadString();
            this.Attributes = this.ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Handle);
            this.Write(this.Attributes);
        }
    }
}
