using System.Text;

namespace Renci.SshClient.Messages.Transport
{
    public class DisconnectMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Disconnect; }
        }

        public DisconnectReasons ReasonCode { get; private set; }

        public string Description { get; private set; }

        public string Language { get; private set; }

        public DisconnectMessage()
        {

        }

        public DisconnectMessage(DisconnectReasons reasonCode, string message)
        {
            this.ReasonCode = reasonCode;
            this.Description = message;
        }

        protected override void LoadData()
        {
            this.ReasonCode = (DisconnectReasons)this.ReadUInt32();
            this.Description = this.ReadString();
            this.Language = this.ReadString();
        }

        protected override void SaveData()
        {
            this.Write((uint)this.ReasonCode);
            this.Write(this.Description, Encoding.UTF8);
            this.Write(this.Language ?? "en");
        }
    }
}
