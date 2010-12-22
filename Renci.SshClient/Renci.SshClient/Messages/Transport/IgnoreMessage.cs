
namespace Renci.SshClient.Messages.Transport
{
    public class IgnoreMessage : Message
    {
        public string Data { get; private set; }

        public override MessageTypes MessageType
        {
            get { return MessageTypes.Ignore; }
        }

        protected override void LoadData()
        {
            this.Data = this.ReadString();
        }

        protected override void SaveData()
        {
            this.Write(this.Data);
        }
    }
}
