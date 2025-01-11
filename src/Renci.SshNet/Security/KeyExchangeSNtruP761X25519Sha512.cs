using System;
using System.Globalization;
using System.Linq;

using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pqc.Crypto.NtruPrime;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal sealed class KeyExchangeSNtruP761X25519Sha512 : KeyExchangeEC
    {
        private SNtruPrimeKemExtractor _sntrup761Extractor;
        private X25519Agreement _x25519Agreement;

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "sntrup761x25519-sha512"; }
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected override int HashSize
        {
            get { return 512; }
        }

        /// <inheritdoc/>
        public override void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            base.Start(session, message, sendClientInitMessage);

            Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            Session.KeyExchangeEcdhReplyMessageReceived += Session_KeyExchangeEcdhReplyMessageReceived;

            var sntrup761KeyPairGenerator = new SNtruPrimeKeyPairGenerator();
            sntrup761KeyPairGenerator.Init(new SNtruPrimeKeyGenerationParameters(CryptoAbstraction.SecureRandom, SNtruPrimeParameters.sntrup761));
            var sntrup761KeyPair = sntrup761KeyPairGenerator.GenerateKeyPair();

            _sntrup761Extractor = new SNtruPrimeKemExtractor((SNtruPrimePrivateKeyParameters)sntrup761KeyPair.Private);

            var x25519KeyPairGenerator = new X25519KeyPairGenerator();
            x25519KeyPairGenerator.Init(new X25519KeyGenerationParameters(CryptoAbstraction.SecureRandom));
            var x25519KeyPair = x25519KeyPairGenerator.GenerateKeyPair();

            _x25519Agreement = new X25519Agreement();
            _x25519Agreement.Init(x25519KeyPair.Private);

            var sntrup761PublicKey = ((SNtruPrimePublicKeyParameters)sntrup761KeyPair.Public).GetEncoded();
            var x25519PublicKey = ((X25519PublicKeyParameters)x25519KeyPair.Public).GetEncoded();

            _clientExchangeValue = sntrup761PublicKey.Concat(x25519PublicKey);

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
            return CryptoAbstraction.HashSHA512(hashData);
        }

        private void Session_KeyExchangeEcdhReplyMessageReceived(object sender, MessageEventArgs<KeyExchangeEcdhReplyMessage> e)
        {
            var message = e.Message;

            // Unregister message once received
            Session.UnRegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            HandleServerEcdhReply(message.KS, message.QS, message.Signature);

            // When SSH_MSG_KEX_ECDH_REPLY received key exchange is completed
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

            if (serverExchangeValue.Length != _sntrup761Extractor.EncapsulationLength + X25519PublicKeyParameters.KeySize)
            {
                throw new SshConnectionException(
                    string.Format(CultureInfo.CurrentCulture, "Bad Q_S length: {0}.", serverExchangeValue.Length),
                    DisconnectReason.KeyExchangeFailed);
            }

            var sntrup761CipherText = serverExchangeValue.Take(_sntrup761Extractor.EncapsulationLength);
            var secret = _sntrup761Extractor.ExtractSecret(sntrup761CipherText);
            var sntrup761SecretLength = secret.Length;

            var x25519PublicKey = new X25519PublicKeyParameters(serverExchangeValue, _sntrup761Extractor.EncapsulationLength);
            Array.Resize(ref secret, sntrup761SecretLength + _x25519Agreement.AgreementSize);
            _x25519Agreement.CalculateAgreement(x25519PublicKey, secret, sntrup761SecretLength);

            SharedKey = CryptoAbstraction.HashSHA512(secret);
        }
    }
}
