
namespace Renci.SshClient.Messages.Connection
{
    public class ChannelWindowAdjustMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelWindowAdjust; }
        }

        public uint BytesToAdd { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.BytesToAdd = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.BytesToAdd);
        }
    }
}
