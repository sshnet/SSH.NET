namespace Renci.SshClient.Channels
{
    internal class ChannelOpenFailedEventArgs : ChannelEventArgs
    {
        public uint ReasonCode { get; private set; }

        public string Description { get; private set; }

        public string Language { get; private set; }

        public ChannelOpenFailedEventArgs(uint channelNumber, uint reasonCode, string description, string language)
            : base(channelNumber)
        {
            this.ReasonCode = reasonCode;
            this.Description = description;
            this.Language = language;
        }
    }
}
