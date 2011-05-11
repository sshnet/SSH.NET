
namespace Renci.SshClient.Sftp.Messages
{
    internal class FStatMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FStat; }
        }

        public byte[] Handle { get; private set; }

        public FStatMessage()
        {

        }

        public FStatMessage(uint requestId, byte[] handle)
            : base(requestId)
        {
            this.Handle = handle;
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
