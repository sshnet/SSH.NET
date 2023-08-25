﻿namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Channels.Channel.DataReceived"/> event.
    /// </summary>
    internal class ChannelDataEventArgs : ChannelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataEventArgs"/> class.
        /// </summary>
        /// <param name="channelNumber">Channel number.</param>
        /// <param name="data">Channel data.</param>
        public ChannelDataEventArgs(uint channelNumber, byte[] data)
            : base(channelNumber)
        {
            Data = data;
        }

        /// <summary>
        /// Gets channel data.
        /// </summary>
        public byte[] Data { get; }
    }
}
