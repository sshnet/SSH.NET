using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Represents a factory to create <see cref="Socket"/> instances.
    /// </summary>
    internal interface ISocketFactory
    {
        /// <summary>
        /// Creates a <see cref="Socket"/> with the specified <see cref="SocketType"/>
        /// and <see cref="ProtocolType"/> that does not use the <c>Nagle</c> algorithm.
        /// </summary>
        /// <param name="socketType">The <see cref="SocketType"/>.</param>
        /// <param name="protocolType">The <see cref="ProtocolType"/>.</param>
        /// <returns>
        /// The <see cref="Socket"/>.
        /// </returns>
        Socket Create(SocketType socketType, ProtocolType protocolType);
    }
}
