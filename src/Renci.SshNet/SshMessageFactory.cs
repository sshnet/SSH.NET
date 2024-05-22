﻿using System;
using System.Collections.Generic;
using System.Globalization;

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    internal sealed class SshMessageFactory
    {
        private readonly MessageMetadata[] _enabledMessagesByNumber;
        private readonly bool[] _activatedMessagesById;
        private readonly object _lock = new object();

        private static readonly MessageMetadata[] AllMessages = new MessageMetadata[]
            {
                new MessageMetadata<KeyExchangeInitMessage>(0, "SSH_MSG_KEXINIT", 20),
                new MessageMetadata<NewKeysMessage>(1, "SSH_MSG_NEWKEYS", 21),
                new MessageMetadata<RequestFailureMessage>(2, "SSH_MSG_REQUEST_FAILURE", 82),
                new MessageMetadata<ChannelOpenFailureMessage>(3, "SSH_MSG_CHANNEL_OPEN_FAILURE", 92),
                new MessageMetadata<ChannelFailureMessage>(4, "SSH_MSG_CHANNEL_FAILURE", 100),
                new MessageMetadata<ChannelExtendedDataMessage>(5, "SSH_MSG_CHANNEL_EXTENDED_DATA", 95),
                new MessageMetadata<ChannelDataMessage>(6, "SSH_MSG_CHANNEL_DATA", 94),
                new MessageMetadata<ChannelRequestMessage>(7, "SSH_MSG_CHANNEL_REQUEST", 98),
                new MessageMetadata<BannerMessage>(8, "SSH_MSG_USERAUTH_BANNER", 53),
                new MessageMetadata<InformationResponseMessage>(9, "SSH_MSG_USERAUTH_INFO_RESPONSE", 61),
                new MessageMetadata<FailureMessage>(10, "SSH_MSG_USERAUTH_FAILURE", 51),
                new MessageMetadata<DebugMessage>(11, "SSH_MSG_DEBUG", 4),
                new MessageMetadata<GlobalRequestMessage>(12, "SSH_MSG_GLOBAL_REQUEST", 80),
                new MessageMetadata<ChannelOpenMessage>(13, "SSH_MSG_CHANNEL_OPEN", 90),
                new MessageMetadata<ChannelOpenConfirmationMessage>(14, "SSH_MSG_CHANNEL_OPEN_CONFIRMATION", 91),
                new MessageMetadata<InformationRequestMessage>(15, "SSH_MSG_USERAUTH_INFO_REQUEST", 60),
                new MessageMetadata<UnimplementedMessage>(16, "SSH_MSG_UNIMPLEMENTED", 3),
                new MessageMetadata<RequestSuccessMessage>(17, "SSH_MSG_REQUEST_SUCCESS", 81),
                new MessageMetadata<ChannelSuccessMessage>(18, "SSH_MSG_CHANNEL_SUCCESS", 99),
                new MessageMetadata<PasswordChangeRequiredMessage>(19, "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", 60),
                new MessageMetadata<DisconnectMessage>(20, "SSH_MSG_DISCONNECT", 1),
                new MessageMetadata<SuccessMessage>(21, "SSH_MSG_USERAUTH_SUCCESS", 52),
                new MessageMetadata<PublicKeyMessage>(22, "SSH_MSG_USERAUTH_PK_OK", 60),
                new MessageMetadata<IgnoreMessage>(23, "SSH_MSG_IGNORE", 2),
                new MessageMetadata<ChannelWindowAdjustMessage>(24, "SSH_MSG_CHANNEL_WINDOW_ADJUST", 93),
                new MessageMetadata<ChannelEofMessage>(25, "SSH_MSG_CHANNEL_EOF", 96),
                new MessageMetadata<ChannelCloseMessage>(26, "SSH_MSG_CHANNEL_CLOSE", 97),
                new MessageMetadata<ServiceAcceptMessage>(27, "SSH_MSG_SERVICE_ACCEPT", 6),
                new MessageMetadata<KeyExchangeDhGroupExchangeGroup>(28, "SSH_MSG_KEX_DH_GEX_GROUP", 31),
                new MessageMetadata<KeyExchangeDhReplyMessage>(29, "SSH_MSG_KEXDH_REPLY", 31),
                new MessageMetadata<KeyExchangeDhGroupExchangeReply>(30, "SSH_MSG_KEX_DH_GEX_REPLY", 33),
                new MessageMetadata<KeyExchangeEcdhReplyMessage>(31, "SSH_MSG_KEX_ECDH_REPLY", 31)
            };
        private static readonly Dictionary<string, MessageMetadata> MessagesByName = CreateMessagesByNameMapping();

        /// <summary>
        /// Defines the highest message number that is currently supported.
        /// </summary>
        internal const byte HighestMessageNumber = 100;

        /// <summary>
        /// Defines the total number of supported messages.
        /// </summary>
        internal const int TotalMessageCount = 32;

        /// <summary>
        /// Initializes a new instance of the <see cref="SshMessageFactory"/> class.
        /// </summary>
        public SshMessageFactory()
        {
            _activatedMessagesById = new bool[TotalMessageCount];
            _enabledMessagesByNumber = new MessageMetadata[HighestMessageNumber + 1];
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
            if (messageNumber > HighestMessageNumber)
            {
                throw CreateMessageTypeNotSupportedException(messageNumber);
            }

            var enabledMessageMetadata = _enabledMessagesByNumber[messageNumber];
            if (enabledMessageMetadata is null)
            {
                MessageMetadata definedMessageMetadata = null;

                // find first message with specified number
                for (var i = 0; i < AllMessages.Length; i++)
                {
                    var messageMetadata = AllMessages[i];
                    if (messageMetadata.Number == messageNumber)
                    {
                        definedMessageMetadata = messageMetadata;
                        break;
                    }
                }

                if (definedMessageMetadata is null)
                {
                    throw CreateMessageTypeNotSupportedException(messageNumber);
                }

                throw new SshException(string.Format(CultureInfo.InvariantCulture, "Message type {0} is not valid in the current context.", messageNumber));
            }

            return enabledMessageMetadata.Create();
        }

        /// <summary>
        /// Disables non-KeyExchange messages.
        /// </summary>
        /// <param name="strict">
        /// <see langword="true"/> to indicate the strict key exchange mode; otherwise <see langword="false"/>.
        /// <para>In strict key exchange mode, only below messages are allowed:</para>
        /// <list type="bullet">
        /// <item>SSH_MSG_KEXINIT -> 20</item>
        /// <item>SSH_MSG_NEWKEYS -> 21</item>
        /// <item>SSH_MSG_DISCONNECT -> 1</item>
        /// </list>
        /// <para>Note:</para>
        /// <para>  The relevant KEX Reply MSG will be allowed from a sub class of KeyExchange class.</para>
        /// <para>  For example, it calls <c>Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");</c> if the curve25519-sha256 KEX algorithm is selected per negotiation.</para>
        /// </param>
        public void DisableNonKeyExchangeMessages(bool strict)
        {
            for (var i = 0; i < AllMessages.Length; i++)
            {
                var messageMetadata = AllMessages[i];

                var messageNumber = messageMetadata.Number;
                if (strict)
                {
                    if (messageNumber is not 20 and not 21 and not 1)
                    {
                        _enabledMessagesByNumber[messageNumber] = null;
                    }
                }
                else
                {
                    if (messageNumber is (> 2 and < 20) or > 30)
                    {
                        _enabledMessagesByNumber[messageNumber] = null;
                    }
                }
            }
        }

        public void EnableActivatedMessages()
        {
            for (var i = 0; i < AllMessages.Length; i++)
            {
                var messageMetadata = AllMessages[i];

                if (!_activatedMessagesById[messageMetadata.Id])
                {
                    continue;
                }

                var enabledMessageMetadata = _enabledMessagesByNumber[messageMetadata.Number];
                if (enabledMessageMetadata != null && enabledMessageMetadata != messageMetadata)
                {
                    throw CreateMessageTypeAlreadyEnabledForOtherMessageException(messageMetadata.Number,
                                                                                  messageMetadata.Name,
                                                                                  enabledMessageMetadata.Name);
                }

                _enabledMessagesByNumber[messageMetadata.Number] = messageMetadata;
            }
        }

        public void EnableAndActivateMessage(string messageName)
        {
            if (messageName is null)
            {
                throw new ArgumentNullException(nameof(messageName));
            }

            lock (_lock)
            {
                if (!MessagesByName.TryGetValue(messageName, out var messageMetadata))
                {
                    throw CreateMessageNotSupportedException(messageName);
                }

                var enabledMessageMetadata = _enabledMessagesByNumber[messageMetadata.Number];
                if (enabledMessageMetadata != null && enabledMessageMetadata != messageMetadata)
                {
                    throw CreateMessageTypeAlreadyEnabledForOtherMessageException(messageMetadata.Number,
                        messageMetadata.Name,
                        enabledMessageMetadata.Name);
                }

                _enabledMessagesByNumber[messageMetadata.Number] = messageMetadata;
                _activatedMessagesById[messageMetadata.Id] = true;
            }
        }

        public void DisableAndDeactivateMessage(string messageName)
        {
            if (messageName is null)
            {
                throw new ArgumentNullException(nameof(messageName));
            }

            lock (_lock)
            {
                if (!MessagesByName.TryGetValue(messageName, out var messageMetadata))
                {
                    throw CreateMessageNotSupportedException(messageName);
                }

                var enabledMessageMetadata = _enabledMessagesByNumber[messageMetadata.Number];
                if (enabledMessageMetadata != null && enabledMessageMetadata != messageMetadata)
                {
                    throw CreateMessageTypeAlreadyEnabledForOtherMessageException(messageMetadata.Number,
                                                                                  messageMetadata.Name,
                                                                                  enabledMessageMetadata.Name);
                }

                _activatedMessagesById[messageMetadata.Id] = false;
                _enabledMessagesByNumber[messageMetadata.Number] = null;
            }
        }

        private static Dictionary<string, MessageMetadata> CreateMessagesByNameMapping()
        {
            var messagesByName = new Dictionary<string, MessageMetadata>(AllMessages.Length);

            for (var i = 0; i < AllMessages.Length; i++)
            {
                var messageMetadata = AllMessages[i];
                messagesByName.Add(messageMetadata.Name, messageMetadata);
            }

            return messagesByName;
        }

        private static SshException CreateMessageTypeNotSupportedException(byte messageNumber)
        {
            throw new SshException(string.Format(CultureInfo.InvariantCulture, "Message type {0} is not supported.", messageNumber));
        }

        private static SshException CreateMessageNotSupportedException(string messageName)
        {
            throw new SshException(string.Format(CultureInfo.InvariantCulture, "Message '{0}' is not supported.", messageName));
        }

        private static SshException CreateMessageTypeAlreadyEnabledForOtherMessageException(byte messageNumber, string messageName, string currentEnabledForMessageName)
        {
            throw new SshException(string.Format(CultureInfo.InvariantCulture,
                                                 "Cannot enable message '{0}'. Message type {1} is already enabled for '{2}'.",
                                                 messageName,
                                                 messageNumber,
                                                 currentEnabledForMessageName));
        }

        private abstract class MessageMetadata
        {
            protected MessageMetadata(byte id, string name, byte number)
            {
                Id = id;
                Name = name;
                Number = number;
            }

            public byte Id { get; }

            public string Name { get; }

            public byte Number { get; }

            public abstract Message Create();
        }

        private sealed class MessageMetadata<T> : MessageMetadata
            where T : Message, new()
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
