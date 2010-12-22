
namespace Renci.SshClient.Messages.Transport
{
    public class DebugMessage : Message
    {
        public bool IsAlwaysDisplay { get; private set; }

        public string Message { get; private set; }

        public string Language { get; private set; }

        public override MessageTypes MessageType
        {
            get { return MessageTypes.Debug; }
        }

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
