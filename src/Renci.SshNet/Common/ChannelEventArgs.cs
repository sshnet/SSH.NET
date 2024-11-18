using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for all channel related events.
    /// </summary>
    internal class ChannelEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEventArgs"/> class.
        /// </summary>
        /// <param name="channelNumber">The channel number.</param>
        public ChannelEventArgs(uint channelNumber)
        {
            ChannelNumber = channelNumber;
        }

        /// <summary>
        /// Gets the channel number.
        /// </summary>
        /// <value>
        /// The channel number.
        /// </value>
        public uint ChannelNumber { get; }
    }
}
