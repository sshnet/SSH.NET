
namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_DEBUG", 4)]
    public class DebugMessage : Message
    {
        public bool IsAlwaysDisplay { get; private set; }

        public string Message { get; private set; }

        public string Language { get; private set; }

        protected override void LoadData()
        {
            this.IsAlwaysDisplay = this.ReadBoolean();
            this.Message = this.ReadString();
            this.Language = this.ReadString();
        }

        protected override void SaveData()
        {
            this.Write(this.IsAlwaysDisplay);
            this.Write(this.Message);
            this.Write(this.Language);
        }
    }
}
