namespace Renci.SshClient.Messages.Connection
{
    public class ChannelOpenFailureMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelOpenFailure; }
        }

        public uint ReasonCode { get; private set; }

        public string Description { get; private set; }

        public string Language { get; private set; }

        public ChannelOpenFailureMessage()
        {

        }

        public ChannelOpenFailureMessage(uint localChannelNumber, string description, uint reasonCode)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.Description = description;
            this.ReasonCode = reasonCode;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.ReasonCode = this.ReadUInt32();
            this.Description = this.ReadString();
            this.Language = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.ReasonCode);
            this.Write(this.Description ?? string.Empty);
            this.Write(this.Language ?? "en");
        }
    }
}
