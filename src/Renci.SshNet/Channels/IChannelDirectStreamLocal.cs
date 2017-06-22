using System.Net.Sockets;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// A "direct-streamlocal@openssh.com" SSH channel.
    /// </summary>
    internal interface IChannelDirectStreamLocal : IChannelDirectBase
    {
        /// <summary>
        /// Opens a channel for a locally forwarded Unix socket.
        /// </summary>
        /// <param name="socketPath">Path to the socket.</param>
        /// <param name="forwardedPort">The forwarded port for which the channel is opened.</param>
        /// <param name="socket">The socket to receive requests from, and send responses from the remote host to.</param>
        void Open(string socketPath, IForwardedPort forwardedPort, Socket socket);
    }
}
