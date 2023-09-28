namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Channels.Channel.ExtendedDataReceived"/> events.
    /// </summary>
    internal sealed class ChannelExtendedDataEventArgs : ChannelDataEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelExtendedDataEventArgs"/> class.
        /// </summary>
        /// <param name="channelNumber">Channel number.</param>
        /// <param name="data">Channel data.</param>
        /// <param name="dataTypeCode">Channel data type code.</param>
        public ChannelExtendedDataEventArgs(uint channelNumber, byte[] data, uint dataTypeCode)
            : base(channelNumber, data)
        {
            DataTypeCode = dataTypeCode;
        }

        /// <summary>
        /// Gets the data type code.
        /// </summary>
        /// <value>
        /// The data type code.
        /// </value>
        public uint DataTypeCode { get; }
    }
}
