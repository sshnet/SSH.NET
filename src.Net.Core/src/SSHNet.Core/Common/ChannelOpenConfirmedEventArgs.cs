namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshNet.Channels.ClientChannel.OpenConfirmed"/> event.
    /// </summary>
    internal class ChannelOpenConfirmedEventArgs : ChannelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenConfirmedEventArgs"/> class.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">The initial window size.</param>
        /// <param name="maximumPacketSize">The maximum packet size.</param>
        public ChannelOpenConfirmedEventArgs(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
            : base(remoteChannelNumber)
        {
            InitialWindowSize = initialWindowSize;
            MaximumPacketSize = maximumPacketSize;
        }

        /// <summary>
        /// Gets the initial size of the window.
        /// </summary>
        /// <value>
        /// The initial size of the window.
        /// </value>
        public uint InitialWindowSize { get; private set; }

        /// <summary>
        /// Gets the maximum size of the packet.
        /// </summary>
        /// <value>
        /// The maximum size of the packet.
        /// </value>
        public uint MaximumPacketSize { get; private set; }
    }
}
