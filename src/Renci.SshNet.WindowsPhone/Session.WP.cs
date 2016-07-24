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

            var disconnectMessage = message as DisconnectMessage;
            if (disconnectMessage != null)
            {
                HandleMessage(disconnectMessage);
                return;
            }

            var serviceRequestMessage = message as ServiceRequestMessage;
            if (serviceRequestMessage != null)
            {
                HandleMessage(serviceRequestMessage);
                return;
            }

            var serviceAcceptMessage = message as ServiceAcceptMessage;
            if (serviceAcceptMessage != null)
            {
                HandleMessage(serviceAcceptMessage);
                return;
            }

            var keyExchangeInitMessage = message as KeyExchangeInitMessage;
            if (keyExchangeInitMessage != null)
            {
                HandleMessage(keyExchangeInitMessage);
                return;
            }

            var newKeysMessage = message as NewKeysMessage;
            if (newKeysMessage != null)
            {
                HandleMessage(newKeysMessage);
                return;
            }

            var requestMessage = message as RequestMessage;
            if (requestMessage != null)
            {
                HandleMessage(requestMessage);
                return;
            }

            var failureMessage = message as FailureMessage;
            if (failureMessage != null)
            {
                HandleMessage(failureMessage);
                return;
            }

            var successMessage = message as SuccessMessage;
            if (successMessage != null)
            {
                HandleMessage(successMessage);
                return;
            }

            var bannerMessage = message as BannerMessage;
            if (bannerMessage != null)
            {
                HandleMessage(bannerMessage);
                return;
            }

            var globalRequestMessage = message as GlobalRequestMessage;
            if (globalRequestMessage != null)
            {
                HandleMessage(globalRequestMessage);
                return;
            }

            var requestSuccessMessage = message as RequestSuccessMessage;
            if (requestSuccessMessage != null)
            {
                HandleMessage(requestSuccessMessage);
                return;
            }

            var requestFailureMessage = message as RequestFailureMessage;
            if (requestFailureMessage != null)
            {
                HandleMessage(requestFailureMessage);
                return;
            }

            var channelOpenMessage = message as ChannelOpenMessage;
            if (channelOpenMessage != null)
            {
                HandleMessage(channelOpenMessage);
                return;
            }

            var channelOpenConfirmationMessage = message as ChannelOpenConfirmationMessage;
            if (channelOpenConfirmationMessage != null)
            {
                HandleMessage(channelOpenConfirmationMessage);
                return;
            }

            var channelOpenFailureMessage = message as ChannelOpenFailureMessage;
            if (channelOpenFailureMessage != null)
            {
                HandleMessage(channelOpenFailureMessage);
                return;
            }

            var channelWindowAdjustMessage = message as ChannelWindowAdjustMessage;
            if (channelWindowAdjustMessage != null)
            {
                HandleMessage(channelWindowAdjustMessage);
                return;
            }

            var channelDataMessage = message as ChannelDataMessage;
            if (channelDataMessage != null)
            {
                HandleMessage(channelDataMessage);
                return;
            }

            var channelExtendedDataMessage = message as ChannelExtendedDataMessage;
            if (channelExtendedDataMessage != null)
            {
                HandleMessage(channelExtendedDataMessage);
                return;
            }

            var channelEofMessage = message as ChannelEofMessage;
            if (channelEofMessage != null)
            {
                HandleMessage(channelEofMessage);
                return;
            }

            var channelCloseMessage = message as ChannelCloseMessage;
            if (channelCloseMessage != null)
            {
                HandleMessage(channelCloseMessage);
                return;
            }

            var channelRequestMessage = message as ChannelRequestMessage;
            if (channelRequestMessage != null)
            {
                HandleMessage(channelRequestMessage);
                return;
            }

            var channelSuccessMessage = message as ChannelSuccessMessage;
            if (channelSuccessMessage != null)
            {
                HandleMessage(channelSuccessMessage);
                return;
            }

            var channelFailureMessage = message as ChannelFailureMessage;
            if (channelFailureMessage != null)
            {
                HandleMessage(channelFailureMessage);
                return;
            }

            var ignoreMessage = message as IgnoreMessage;
            if (ignoreMessage != null)
            {
                HandleMessage(ignoreMessage);
                return;
            }

            var unimplementedMessage = message as UnimplementedMessage;
            if (unimplementedMessage != null)
            {
                HandleMessage(unimplementedMessage);
                return;
            }

            var debugMessage = message as DebugMessage;
            if (debugMessage != null)
            {
                HandleMessage(debugMessage);
                return;
            }

            Debug.WriteLine(
                "SSH.NET WARNING: unknown message type {0} - may need to add new type to Session.WP.cs, HandleMessageCore method",
                message.GetType().FullName);

            HandleMessage(message);
        }
    }
}
