using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal sealed partial class KeyExchangeMLKem768X25519Sha256 : KeyExchangeECCurve25519
    {
#if Test_BCL_MLKem
        private MLKemBclImpl _mlkemImpl;
#elif Test_BouncyCastle_MLKem
        private MLKemBouncyCastleImpl _mlkemImpl;
#else
        private Impl _mlkemImpl;
#endif

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
        protected override void StartImpl()
        {
            Session.RegisterMessage("SSH_MSG_KEX_HYBRID_REPLY");

            Session.KeyExchangeHybridReplyMessageReceived += Session_KeyExchangeHybridReplyMessageReceived;

#if Test_BCL_MLKem
            _mlkemImpl = new MLKemBclImpl();
#elif Test_BouncyCastle_MLKem
            _mlkemImpl = new MLKemBouncyCastleImpl();
#else
            if (MLKem.IsSupported)
            {
                _mlkemImpl = new MLKemBclImpl();
            }
            else
            {
                _mlkemImpl = new MLKemBouncyCastleImpl();
            }
#endif
            var mlkem768PublicKey = _mlkemImpl.GenerateClientPublicKey();

            var x25519PublicKey = _impl.GenerateClientPublicKey();

            _clientExchangeValue = mlkem768PublicKey.Concat(x25519PublicKey);

            SendMessage(new KeyExchangeHybridInitMessage(_clientExchangeValue));
        }

        /// <inheritdoc/>
        protected override void FinishImpl()
        {
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
            return SHA256.HashData(hashData);
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

            if (serverExchangeValue.Length != MLKemAlgorithm.MLKem768.CiphertextSizeInBytes + X25519PublicKeyParameters.KeySize)
            {
                throw new SshConnectionException(
                    string.Format(CultureInfo.CurrentCulture, "Bad S_Reply length: {0}.", serverExchangeValue.Length),
                    DisconnectReason.KeyExchangeFailed);
            }

            var mlkemSecret = _mlkemImpl.CalculateAgreement(serverExchangeValue);

            var x25519Agreement = _impl.CalculateAgreement(serverExchangeValue.Take(MLKemAlgorithm.MLKem768.CiphertextSizeInBytes, X25519PublicKeyParameters.KeySize));

            SharedKey = SHA256.HashData(mlkemSecret.Concat(x25519Agreement));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mlkemImpl?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
