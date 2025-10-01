using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal partial class KeyExchangeECCurve25519 : KeyExchangeEC
    {
#pragma warning disable SA1401 // Fields should be private
#if NET
        protected Impl _impl;
#else
        protected BouncyCastleImpl _impl;
#endif
#pragma warning restore SA1401 // Fields should be private

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
#if NET
            if (System.OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                _impl = new BclImpl();
            }
            else
#endif
            {
                _impl = new BouncyCastleImpl();
            }

            StartImpl();
        }

        /// <summary>
        /// The implementation of start key exchange algorithm.
        /// </summary>
        protected virtual void StartImpl()
        {
            Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            Session.KeyExchangeEcdhReplyMessageReceived += Session_KeyExchangeEcdhReplyMessageReceived;

            _clientExchangeValue = _impl.GenerateClientPublicKey();

            SendMessage(new KeyExchangeEcdhInitMessage(_clientExchangeValue));
        }

        /// <inheritdoc/>
        public override void Finish()
        {
            base.Finish();
            FinishImpl();
        }

        /// <summary>
        /// The implementation of finish key exchange algorithm.
        /// </summary>
        protected virtual void FinishImpl()
        {
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
            return CryptoAbstraction.HashSHA256(hashData);
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

            var k1 = _impl.CalculateAgreement(serverExchangeValue);
            SharedKey = k1.ToBigInteger2().ToByteArray(isBigEndian: true);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _impl?.Dispose();
            }
        }
    }
}
