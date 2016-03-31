using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for all channel related events.
    /// </summary>
    internal class ChannelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the channel number.
        /// </summary>
        public uint ChannelNumber { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEventArgs"/> class.
        /// </summary>
        /// <param name="channelNumber">The channel number.</param>
        public ChannelEventArgs(uint channelNumber)
        {
            ChannelNumber = channelNumber;
        }
    }
}
