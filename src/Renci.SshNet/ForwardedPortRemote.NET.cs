using System.Net;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    public partial class ForwardedPortRemote
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortRemote"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\ForwardedPortRemoteTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortRemote" language="C#" title="Remote port forwarding" />
        /// </example>
        public ForwardedPortRemote(uint boundPort, string host, uint port)
            : this(string.Empty, boundPort, host, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortRemote"/> class.
        /// </summary>
        /// <param name="boundHost">The bound host.</param>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public ForwardedPortRemote(string boundHost, uint boundPort, string host, uint port)
            : this(Dns.GetHostEntry(boundHost).AddressList[0], boundPort, Dns.GetHostEntry(host).AddressList[0], port)
        {
        }
    }
}
