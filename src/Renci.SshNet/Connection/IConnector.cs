using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    internal interface IConnector
    {
        Socket Connect(IConnectionInfo connectionInfo);
    }
}
