using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Handles the SSH protocol version exchange.
    /// </summary>
    internal interface IProtocolVersionExchange
    {
        /// <summary>
        /// Performs the SSH protocol version exchange.
        /// </summary>
        /// <param name="clientVersion">The identification string of the SSH client.</param>
        /// <param name="socket">A <see cref="Socket"/> connected to the server.</param>
        /// <param name="timeout">The maximum time to wait for the server to respond.</param>
        /// <returns>
        /// The SSH identification of the server.
        /// </returns>
        SshIdentification Start(string clientVersion, Socket socket, TimeSpan timeout);

        /// <summary>
        /// Asynchronously performs the SSH protocol version exchange.
        /// </summary>
        /// <param name="clientVersion">The identification string of the SSH client.</param>
        /// <param name="socket">A <see cref="Socket"/> connected to the server.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the SSH protocol version exchange. The value of its
        /// <see cref="Task{Task}.Result"/> contains the SSH identification of the server.
        /// </returns>
        Task<SshIdentification> StartAsync(string clientVersion, Socket socket, CancellationToken cancellationToken);
    }
}
