
namespace Renci.SshClient.Messages.Connection
{
    public class RequestSuccessMessage : Message
    {
        public uint? BoundPort { get; private set; }

        public override MessageTypes MessageType
        {
            get { return MessageTypes.RequestSuccess; }
        }

        public RequestSuccessMessage()
        {

        }

        public RequestSuccessMessage(uint boundPort)
        {
            this.BoundPort = boundPort;
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
