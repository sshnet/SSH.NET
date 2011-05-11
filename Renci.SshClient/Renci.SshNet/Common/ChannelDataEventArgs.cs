namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshNet.Channels.Channel.DataReceived"/> event and <see cref="Renci.SshNet.Channels.Channel.ExtendedDataReceived"/> events.
    /// </summary>
    internal class ChannelDataEventArgs : ChannelEventArgs
    {
        /// <summary>
        /// Gets channel data.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets the data type code.
        /// </summary>
        public uint DataTypeCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataEventArgs"/> class.
        /// </summary>
        /// <param name="channelNumber">Channel number.</param>
        /// <param name="data">Channel data.</param>
        public ChannelDataEventArgs(uint channelNumber, byte[] data)
            : base(channelNumber)
        {
            this.Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataEventArgs"/> class.
        /// </summary>
        /// <param name="channelNumber">Channel number.</param>
        /// <param name="data">Channel data.</param>
        /// <param name="dataTypeCode">Channel data type code.</param>
        public ChannelDataEventArgs(uint channelNumber, byte[] data, uint dataTypeCode)
            : this(channelNumber, data)
        {
            this.DataTypeCode = dataTypeCode;
        }
    }
}
