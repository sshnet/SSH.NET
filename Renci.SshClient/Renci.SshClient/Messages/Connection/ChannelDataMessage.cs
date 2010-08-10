namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelDataMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelData; }
        }

        public string Data { get; set; }

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
