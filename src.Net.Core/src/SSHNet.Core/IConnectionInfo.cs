using System;
using System.Collections.Generic;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    internal interface IConnectionInfoInternal : IConnectionInfo
    {
        /// <summary>
        /// Signals that an authentication banner message was received from the server.
        /// </summary>
        /// <param name="sender">The session in which the banner message was received.</param>
        /// <param name="e">The banner message.{</param>
        void UserAuthenticationBannerReceived(object sender, MessageEventArgs<BannerMessage> e);

        /// <summary>
        /// Gets the supported authentication methods for this connection.
        /// </summary>
        /// <value>
        /// The supported authentication methods for this connection.
        /// </value>
        IEnumerable<IAuthenticationMethod> AuthenticationMethods { get; }

        /// <summary>
        /// Creates a <see cref="NoneAuthenticationMethod"/> for the credentials represented
        /// by the current <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NoneAuthenticationMethod"/> for the credentials represented by the
        /// current <see cref="IConnectionInfo"/>.
        /// </returns>
        IAuthenticationMethod CreateNoneAuthenticationMethod();
    }

    /// <summary>
    /// Represents remote connection information.
    /// </summary>
    internal interface IConnectionInfo
    {
        /// <summary>
        /// Gets the supported channel requests for this connection.
        /// </summary>
        /// <value>
        /// The supported channel requests for this connection.
        /// </value>
        IDictionary<string, RequestInfo> ChannelRequests { get; }

        /// <summary>
        /// Gets the character encoding.
        /// </summary>
        /// <value>
        /// The character encoding.
        /// </value>
        Encoding Encoding { get; }

        /// <summary>
        /// Gets the number of retry attempts when session channel creation failed.
        /// </summary>
        /// <value>
        /// The number of retry attempts when session channel creation failed.
        /// </value>
        int RetryAttempts { get; }

        /// <summary>
        /// Gets or sets connection timeout.
        /// </summary>
        /// <value>
        /// The connection timeout. The default value is 30 seconds.
        /// </value>
        /// <example>
        ///   <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect Timeout" language="C#" title="Specify connection timeout" />
        /// </example>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Occurs when authentication banner is sent by the server.
        /// </summary>
        event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;
    }
}
