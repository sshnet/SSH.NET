using System;
using System.Collections.Generic;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents remote connection information.
    /// </summary>
    public interface IConnectionInfo
    {
        /// <summary>
        /// Gets the character encoding.
        /// </summary>
        /// <value>
        /// The character encoding.
        /// </value>
        Encoding Encoding { get; }

        /// <summary>
        /// Gets connection host.
        /// </summary>
        /// <value>
        /// The connection host.
        /// </value>
        string Host { get; }

        /// <summary>
        /// Gets connection port.
        /// </summary>
        /// <value>
        /// The connection port. The default value is 22.
        /// </value>
        int Port { get; }

        /// <summary>
        /// Gets proxy type.
        /// </summary>
        /// <value>
        /// The type of the proxy.
        /// </value>
        ProxyTypes ProxyType { get; }

        /// <summary>
        /// Gets the connection info to connect to the proxy.
        /// </summary>
        IConnectionInfo ProxyConnection { get; }

        /// <summary>
        /// Gets the connection timeout.
        /// </summary>
        /// <value>
        /// The connection timeout. The default value is 30 seconds.
        /// </value>
        TimeSpan Timeout { get; }
    }

    /// <summary>
    /// Represents remote SSH connection information.
    /// </summary>
    internal interface ISshConnectionInfo : IConnectionInfo
    {
        /// <summary>
        /// Gets the timeout to used when waiting for a server to acknowledge closing a channel.
        /// </summary>
        /// <value>
        /// The channel close timeout. The default value is 1 second.
        /// </value>
        /// <remarks>
        /// If a server does not send a <c>SSH2_MSG_CHANNEL_CLOSE</c> message before the specified timeout
        /// elapses, the channel will be closed immediately.
        /// </remarks>
        TimeSpan ChannelCloseTimeout { get; }

        /// <summary>
        /// Gets the supported channel requests for this connection.
        /// </summary>
        /// <value>
        /// The supported channel requests for this connection.
        /// </value>
        IDictionary<string, RequestInfo> ChannelRequests { get; }

        /// <summary>
        /// Gets the number of retry attempts when session channel creation failed.
        /// </summary>
        /// <value>
        /// The number of retry attempts when session channel creation failed.
        /// </value>
        int RetryAttempts { get; }

        /// <summary>
        /// Occurs when authentication banner is sent by the server.
        /// </summary>
        event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;
    }

    /// <summary>
    /// Represents proxy connection information (HTTP, SOCKS4, SOCKS5).
    /// </summary>
    internal interface IProxyConnectionInfo : IConnectionInfo
    {
        /// <summary>
        /// Gets the username to authenticate this proxy host.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password to authenticat this proxy host.
        /// </summary>
        string Password { get; }
    }
}
