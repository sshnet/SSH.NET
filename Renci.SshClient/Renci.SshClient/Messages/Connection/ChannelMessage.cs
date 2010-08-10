
namespace Renci.SshClient.Messages.Connection
{
    internal abstract class ChannelMessage : Message
    {
        public uint ChannelNumber { get; set; }

        protected override void LoadData()
        {
            this.ChannelNumber = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            this.Write(this.ChannelNumber);
        }
    }
}
