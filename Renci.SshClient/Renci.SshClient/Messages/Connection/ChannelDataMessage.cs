namespace Renci.SshClient.Messages.Connection
{
    public class ChannelDataMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelData; }
        }

        public string Data { get; protected set; }

        public ChannelDataMessage()
            : base()
        {

        }

        public ChannelDataMessage(uint localChannelNumber, string data)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.Data = data;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Data = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Data);
        }
    }
}
