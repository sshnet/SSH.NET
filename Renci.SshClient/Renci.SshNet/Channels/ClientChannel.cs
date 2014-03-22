using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    internal abstract class ClientChannel : Channel
    {
        /// <summary>
        /// Occurs when <see cref="ChannelOpenConfirmationMessage"/> message is received.
        /// </summary>
        public event EventHandler<ChannelOpenConfirmedEventArgs> OpenConfirmed;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenFailureMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelOpenFailedEventArgs> OpenFailed;

        /// <summary>
        /// Initializes the channel.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        internal override void Initialize(Session session, uint localWindowSize, uint localPacketSize)
        {
            base.Initialize(session, localWindowSize, localPacketSize);
            Session.ChannelOpenConfirmationReceived += OnChannelOpenConfirmation;
            Session.ChannelOpenFailureReceived += OnChannelOpenFailure;
        }

        /// <summary>
        /// Called when channel is opened by the server.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected virtual void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            InitializeRemoteInfo(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            //  Channel is consider to be open when confirmation message was received
            this.IsOpen = true;

            var openConfirmed = OpenConfirmed;
            if (openConfirmed != null)
                openConfirmed(this, new ChannelOpenConfirmedEventArgs(remoteChannelNumber, initialWindowSize, maximumPacketSize));
        }

        /// <summary>
        /// Send message to open a channel.
        /// </summary>
        /// <param name="message">Message to send</param>
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
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnOpenConfirmation(e.Message.RemoteChannelNumber, e.Message.InitialWindowSize, e.Message.MaximumPacketSize);
            }
        }

        private void OnChannelOpenFailure(object sender, MessageEventArgs<ChannelOpenFailureMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnOpenFailure(e.Message.ReasonCode, e.Message.Description, e.Message.Language);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var session = Session;
                if (session != null)
                {
                    session.ChannelOpenConfirmationReceived -= OnChannelOpenConfirmation;
                    session.ChannelOpenFailureReceived -= OnChannelOpenFailure;
                }
            }
        }
    }
}
