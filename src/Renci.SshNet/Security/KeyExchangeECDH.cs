using Org.BouncyCastle.Asn1.X9;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal abstract partial class KeyExchangeECDH : KeyExchangeEC
    {
#if NET
        private Impl _impl;

        /// <summary>
        /// Gets the curve.
        /// </summary>
        /// <value>
        /// The curve.
        /// </value>
        protected abstract System.Security.Cryptography.ECCurve Curve { get; }
#else
        private BouncyCastleImpl _impl;
#endif

        /// <summary>
        /// Gets the parameter of the curve.
        /// </summary>
        /// <value>
        /// The parameter of the curve.
        /// </value>
        protected abstract X9ECParameters CurveParameter { get; }

        /// <inheritdoc/>
        public override void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            base.Start(session, message, sendClientInitMessage);

            Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            Session.KeyExchangeEcdhReplyMessageReceived += Session_KeyExchangeEcdhReplyMessageReceived;

#if NET
            if (!System.OperatingSystem.IsWindows() || System.OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                _impl = new BclImpl(Curve);
            }
            else
#endif
            {
                _impl = new BouncyCastleImpl(CurveParameter);
            }

            _clientExchangeValue = _impl.GenerateClientPublicKey();

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

            var agreement = _impl.CalculateAgreement(serverExchangeValue);

            SharedKey = agreement.ToBigInteger2().ToByteArray(isBigEndian: true);
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
