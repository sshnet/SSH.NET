using System;
#if NET8_0_OR_GREATER
using System.Security.Cryptography;
#endif

using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal abstract class KeyExchangeECDH : KeyExchangeEC
    {
#if NET8_0_OR_GREATER
        private ECDiffieHellman _clientECDH;
#endif
        private ECDHCBasicAgreement _keyAgreement;
        private ECDomainParameters _domainParameters;

#if NET8_0_OR_GREATER
        /// <summary>
        /// Gets the curve.
        /// </summary>
        /// <value>
        /// The curve.
        /// </value>
        protected abstract ECCurve Curve { get; }
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

#if NET8_0_OR_GREATER
            if (!OperatingSystem.IsWindows() || OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                _clientECDH = ECDiffieHellman.Create();
                _clientECDH.GenerateKey(Curve);

                var q = _clientECDH.PublicKey.ExportParameters().Q;

                _clientExchangeValue = new byte[1 + q.X.Length + q.Y.Length];
                _clientExchangeValue[0] = 0x04;
                Buffer.BlockCopy(q.X, 0, _clientExchangeValue, 1, q.X.Length);
                Buffer.BlockCopy(q.Y, 0, _clientExchangeValue, q.X.Length + 1, q.Y.Length);

                SendMessage(new KeyExchangeEcdhInitMessage(_clientExchangeValue));

                return;
            }
#endif
            _domainParameters = new ECDomainParameters(CurveParameter);

            var g = new ECKeyPairGenerator();
            g.Init(new ECKeyGenerationParameters(_domainParameters, CryptoAbstraction.SecureRandom));

            var aKeyPair = g.GenerateKeyPair();
            _keyAgreement = new ECDHCBasicAgreement();
            _keyAgreement.Init(aKeyPair.Private);
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

            var cordSize = (serverExchangeValue.Length - 1) / 2;
            var x = new byte[cordSize];
            Buffer.BlockCopy(serverExchangeValue, 1, x, 0, x.Length); // first byte is format. should be checked and passed to bouncy castle?
            var y = new byte[cordSize];
            Buffer.BlockCopy(serverExchangeValue, cordSize + 1, y, 0, y.Length);

#if NET8_0_OR_GREATER
            if (!OperatingSystem.IsWindows() || OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                using var serverECDH = ECDiffieHellman.Create(
                    new ECParameters
                    {
                        Curve = Curve,
                        Q =
                        {
                            X = x,
                            Y = y,
                        },
                    });

                var k = _clientECDH.DeriveRawSecretAgreement(serverECDH.PublicKey);
                SharedKey = k.ToBigInteger2().ToByteArray().Reverse();

                return;
            }
#endif
            var c = _domainParameters.Curve;
            var q = c.CreatePoint(new Org.BouncyCastle.Math.BigInteger(1, x), new Org.BouncyCastle.Math.BigInteger(1, y));
            var publicKey = new ECPublicKeyParameters("ECDH", q, _domainParameters);

            var k1 = _keyAgreement.CalculateAgreement(publicKey);
            SharedKey = k1.ToByteArray().ToBigInteger2().ToByteArray().Reverse();
        }

#if NET8_0_OR_GREATER

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _clientECDH?.Dispose();
            }
        }
#endif
    }
}
