
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelOpenMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelOpen; }
        }

        public string ChannelName { get; set; }

        public uint InitialWindowSize { get; set; }

        public uint MaximumPacketSize { get; set; }

        protected override void LoadData()
        {
            this.ChannelName = this.ReadString();
            this.ChannelNumber = this.ReadUInt32();
            this.InitialWindowSize = this.ReadUInt32();
            this.MaximumPacketSize = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            this.Write(this.ChannelName);
            this.Write(this.ChannelNumber);
            this.Write(this.InitialWindowSize);
            this.Write(this.MaximumPacketSize);
        }
    }
}
