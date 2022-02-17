using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Connection
{
    internal interface IConnector
    {
        Socket Connect(IConnectionInfo connectionInfo);

#if FEATURE_TAP
        System.Threading.Tasks.Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken);
#endif
    }
}
