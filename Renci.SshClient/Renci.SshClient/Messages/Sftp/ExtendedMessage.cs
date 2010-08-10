
namespace Renci.SshClient.Messages.Sftp
{
    internal class ExtendedMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }

        public string ExtendedRequest { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.ExtendedRequest = this.ReadString();
            //  TODO:   Read extended request data
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.ExtendedRequest);
            //  TODO:   Save extended request data
        }
    }
}
