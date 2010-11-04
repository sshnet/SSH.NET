namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelOpenConfirmationMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelOpenConfirmation; }
        }

        public uint RemoteChannelNumber { get; set; }

        public uint InitialWindowSize { get; set; }

        public uint MaximumPacketSize { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.RemoteChannelNumber = this.ReadUInt32();
            this.InitialWindowSize = this.ReadUInt32();
            this.MaximumPacketSize = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.RemoteChannelNumber);
            this.Write(this.InitialWindowSize);
            this.Write(this.MaximumPacketSize);
        }
    }
}
