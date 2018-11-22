﻿using System;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group1-sha1" algorithm implementation.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellmanGroupSha256 : KeyExchangeDiffieHellman
    {
        /// <summary>
        /// Gets the group prime.
        /// </summary>
        /// <value>
        /// The group prime.
        /// </value>
        public abstract BigInteger GroupPrime { get; }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected override int HashSize
        {
            get { return 256; }
        }

        /// <summary>
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>
        /// Key exchange hash.
        /// </returns>
        protected override byte[] CalculateHash()
        {
            var hashData = new KeyExchangeDiffieHellmanGroupSha1._ExchangeHashData
            {
                ClientVersion = Session.ClientVersion,
                ServerVersion = Session.ServerVersion,
                ClientPayload = _clientPayload,
                ServerPayload = _serverPayload,
                HostKey = _hostKey,
                ClientExchangeValue = _clientExchangeValue,
                ServerExchangeValue = _serverExchangeValue,
                SharedKey = SharedKey,
            }.GetBytes();

            return Hash(hashData);
        }

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            Session.RegisterMessage("SSH_MSG_KEXDH_REPLY");

            Session.KeyExchangeDhReplyMessageReceived += Session_KeyExchangeDhReplyMessageReceived;

            _prime = GroupPrime;
            _group = new BigInteger(new byte[] { 2 });

            PopulateClientExchangeValue();

            SendMessage(new KeyExchangeDhInitMessage(_clientExchangeValue));
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public override void Finish()
        {
            base.Finish();

            Session.KeyExchangeDhReplyMessageReceived -= Session_KeyExchangeDhReplyMessageReceived;
        }

        private void Session_KeyExchangeDhReplyMessageReceived(object sender, MessageEventArgs<KeyExchangeDhReplyMessage> e)
        {
            var message = e.Message;

            //  Unregister message once received
            Session.UnRegisterMessage("SSH_MSG_KEXDH_REPLY");

            HandleServerDhReply(message.HostKey, message.F, message.Signature);

            //  When SSH_MSG_KEXDH_REPLY received key exchange is completed
            Finish();
        }

        protected override byte[] Hash(byte[] hashBytes)
        {
            using (var sha256 = CryptoAbstraction.CreateSHA256())
            {
                return sha256.ComputeHash(hashBytes);
            }
        }

    }
}
