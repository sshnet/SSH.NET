using System;

using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal sealed class KeyExchangeECCurve25519 : KeyExchangeEC
    {
        private X25519Agreement _keyAgreement;

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "curve25519-sha256"; }
        }

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

        /// <inheritdoc/>
        public override void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            base.Start(session, message, sendClientInitMessage);

            Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            Session.KeyExchangeEcdhReplyMessageReceived += Session_KeyExchangeEcdhReplyMessageReceived;

            var g = new X25519KeyPairGenerator();
            g.Init(new X25519KeyGenerationParameters(new SecureRandom()));

            var aKeyPair = g.GenerateKeyPair();
            _keyAgreement = new X25519Agreement();
            _keyAgreement.Init(aKeyPair.Private);
            _clientExchangeValue = ((X25519PublicKeyParameters)aKeyPair.Public).GetEncoded();

            SendMessage(new KeyExchangeEcdhInitMessage(_clientExchangeValue));
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public override void Finish()
        {
            base.Finish();

            Session.KeyExchangeEcdhReplyMessageReceived -= Session_KeyExchangeEcdhReplyMessageReceived;
        }

        /// <summary>
        /// Hashes the specified data bytes.
        /// </summary>
        /// <param name="hashData">The hash data.</param>
        /// <returns>
        /// The hash of the data.
        /// </returns>
        protected override byte[] Hash(byte[] hashData)
        {
            using (var sha256 = CryptoAbstraction.CreateSHA256())
            {
                return sha256.ComputeHash(hashData, 0, hashData.Length);
            }
        }

        private void Session_KeyExchangeEcdhReplyMessageReceived(object sender, MessageEventArgs<KeyExchangeEcdhReplyMessage> e)
        {
            var message = e.Message;

            // Unregister message once received
            Session.UnRegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            HandleServerEcdhReply(message.KS, message.QS, message.Signature);

            // When SSH_MSG_KEXDH_REPLY received key exchange is completed
            Finish();
        }

        /// <summary>
        /// Handles the server DH reply message.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        /// <param name="serverExchangeValue">The server exchange value.</param>
        /// <param name="signature">The signature.</param>
        private void HandleServerEcdhReply(byte[] hostKey, byte[] serverExchangeValue, byte[] signature)
        {
            _serverExchangeValue = serverExchangeValue;
            _hostKey = hostKey;
            _signature = signature;

            var publicKey = new X25519PublicKeyParameters(serverExchangeValue);

            var k1 = new byte[_keyAgreement.AgreementSize];
            _keyAgreement.CalculateAgreement(publicKey, k1, 0);
            SharedKey = k1.ToBigInteger2().ToByteArray().Reverse();
        }
    }
}
