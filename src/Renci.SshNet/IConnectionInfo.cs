using System;
using System.Collections.Generic;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    internal interface IConnectionInfoInternal : ISshConnectionInfo
    {
        /// <summary>
        /// Signals that an authentication banner message was received from the server.
        /// </summary>
        /// <param name="sender">The session in which the banner message was received.</param>
        /// <param name="e">The banner message.</param>
        void UserAuthenticationBannerReceived(object sender, MessageEventArgs<BannerMessage> e);

        /// <summary>
        /// Gets the supported authentication methods for this connection.
        /// </summary>
        /// <value>
        /// The supported authentication methods for this connection.
        /// </value>
        IList<IAuthenticationMethod> AuthenticationMethods { get; }

        /// <summary>
        /// Creates a <see cref="NoneAuthenticationMethod"/> for the credentials represented
        /// by the current <see cref="ISshConnectionInfo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NoneAuthenticationMethod"/> for the credentials represented by the
        /// current <see cref="ISshConnectionInfo"/>.
        /// </returns>
        IAuthenticationMethod CreateNoneAuthenticationMethod();
    }

    /// <summary>
    /// Represents remote SSH connection information.
    /// </summary>
    internal interface ISshConnectionInfo: IConnectionInfo
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
    /// Represents proxy connection information (HTTP, SOCKS4, SOCKS5)
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
        /// <example>
        ///   <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect Timeout" language="C#" title="Specify connection timeout" />
        /// </example>
        TimeSpan Timeout { get; }

    }
}
