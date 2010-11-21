namespace Renci.SshClient.Common
{
    internal class ChannelDataEventArgs : ChannelEventArgs
    {
        public string Data { get; private set; }

        public uint DataTypeCode { get; set; }

        public ChannelDataEventArgs(uint channelNumber, string data)
            : base(channelNumber)
        {
            this.Data = data;
        }

        public ChannelDataEventArgs(uint channelNumber, string data, uint dataTypeCode)
            : this(channelNumber, data)
        {
            this.DataTypeCode = dataTypeCode;
        }
    }
}
