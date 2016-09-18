using System;
using System.Net.Sockets;
using Renci.SshNet.Common;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// A "direct-tcpip" SSH channel.
    /// </summary>
    internal interface IChannelDirectTcpip : IDisposable
    {
        /// <summary>
        /// Occurs when an exception is thrown while processing channel messages.
        /// </summary>
        event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Gets a value indicating whether this channel is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this channel is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Gets the local channel number.
        /// </summary>
        /// <value>
        /// The local channel number.
        /// </value>
        uint LocalChannelNumber { get; }

        /// <summary>
        /// Opens a channel for a locally forwarded TCP/IP port.
        /// </summary>
        /// <param name="remoteHost">The name of the remote host to forward to.</param>
        /// <param name="port">The port of the remote hosts to forward to.</param>
        /// <param name="forwardedPort">The forwarded port for which the channel is opened.</param>
        /// <param name="socket">The socket to receive requests from, and send responses from the remote host to.</param>
        void Open(string remoteHost, uint port, IForwardedPort forwardedPort, Socket socket);

        /// <summary>
        /// Binds the channel to the remote host.
        /// </summary>
        void Bind();
    }
}
