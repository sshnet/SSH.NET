
namespace Renci.SshClient.Sftp.Messages
{
    internal class HandleMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Handle; }
        }

        public byte[] Handle { get; private set; }

        public HandleMessage()
        {

        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
        }
    }
}
