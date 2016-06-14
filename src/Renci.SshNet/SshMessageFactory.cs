using System;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    internal class SshMessageFactory
    {
        private readonly MessageMetadata[] _enabledMessagesByNumber;
        private readonly bool[] _activatedMessagesById;

        private static readonly MessageMetadata[] AllMessages;
        private static readonly IDictionary<string, MessageMetadata> MessagesByName;

        static SshMessageFactory()
        {
            AllMessages = new MessageMetadata[]
            {
                new MessageMetadata<KeyExchangeInitMessage>(0, "SSH_MSG_KEXINIT", 20),
                new MessageMetadata<NewKeysMessage> (1, "SSH_MSG_NEWKEYS", 21),
                new MessageMetadata<RequestFailureMessage> (2, "SSH_MSG_REQUEST_FAILURE", 82),
                new MessageMetadata<ChannelOpenFailureMessage> (3, "SSH_MSG_CHANNEL_OPEN_FAILURE", 92),
                new MessageMetadata<ChannelFailureMessage> (4, "SSH_MSG_CHANNEL_FAILURE", 100),
                new MessageMetadata<ChannelExtendedDataMessage> (5, "SSH_MSG_CHANNEL_EXTENDED_DATA", 95),
                new MessageMetadata<ChannelDataMessage> (6, "SSH_MSG_CHANNEL_DATA", 94),
                new MessageMetadata<ChannelRequestMessage> (7, "SSH_MSG_CHANNEL_REQUEST", 98),
                new MessageMetadata<BannerMessage> (8, "SSH_MSG_USERAUTH_BANNER", 53),
                new MessageMetadata<InformationResponseMessage> (9, "SSH_MSG_USERAUTH_INFO_RESPONSE", 61),
                new MessageMetadata<FailureMessage> (10, "SSH_MSG_USERAUTH_FAILURE", 51),
                new MessageMetadata<DebugMessage> (11, "SSH_MSG_DEBUG", 4),
                new MessageMetadata<GlobalRequestMessage> (12, "SSH_MSG_GLOBAL_REQUEST", 80),
                new MessageMetadata<ChannelOpenMessage> (13, "SSH_MSG_CHANNEL_OPEN", 90),
                new MessageMetadata<ChannelOpenConfirmationMessage> (14, "SSH_MSG_CHANNEL_OPEN_CONFIRMATION", 91),
                new MessageMetadata<InformationRequestMessage> (15, "SSH_MSG_USERAUTH_INFO_REQUEST", 60),
                new MessageMetadata<UnimplementedMessage> (16, "SSH_MSG_UNIMPLEMENTED", 3),
                new MessageMetadata<RequestSuccessMessage> (17, "SSH_MSG_REQUEST_SUCCESS", 81),
                new MessageMetadata<ChannelSuccessMessage> (18, "SSH_MSG_CHANNEL_SUCCESS", 99),
                new MessageMetadata<PasswordChangeRequiredMessage> (19, "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", 60),
                new MessageMetadata<DisconnectMessage> (20, "SSH_MSG_DISCONNECT", 1),
                new MessageMetadata<SuccessMessage> (21, "SSH_MSG_USERAUTH_SUCCESS", 52),
                new MessageMetadata<PublicKeyMessage> (22, "SSH_MSG_USERAUTH_PK_OK", 60),
                new MessageMetadata<IgnoreMessage> (23, "SSH_MSG_IGNORE", 2),
                new MessageMetadata<ChannelWindowAdjustMessage> (24, "SSH_MSG_CHANNEL_WINDOW_ADJUST", 93),
                new MessageMetadata<ChannelEofMessage> (25, "SSH_MSG_CHANNEL_EOF", 96),
                new MessageMetadata<ChannelCloseMessage> (26, "SSH_MSG_CHANNEL_CLOSE", 97),
                new MessageMetadata<ServiceAcceptMessage> (27, "SSH_MSG_SERVICE_ACCEPT", 6),
                new MessageMetadata<KeyExchangeDhGroupExchangeGroup> (28, "SSH_MSG_KEX_DH_GEX_GROUP", 31),
                new MessageMetadata<KeyExchangeDhReplyMessage> (29, "SSH_MSG_KEXDH_REPLY", 31),
                new MessageMetadata<KeyExchangeDhGroupExchangeReply> (30, "SSH_MSG_KEX_DH_GEX_REPLY", 33)
            };

            MessagesByName = new Dictionary<string, MessageMetadata>(31);
            foreach (var messageMetadata in AllMessages)
                MessagesByName.Add(messageMetadata.Name, messageMetadata);
        }

        public SshMessageFactory()
        {
            _activatedMessagesById = new bool[31];
            _enabledMessagesByNumber = new MessageMetadata[101];
        }

        /// <summary>
        /// Disables and deactivate all messages.
        /// </summary>
        public void Reset()
        {
            Array.Clear(_activatedMessagesById, 0, _activatedMessagesById.Length);
            Array.Clear(_enabledMessagesByNumber, 0, _enabledMessagesByNumber.Length);
        }

        public Message Create(byte messageNumber)
        {
            var messageMetadata = _enabledMessagesByNumber[messageNumber];

            if (messageMetadata == null)
            {
                throw new SshException(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid.", messageNumber));
            }

            return messageMetadata.Create();
        }

        public void DisableNonKeyExchangeMessages()
        {
            foreach (var messageMetadata in AllMessages)
            {
                var messageNumber = messageMetadata.Number;
                if ((messageNumber > 2 && messageNumber < 20) || messageNumber > 30)
                {
                    //Console.WriteLine("Disabling " + messageMetadata.Name + "...");
                    _enabledMessagesByNumber[messageNumber] = null;
                }
            }
        }

        public void EnableActivatedMessages()
        {
            foreach (var messageMetadata in AllMessages)
            {
                if (!_activatedMessagesById[messageMetadata.Id])
                    continue;

                var enabledMessage = _enabledMessagesByNumber[messageMetadata.Number];
                if (enabledMessage != null && enabledMessage != messageMetadata)
                {
                    throw new Exception("Message X is already enabled for Y");
                }
                _enabledMessagesByNumber[messageMetadata.Number] = messageMetadata;
            }
        }

        public void EnableAndActivateMessage(string messageName)
        {
            lock (this)
            {
                MessageMetadata messageMetadata;

                if (!MessagesByName.TryGetValue(messageName, out messageMetadata))
                {
                    throw new Exception("TODO");
                }

                var enabledMessage = _enabledMessagesByNumber[messageMetadata.Number];
                if (enabledMessage != null && enabledMessage != messageMetadata)
                {
                    throw new Exception("Message X is already enabled for Y");
                }
                _enabledMessagesByNumber[messageMetadata.Number] = messageMetadata;

                _activatedMessagesById[messageMetadata.Id] = true;
            }
        }

        public void DisableAndDeactivateMessage(string messageName)
        {
            lock (this)
            {
                MessageMetadata messageMetadata;

                if (!MessagesByName.TryGetValue(messageName, out messageMetadata))
                {
                    throw new Exception("TODO");
                }

                _activatedMessagesById[messageMetadata.Id] = false;

                var enabledMetadata = _enabledMessagesByNumber[messageMetadata.Number];
                if (enabledMetadata != null && enabledMetadata != messageMetadata)
                    throw new Exception();
                _enabledMessagesByNumber[messageMetadata.Number] = null;
            }
        }

        private abstract class MessageMetadata
        {
            protected MessageMetadata(byte id, string name, byte number)
            {
                Id = id;
                Name = name;
                Number = number;
            }

            public readonly byte Id;

            public readonly string Name;

            public readonly byte Number;

            public abstract Message Create();
        }

        private class MessageMetadata<T> : MessageMetadata where T : Message, new()
        {
            public MessageMetadata(byte id, string name, byte number)
                : base(id, name, number)
            {
            }

            public override Message Create()
            {
                return new T();
            }
        }
    }
}
