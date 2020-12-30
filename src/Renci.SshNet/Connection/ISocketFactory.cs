using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    internal interface ISocketFactory
    {
        Socket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}
