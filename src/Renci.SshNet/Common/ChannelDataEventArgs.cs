using System;

namespace Renci.SshNet.Common
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
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public ChannelDataEventArgs(uint channelNumber, ArraySegment<byte> data)
            : base(channelNumber)
        {
            ThrowHelper.ThrowIfNull(data.Array);

            Data = data;
        }

        internal ChannelDataEventArgs(uint channelNumber, byte[] data)
            : this(channelNumber, new ArraySegment<byte>(data))
        {
        }

        /// <summary>
        /// Gets channel data.
        /// </summary>
        public ArraySegment<byte> Data { get; }
    }
}
