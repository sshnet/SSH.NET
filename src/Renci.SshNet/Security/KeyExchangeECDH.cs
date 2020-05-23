using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Generators;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Parameters;
using Renci.SshNet.Security.Org.BouncyCastle.Security;
using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;
using Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Agreement;

namespace Renci.SshNet.Security
{
    internal abstract class KeyExchangeECDH : KeyExchangeEC
    {
        /// <summary>
        /// Gets the parameter of the curve.
        /// </summary>
        /// <value>
        /// The parameter of the curve.
        /// </value>
        protected abstract X9ECParameters CurveParameter { get; }

        protected ECDHCBasicAgreement KeyAgreement;
        protected ECDomainParameters DomainParameters;

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            Session.KeyExchangeEcdhReplyMessageReceived += Session_KeyExchangeEcdhReplyMessageReceived;

            DomainParameters = new ECDomainParameters(CurveParameter.Curve,
                                                      CurveParameter.G,
                                                      CurveParameter.N,
                                                      CurveParameter.H,
                                                      CurveParameter.GetSeed());

            var g = new ECKeyPairGenerator();
            g.Init(new ECKeyGenerationParameters(DomainParameters, new SecureRandom()));

            var aKeyPair = g.GenerateKeyPair();
            KeyAgreement = new ECDHCBasicAgreement();
            KeyAgreement.Init(aKeyPair.Private);
            _clientExchangeValue = ((ECPublicKeyParameters)aKeyPair.Public).Q.GetEncoded();

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

            //  Unregister message once received
            Session.UnRegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            HandleServerEcdhReply(message.KS, message.QS, message.Signature);

            //  When SSH_MSG_KEXDH_REPLY received key exchange is completed
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

            var cordSize = (serverExchangeValue.Length - 1) / 2;
            var x = new byte[cordSize];
            Buffer.BlockCopy(serverExchangeValue, 1, x, 0, x.Length); // first byte is format. should be checked and passed to bouncy castle?
            var y = new byte[cordSize];
            Buffer.BlockCopy(serverExchangeValue, cordSize + 1, y, 0, y.Length);

            var c = (FpCurve)DomainParameters.Curve;
            var q = c.CreatePoint(new Org.BouncyCastle.Math.BigInteger(1, x), new Org.BouncyCastle.Math.BigInteger(1, y));
            var publicKey = new ECPublicKeyParameters("ECDH", q, DomainParameters);

            var k1 = KeyAgreement.CalculateAgreement(publicKey);
            SharedKey = k1.ToByteArray().ToBigInteger2().ToByteArray().Reverse();
        }
    }
}
