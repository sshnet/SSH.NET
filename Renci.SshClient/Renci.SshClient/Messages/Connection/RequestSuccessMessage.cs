
namespace Renci.SshClient.Messages.Connection
{
    internal class RequestSuccessMessage : Message
    {
        public uint? BoundPort { get; set; }

        public override MessageTypes MessageType
        {
            get { return MessageTypes.RequestSuccess; }
        }

        protected override void LoadData()
        {
            if (!this.IsEndOfData)
                this.BoundPort = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            if (this.BoundPort != null)
                this.Write(this.BoundPort.Value);
        }
    }
}
