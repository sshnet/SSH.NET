namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_CHANNEL_WINDOW_ADJUST", 93)]
    public class ChannelWindowAdjustMessage : ChannelMessage
    {
        public uint BytesToAdd { get; private set; }

        public ChannelWindowAdjustMessage()
        {

        }

        public ChannelWindowAdjustMessage(uint localChannelNumber, uint bytesToAdd)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.BytesToAdd = bytesToAdd;
        }

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
