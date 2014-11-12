using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Represents SSH channel.
    /// </summary>
    internal interface IChannel : IDisposable
    {
        /// <summary>
        /// Occurs when <see cref="ChannelDataMessage"/> message received
        /// </summary>
        event EventHandler<ChannelDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when an exception is thrown when processing channel messages.
        /// </summary>
        event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Occurs when <see cref="ChannelExtendedDataMessage"/> message received
        /// </summary>
        event EventHandler<ChannelDataEventArgs> ExtendedDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelRequestMessage"/> message received
        /// </summary>
        event EventHandler<ChannelRequestEventArgs> RequestReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelCloseMessage"/> message received
        /// </summary>
        event EventHandler<ChannelEventArgs> Closed;

        /// <summary>
        /// Gets the local channel number.
        /// </summary>
        /// <value>
        /// The local channel number.
        /// </value>
        uint LocalChannelNumber { get; }

        /// <summary>
        /// Gets the maximum size of a packet.
        /// </summary>
        /// <value>
        /// The maximum size of a packet.
        /// </value>
        uint LocalPacketSize { get; }

        /// <summary>
        /// Gets the maximum size of a data packet that can be sent using the channel.
        /// </summary>
        /// <value>
        /// The maximum size of data that can be sent using a <see cref="ChannelDataMessage"/>
        /// on the current channel.
        /// </value>
        /// <exception cref="InvalidOperationException">The channel has not been opened, or the open has not yet been confirmed.</exception>
        uint RemotePacketSize { get; }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets a value indicating whether this channel is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this channel is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_DATA message with the specified payload.
        /// </summary>
        /// <param name="data">The payload to send.</param>
        void SendData(byte[] data);
    }
}
