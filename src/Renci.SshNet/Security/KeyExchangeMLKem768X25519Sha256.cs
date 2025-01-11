using System.Globalization;
using System.Linq;

using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal sealed class KeyExchangeMLKem768X25519Sha256 : KeyExchangeEC
    {
        private MLKemDecapsulator _mlkemDecapsulator;
        private X25519Agreement _x25519Agreement;

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "mlkem768x25519-sha256"; }
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

            Session.RegisterMessage("SSH_MSG_KEX_HYBRID_REPLY");

            Session.KeyExchangeHybridReplyMessageReceived += Session_KeyExchangeHybridReplyMessageReceived;

            var mlkem768KeyPairGenerator = new MLKemKeyPairGenerator();
            mlkem768KeyPairGenerator.Init(new MLKemKeyGenerationParameters(CryptoAbstraction.SecureRandom, MLKemParameters.ml_kem_768));
            var mlkem768KeyPair = mlkem768KeyPairGenerator.GenerateKeyPair();

            _mlkemDecapsulator = new MLKemDecapsulator(MLKemParameters.ml_kem_768);
            _mlkemDecapsulator.Init(mlkem768KeyPair.Private);

            var x25519KeyPairGenerator = new X25519KeyPairGenerator();
            x25519KeyPairGenerator.Init(new X25519KeyGenerationParameters(CryptoAbstraction.SecureRandom));
            var x25519KeyPair = x25519KeyPairGenerator.GenerateKeyPair();

            _x25519Agreement = new X25519Agreement();
            _x25519Agreement.Init(x25519KeyPair.Private);

            var mlkem768PublicKey = ((MLKemPublicKeyParameters)mlkem768KeyPair.Public).GetEncoded();
            var x25519PublicKey = ((X25519PublicKeyParameters)x25519KeyPair.Public).GetEncoded();

            _clientExchangeValue = mlkem768PublicKey.Concat(x25519PublicKey);

            SendMessage(new KeyExchangeHybridInitMessage(_clientExchangeValue));
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public override void Finish()
        {
            base.Finish();

            Session.KeyExchangeHybridReplyMessageReceived -= Session_KeyExchangeHybridReplyMessageReceived;
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
            return CryptoAbstraction.HashSHA256(hashData);
        }

        private void Session_KeyExchangeHybridReplyMessageReceived(object sender, MessageEventArgs<KeyExchangeHybridReplyMessage> e)
        {
            var message = e.Message;

            // Unregister message once received
            Session.UnRegisterMessage("SSH_MSG_KEX_HYBRID_REPLY");

            HandleServerHybridReply(message.KS, message.SReply, message.Signature);

            // When SSH_MSG_KEX_HYBRID_REPLY received key exchange is completed
            Finish();
        }

        /// <summary>
        /// Handles the server hybrid reply message.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        /// <param name="serverExchangeValue">The server exchange value.</param>
        /// <param name="signature">The signature.</param>
        private void HandleServerHybridReply(byte[] hostKey, byte[] serverExchangeValue, byte[] signature)
        {
            _serverExchangeValue = serverExchangeValue;
            _hostKey = hostKey;
            _signature = signature;

            if (serverExchangeValue.Length != _mlkemDecapsulator.EncapsulationLength + _x25519Agreement.AgreementSize)
            {
                throw new SshConnectionException(
                    string.Format(CultureInfo.CurrentCulture, "Bad S_Reply length: {0}.", serverExchangeValue.Length),
                    DisconnectReason.KeyExchangeFailed);
            }

            var secret = new byte[_mlkemDecapsulator.SecretLength + _x25519Agreement.AgreementSize];

            _mlkemDecapsulator.Decapsulate(serverExchangeValue, 0, _mlkemDecapsulator.EncapsulationLength, secret, 0, _mlkemDecapsulator.SecretLength);

            var x25519PublicKey = new X25519PublicKeyParameters(serverExchangeValue, _mlkemDecapsulator.EncapsulationLength);
            _x25519Agreement.CalculateAgreement(x25519PublicKey, secret, _mlkemDecapsulator.SecretLength);

            SharedKey = CryptoAbstraction.HashSHA256(secret);
        }
    }
}
