
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelRequest; }
        }

        public string RequestName { get; protected set; }

        public bool WantReply { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.RequestName = this.ReadString();
            this.WantReply = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.RequestName);
            this.Write(this.WantReply);
        }
    }
}
