
namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_REQUEST_SUCCESS", 81)]
    public class RequestSuccessMessage : Message
    {
        public uint? BoundPort { get; private set; }

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
