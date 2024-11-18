using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Represents a factory to create <see cref="Socket"/> instances.
    /// </summary>
    internal sealed class SocketFactory : ISocketFactory
    {
        /// <inheritdoc/>
        public Socket Create(SocketType socketType, ProtocolType protocolType)
        {
            return new Socket(socketType, protocolType) { NoDelay = true };
        }
    }
}
