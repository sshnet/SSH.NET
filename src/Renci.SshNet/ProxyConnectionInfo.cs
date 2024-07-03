using Renci.SshNet.Common;
using System;
using System.Text;

namespace Renci.SshNet
{
    /// <summary>
    /// Connection info for connecting to a proxy host
    /// </summary>
    public class ProxyConnectionInfo : IProxyConnectionInfo
    {
        /// <summary>
        /// Gets the character encoding.
        /// </summary>
        /// <value>
        /// The character encoding.
        /// </value>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Proxy Host to connect through.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Proxy Port to connect through
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the username to authenticate this proxy host.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the password to authenticat this proxy host.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets or sets connection timeout.
        /// </summary>
        /// <value>
        /// The connection timeout. The default value is 30 seconds.
        /// </value>
        /// <example>
        ///   <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect Timeout" language="C#" title="Specify connection timeout" />
        /// </example>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Proxy connection Type when this connection should connect through another proxy host.
        /// </summary>
        public ProxyTypes ProxyType { get; private set; }

        /// <summary>
        /// Proxy host to connect through.
        /// </summary>
        public IConnectionInfo ProxyConnection { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="ProxyConnectionInfo"/>.
        /// </summary>
        /// <param name="host">Poxy host to connect through</param>
        /// <param name="port">Proxy port to connect through</param>
        /// <param name="username">Username to use when authenticating the proxy host connection.</param>
        /// <param name="password">Password to use when authenticating the proxy host connection.</param>
        public ProxyConnectionInfo(string host, int port, string username, string password): this(host, port, username, password, ProxyTypes.None, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ProxyConnectionInfo"/>.
        /// </summary>
        /// <param name="host">Poxy host to connect through</param>
        /// <param name="port">Proxy port to connect through</param>
        /// <param name="username">Username to use when authenticating the proxy host connection.</param>
        /// <param name="password">Password to use when authenticating the proxy host connection.</param>
        /// <param name="proxyType">Proxy type of proxy connection used to connect to this proxy host.</param>
        /// <param name="proxyConnection">Connection info to connect to proxy host, through which this proxy host is connected.</param>
        /// <exception cref="ArgumentNullException"><paramref name="proxyType"/> is not <see cref="ProxyTypes.None"/> and <paramref name=" host" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="proxyType"/> is not <see cref="ProxyTypes.None"/> and <paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        public ProxyConnectionInfo(string host, int port, string username, string password, ProxyTypes proxyType, IConnectionInfo proxyConnection)
        {

            if (host == null)
            {
                throw new ArgumentNullException("proxyHost");
            }
            port.ValidatePort("proxyPort");

            Host = host;
            Port = port;
            Username = username;
            Password = password;

            ProxyType = proxyType;
            ProxyConnection = proxyConnection;

            Encoding = Encoding.UTF8;
            Timeout = TimeSpan.FromSeconds(30);
        }
    }
}
