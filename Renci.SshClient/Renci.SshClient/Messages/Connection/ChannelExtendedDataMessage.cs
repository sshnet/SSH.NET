
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelExtendedDataMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelExtendedData; }
        }

        public uint DataTypeCode { get; set; }

        public string Data { get; set; }

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
