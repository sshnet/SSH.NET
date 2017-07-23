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
        /// Occurs when <see cref="ChannelDataMessage"/> is received.
        /// </summary>
        event EventHandler<ChannelDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when an exception is thrown when processing channel messages.
        /// </summary>
        event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Occurs when <see cref="ChannelExtendedDataMessage"/> is received.
        /// </summary>
        event EventHandler<ChannelExtendedDataEventArgs> ExtendedDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelRequestMessage"/> is received.
        /// </summary>
        event EventHandler<ChannelRequestEventArgs> RequestReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelCloseMessage"/> is received.
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
        /// Gets the maximum size of a data packet that we can receive using the channel.
        /// </summary>
        /// <value>
        /// The maximum size of a packet.
        /// </value>
        /// <remarks>
        /// <para>
        /// This is the maximum size (in bytes) we support for the data (payload) of a
        /// <c>SSH_MSG_CHANNEL_DATA</c> message we receive.
        /// </para>
        /// <para>
        /// We currently do not enforce this limit.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_DATA message with the specified payload.
        /// </summary>
        /// <param name="data">An array of <see cref="byte"/> containing the payload to send.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin taking data from.</param>
        /// <param name="size">The number of bytes of <paramref name="data"/> to send.</param>
        /// <remarks>
        /// <para>
        /// When the size of the data to send exceeds the maximum packet size or the remote window
        /// size does not allow the full data to be sent, then this method will send the data in
        /// multiple chunks and will wait for the remote window size to be adjusted when it's zero.
        /// </para>
        /// <para>
        /// This is done to support SSH servers will a small window size that do not agressively
        /// increase their window size. We need to take into account that there may be SSH servers
        /// that only increase their window size when it has reached zero.
        /// </para>
        /// </remarks>
        void SendData(byte[] data, int offset, int size);

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_EOF message to the remote server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The channel is closed.</exception>
        void SendEof();
    }
}
