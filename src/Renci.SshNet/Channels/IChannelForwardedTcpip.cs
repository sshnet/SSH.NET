using System;
using System.Net;
using Renci.SshNet.Common;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// A "forwarded-tcpip" SSH channel.
    /// </summary>
    internal interface IChannelForwardedTcpip : IDisposable
    {
        /// <summary>
        /// Occurs when an exception is thrown while processing channel messages.
        /// </summary>
        event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Binds the channel to the specified endpoint.
        /// </summary>
        /// <param name="remoteEndpoint">The endpoint to connect to.</param>
        /// <param name="forwardedPort">The forwarded port for which the channel is opened.</param>
        void Bind(IPEndPoint remoteEndpoint, IForwardedPort forwardedPort);
    }
}
