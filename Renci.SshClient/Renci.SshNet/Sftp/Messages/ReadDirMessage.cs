
namespace Renci.SshNet.Sftp.Messages
{
    internal class ReadDirMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ReadDir; }
        }

        public byte[] Handle { get; private set; }

        public ReadDirMessage()
        {

        }

        public ReadDirMessage(uint requestId, byte[] handle)
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
