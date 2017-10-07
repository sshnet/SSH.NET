using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    internal abstract class ClientChannel : Channel
    {
        /// <summary>
        /// Initializes a new <see cref="ClientChannel"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        protected ClientChannel(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
        {
            session.ChannelOpenConfirmationReceived += OnChannelOpenConfirmation;
            session.ChannelOpenFailureReceived += OnChannelOpenFailure;
        }

        /// <summary>
        /// Occurs when <see cref="ChannelOpenConfirmationMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelOpenConfirmedEventArgs> OpenConfirmed;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenFailureMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelOpenFailedEventArgs> OpenFailed;

        /// <summary>
        /// Called when channel is opened by the server.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected virtual void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            InitializeRemoteInfo(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            // Channel is consider to be open when confirmation message was received
            IsOpen = true;

            var openConfirmed = OpenConfirmed;
            if (openConfirmed != null)
                openConfirmed(this, new ChannelOpenConfirmedEventArgs(remoteChannelNumber, initialWindowSize, maximumPacketSize));
        }

        /// <summary>
        /// Send message to open a channel.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <exception cref="SshConnectionException">The client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">The operation timed out.</exception>
        /// <exception cref="InvalidOperationException">The size of the packet exceeds the maximum size defined by the protocol.</exception>
        protected void SendMessage(ChannelOpenMessage message)
        {
            Session.SendMessage(message);
        }

        /// <summary>
        /// Called when channel failed to open.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="description">The description.</param>
        /// <param name="language">The language.</param>
        protected virtual void OnOpenFailure(uint reasonCode, string description, string language)
        {
            var openFailed = OpenFailed;
            if (openFailed != null)
                openFailed(this, new ChannelOpenFailedEventArgs(LocalChannelNumber, reasonCode, description, language));
        }

        private void OnChannelOpenConfirmation(object sender, MessageEventArgs<ChannelOpenConfirmationMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnOpenConfirmation(e.Message.RemoteChannelNumber, e.Message.InitialWindowSize,
                        e.Message.MaximumPacketSize);
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelOpenFailure(object sender, MessageEventArgs<ChannelOpenFailureMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnOpenFailure(e.Message.ReasonCode, e.Message.Description, e.Message.Language);
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            UnsubscribeFromSessionEvents(Session);

            base.Dispose(disposing);
        }

        /// <summary>
        /// Unsubscribes the current <see cref="ClientChannel"/> from session events.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <remarks>
        /// Does nothing when <paramref name="session"/> is <c>null</c>.
        /// </remarks>
        private void UnsubscribeFromSessionEvents(ISession session)
        {
            if (session == null)
                return;

            session.ChannelOpenConfirmationReceived -= OnChannelOpenConfirmation;
            session.ChannelOpenFailureReceived -= OnChannelOpenFailure;
        }
    }
}
