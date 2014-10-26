using System.Net;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// A "forwarded-tcpip" SSH channel.
    /// </summary>
    internal interface IChannelForwardedTcpip
    {
        /// <summary>
        /// Binds the channel to the specified host.
        /// </summary>
        /// <param name="address">The IP address of the host to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        void Bind(IPAddress address, uint port);
    }
}
