
namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_IGNORE", 2)]
    public class IgnoreMessage : Message
    {
        public string Data { get; private set; }

        public IgnoreMessage()
        {
            this.Data = string.Empty;
        }

        public IgnoreMessage(string data)
        {
            this.Data = data;
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
