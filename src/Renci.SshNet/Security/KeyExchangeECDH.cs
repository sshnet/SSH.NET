using System;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

using Renci.SshNet.Security.Org.BouncyCastle.Asn1.Sec;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Agreement;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Generators;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Parameters;
using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;
using Renci.SshNet.Security.Org.BouncyCastle.Security;

namespace Renci.SshNet.Security
{
    internal abstract class KeyExchangeECDH : KeyExchangeEC
    {
#if NET8_0_OR_GREATER
        private System.Security.Cryptography.ECDiffieHellman _clientECDH;
#endif
        private ECDHCBasicAgreement _keyAgreement;
        private ECDomainParameters _domainParameters;

        /// <summary>
        /// Gets the name of the curve.
        /// </summary>
        /// <value>
        /// The name of the curve.
        /// </value>
        protected abstract string CurveName { get; }

        /// <inheritdoc/>
        public override void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            base.Start(session, message, sendClientInitMessage);

            Session.RegisterMessage("SSH_MSG_KEX_ECDH_REPLY");

            Session.KeyExchangeEcdhReplyMessageReceived += Session_KeyExchangeEcdhReplyMessageReceived;

#if NET8_0_OR_GREATER
            if (!OperatingSystem.IsWindows() || OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                _clientECDH = System.Security.Cryptography.ECDiffieHellman.Create();
                _clientECDH.GenerateKey(System.Security.Cryptography.ECCurve.CreateFromFriendlyName(CurveName));

                var q = _clientECDH.PublicKey.ExportParameters().Q;

                _clientExchangeValue = new byte[1 + q.X.Length + q.Y.Length];
                _clientExchangeValue[0] = 0x04;
                Buffer.BlockCopy(q.X, 0, _clientExchangeValue, 1, q.X.Length);
                Buffer.BlockCopy(q.Y, 0, _clientExchangeValue, q.X.Length + 1, q.Y.Length);

                return;
            }
#endif
            var curveParameter = SecNamedCurves.GetByName(CurveName);
            _domainParameters = new ECDomainParameters(curveParameter.Curve,
                                      curveParameter.G,
                                      curveParameter.N,
                                      curveParameter.H,
                                      curveParameter.GetSeed());

            var g = new ECKeyPairGenerator();
            g.Init(new ECKeyGenerationParameters(_domainParameters, new SecureRandom()));

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
                using var serverECDH = System.Security.Cryptography.ECDiffieHellman.Create(
                    new System.Security.Cryptography.ECParameters
                    {
                        Curve = System.Security.Cryptography.ECCurve.CreateFromFriendlyName(CurveName),
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
            var c = (FpCurve)_domainParameters.Curve;
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
