using System;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using System.Diagnostics;

namespace Renci.SshNet
{
    public partial class Session
    {
        partial void HandleMessageCore(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            else if (message is DisconnectMessage)
                this.HandleMessage((DisconnectMessage)message);
            else if (message is IgnoreMessage)
                this.HandleMessage((IgnoreMessage)message);
            else if (message is UnimplementedMessage)
                this.HandleMessage((UnimplementedMessage)message);
            else if (message is DebugMessage)
                this.HandleMessage((DebugMessage)message);
            else if (message is ServiceRequestMessage)
                this.HandleMessage((ServiceRequestMessage)message);
            else if (message is ServiceAcceptMessage)
                this.HandleMessage((ServiceAcceptMessage)message);
            else if (message is KeyExchangeInitMessage)
                this.HandleMessage((KeyExchangeInitMessage)message);
            else if (message is NewKeysMessage)
                this.HandleMessage((NewKeysMessage)message);
            else if (message is RequestMessage)
                this.HandleMessage((RequestMessage)message);
            else if (message is FailureMessage)
                this.HandleMessage((FailureMessage)message);
            else if (message is SuccessMessage)
                this.HandleMessage((SuccessMessage)message);
            else if (message is BannerMessage)
                this.HandleMessage((BannerMessage)message);
            else if (message is GlobalRequestMessage)
                this.HandleMessage((GlobalRequestMessage)message);
            else if (message is RequestSuccessMessage)
                this.HandleMessage((RequestSuccessMessage)message);
            else if (message is RequestFailureMessage)
                this.HandleMessage((RequestFailureMessage)message);
            else if (message is ChannelOpenMessage)
                this.HandleMessage((ChannelOpenMessage)message);
            else if (message is ChannelOpenConfirmationMessage)
                this.HandleMessage((ChannelOpenConfirmationMessage)message);
            else if (message is ChannelOpenFailureMessage)
                this.HandleMessage((ChannelOpenFailureMessage)message);
            else if (message is ChannelWindowAdjustMessage)
                this.HandleMessage((ChannelWindowAdjustMessage)message);
            else if (message is ChannelDataMessage)
                this.HandleMessage((ChannelDataMessage)message);
            else if (message is ChannelExtendedDataMessage)
                this.HandleMessage((ChannelExtendedDataMessage)message);
            else if (message is ChannelEofMessage)
                this.HandleMessage((ChannelEofMessage)message);
            else if (message is ChannelCloseMessage)
                this.HandleMessage((ChannelCloseMessage)message);
            else if (message is ChannelRequestMessage)
                this.HandleMessage((ChannelRequestMessage)message);
            else if (message is ChannelSuccessMessage)
                this.HandleMessage((ChannelSuccessMessage)message);
            else if (message is ChannelFailureMessage)
                this.HandleMessage((ChannelFailureMessage)message);
            else
            {
                Debug.WriteLine("SSH.NET WARNING: unknown message type {0} - may need to add new type to Session.WP.cs, HandleMessageCore method",
                    message.GetType().FullName);

                this.HandleMessage(message);
            }
        }
    }
}
