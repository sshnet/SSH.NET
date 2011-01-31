
namespace Renci.SshClient.Sftp.Messages
{
    internal class ExtendedMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }

        public string ExtendedRequest { get; private set; }

        public ExtendedMessage()
        {

        }

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
