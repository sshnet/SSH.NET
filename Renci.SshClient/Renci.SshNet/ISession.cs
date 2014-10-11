using System;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    internal interface ISession
    {
        ///// <summary>
        ///// Gets or sets the connection info.
        ///// </summary>
        ///// <value>The connection info.</value>
        //IConnectionInfo ConnectionInfo { get; }

        /// <summary>
        /// Create a new SSH session channel.
        /// </summary>
        /// <returns>
        /// A new SSH session channel.
        /// </returns>
        IChannelSession CreateChannelSession();

       /// <summary>
        /// Registers SSH message with the session.
        /// </summary>
        /// <param name="messageName">The name of the message to register with the session.</param>
        void RegisterMessage(string messageName);

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <exception cref="SshConnectionException">The client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">The operation timed out.</exception>
        /// <exception cref="InvalidOperationException">The size of the packet exceeds the maximum size defined by the protocol.</exception>
        void SendMessage(Message message);

        /// <summary>
        /// Unregister SSH message from the session.
        /// </summary>
        /// <param name="messageName">The name of the message to unregister with the session.</param>
        void UnRegisterMessage(string messageName);

        /// <summary>
        /// Occurs when session has been disconnected from the server.
        /// </summary>
        event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        event EventHandler<ExceptionEventArgs> ErrorOccured;

        /// <summary>
        /// Occurs when <see cref="BannerMessage"/> message is received from the server.
        /// </summary>
        event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;
    }
}
