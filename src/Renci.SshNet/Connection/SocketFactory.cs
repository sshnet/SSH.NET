using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    internal class SocketFactory : ISocketFactory
    {
        public Socket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        }
    }
}
