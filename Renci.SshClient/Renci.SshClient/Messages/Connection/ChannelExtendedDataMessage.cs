
namespace Renci.SshClient.Messages.Connection
{
    public class ChannelExtendedDataMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelExtendedData; }
        }

        public uint DataTypeCode { get; private set; }

        public string Data { get; private set; }

        public ChannelExtendedDataMessage()
        {

        }

        public ChannelExtendedDataMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.DataTypeCode = this.ReadUInt32();
            this.Data = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.DataTypeCode);
            this.Write(this.Data);
        }
    }
}
