using System;
using System.Linq;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Messages;
using Renci.SshNet.Common;
using System.Globalization;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for Diffie Hellman key exchange algorithm
    /// </summary>
    public class KeyExchangeEllipticCurveDiffieHellman : KeyExchange
    {
        /// <summary>
        /// Specifies client payload
        /// </summary>
        protected byte[] _clientPayload;

        /// <summary>
        /// Specifies server payload
        /// </summary>
        protected byte[] _serverPayload;

        /// <summary>
        /// Specifies client exchange number.
        /// </summary>
        protected BigInteger _clientExchangeValue;

        /// <summary>
        /// Specifies server exchange number.
        /// </summary>
        protected BigInteger _serverExchangeValue;

        /// <summary>
        /// Specifies random generated number.
        /// </summary>
        protected BigInteger _randomValue;

        /// <summary>
        /// Specifies host key data.
        /// </summary>
        protected byte[] _hostKey;

        /// <summary>
        /// Specifies signature data.
        /// </summary>
        protected byte[] _signature;

        ////  256
        //p = FFFFFFFF 00000001 00000000 00000000 00000000 FFFFFFFF FFFFFFFF FFFFFFFF
        //a = FFFFFFFF 00000001 00000000 00000000 00000000 FFFFFFFF FFFFFFFF FFFFFFFC
        //b = 5AC635D8 AA3A93E7 B3EBBD55 769886BC 651D06B0 CC53B0F6 3BCE3C3E 27D2604B
        //S = C49D3608 86E70493 6A6678E1 139D26B7 819F7E90
        //The base point G in compressed form is:
        //G = 03 6B17D1F2 E12C4247 F8BCE6E5 63A440F2 77037D81 2DEB33A0 F4A13945 D898C296
        //and in uncompressed form is:
        //G = 04 6B17D1F2 E12C4247 F8BCE6E5 63A440F2 77037D81 2DEB33A0 F4A13945 D898C296 4FE342E2 FE1A7F9B 8EE7EB4A 7C0F9E16 2BCE3357 6B315ECE CBB64068 37BF51F5
        //n = FFFFFFFF 00000000 FFFFFFFF FFFFFFFF BCE6FAAD A7179E84 F3B9CAC2 FC632551
        //h = 01

        ////  384
        //p = FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE FFFFFFFF 00000000 00000000 FFFFFFFF
        //a = FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE FFFFFFFF 00000000 00000000 FFFFFFFC
        //b = B3312FA7 E23EE7E4 988E056B E3F82D19 181D9C6E FE814112 0314088F 5013875A C656398D 8A2ED19D 2A85C8ED D3EC2AEF
        //S = A335926A A319A27A 1D00896A 6773A482 7ACDAC73
        //The base point G in compressed form is:
        //G = 03 AA87CA22 BE8B0537 8EB1C71E F320AD74 6E1D3B62 8BA79B98 59F741E0 82542A38 5502F25D BF55296C 3A545E38 72760AB7
        //and in uncompressed form is:
        //G = 04 AA87CA22 BE8B0537 8EB1C71E F320AD74 6E1D3B62 8BA79B98 59F741E0 82542A38 5502F25D BF55296C 3A545E38 72760AB7 3617DE4A 96262C6F 5D9E98BF 9292DC29 F8F41DBD 289A147C E9DA3113 B5F0B8C0 0A60B1CE 1D7E819D 7A431D7C 90EA0E5F
        //n = FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF C7634D81 F4372DDF 581A0DB2 48B0A77A ECEC196A CCC52973
        //h = 01

        public override string Name
        {
            get { return "ecdh-sha2-nistp256"; }
        }

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            this._serverPayload = message.GetBytes().ToArray();
            this._clientPayload = this.Session.ClientInitMessage.GetBytes().ToArray();

            this.Session.RegisterMessage("SSH_MSG_KEXECDH_REPLY");

            this.Session.MessageReceived += Session_MessageReceived;

            //3.2.1 Elliptic Curve Key Pair Generation Primitive
            //Elliptic curve key pairs should be generated as follows:
            //Input: Valid elliptic curve domain parameters T = (p, a, b, G, n, h) or (m, f(x), a, b,G, n, h).
            //Output: An elliptic curve key pair (d,Q) associated with T.
            //Actions: Generate an elliptic curve key pair as follows:
            //1. Randomly or pseudorandomly select an integer d in the interval [1, n − 1].
            //2. Compute Q = dG.
            //3. Output (d,Q).
            
            BigInteger p;
            BigInteger.TryParse("00FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out p);



            BigInteger n;
            BigInteger.TryParse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFC7634D81F4372DDF581A0DB248B0A77AECEC196ACCC52973", NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out n);
            BigInteger G;
            BigInteger.TryParse("00036B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296", NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out G);

            BigInteger d;

            do
            {
                d = BigInteger.Random(n.BitLength);
            } while (d < 1 || d > n);

            var Q = d * G;


            this.SendMessage(new KeyExchangeEcdhInitMessage(d, Q));

        }

        private void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            var message = e.Message as KeyExchangeEcdhReplyMessage;
            if (message != null)
            {
                //  Unregister message once received
                this.Session.UnRegisterMessage("SSH_MSG_KEXECDH_REPLY");

                this.HandleServerEcdhReply();

                //  When SSH_MSG_KEXDH_REPLY received key exchange is completed
                this.Finish();
            }
        }

        /// <summary>
        /// Validates the exchange hash.
        /// </summary>
        /// <returns>
        /// true if exchange hash is valid; otherwise false.
        /// </returns>
        protected override bool ValidateExchangeHash()
        {
            //var exchangeHash = this.CalculateHash();

            //var length = (uint)(this._hostKey[0] << 24 | this._hostKey[1] << 16 | this._hostKey[2] << 8 | this._hostKey[3]);

            //var algorithmName = Encoding.UTF8.GetString(this._hostKey, 4, (int)length);

            //var key = this.Session.ConnectionInfo.HostKeyAlgorithms[algorithmName](this._hostKey);

            //this.Session.ConnectionInfo.CurrentHostKeyAlgorithm = algorithmName;

            //if (this.CanTrustHostKey(key))
            //{

            //    return key.VerifySignature(exchangeHash, this._signature);
            //}
            //else
            //{
            //    return false;
            //}

            return false;
        }

        /// <summary>
        /// Populates the client exchange value.
        /// </summary>
        //protected void PopulateClientExchangeValue()
        //{
        //    if (this._group.IsZero)
        //        throw new ArgumentNullException("_group");

        //    if (this._prime.IsZero)
        //        throw new ArgumentNullException("_prime");

        //    var bitLength = this._prime.BitLength;

        //    do
        //    {
        //        this._randomValue = BigInteger.Random(bitLength);

        //        this._clientExchangeValue = BigInteger.ModPow(this._group, this._randomValue, this._prime);

        //    } while (this._clientExchangeValue < 1 || this._clientExchangeValue > ((this._prime - 1)));
        //}

        protected virtual void HandleServerEcdhReply()
        {
            //this._serverExchangeValue = serverExchangeValue;
            //this._hostKey = hostKey;
            //this.SharedKey = BigInteger.ModPow(serverExchangeValue, this._randomValue, this._prime);
            //this._signature = signature;
        }

        protected override byte[] CalculateHash()
        {
            var hashData = new _ExchangeHashData
            {
                ClientVersion = this.Session.ClientVersion,
                ServerVersion = this.Session.ServerVersion,
                ClientPayload = this._clientPayload,
                ServerPayload = this._serverPayload,
                HostKey = this._hostKey,
                SharedKey = this.SharedKey,
            }.GetBytes();

            //string   V_C, client's identification string (CR and LF excluded)
            //string   V_S, server's identification string (CR and LF excluded)
            //string   I_C, payload of the client's SSH_MSG_KEXINIT
            //string   I_S, payload of the server's SSH_MSG_KEXINIT
            //string   K_S, server's public host key
            //string   Q_C, client's ephemeral public key octet string
            //string   Q_S, server's ephemeral public key octet string
            //mpint    K,   shared secret
            return this.Hash(hashData);
        }

        private class _ExchangeHashData : SshData
        {
            public string ServerVersion { get; set; }

            public string ClientVersion { get; set; }

            public byte[] ClientPayload { get; set; }

            public byte[] ServerPayload { get; set; }

            public byte[] HostKey { get; set; }

            public UInt32 MinimumGroupSize { get; set; }

            public UInt32 PreferredGroupSize { get; set; }

            public UInt32 MaximumGroupSize { get; set; }

            public BigInteger Prime { get; set; }

            public BigInteger SubGroup { get; set; }

            public BigInteger ClientExchangeValue { get; set; }

            public BigInteger ServerExchangeValue { get; set; }

            public BigInteger SharedKey { get; set; }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                this.Write(this.ClientVersion);
                this.Write(this.ServerVersion);
                this.WriteBinaryString(this.ClientPayload);
                this.WriteBinaryString(this.ServerPayload);
                this.WriteBinaryString(this.HostKey);
                this.Write(this.MinimumGroupSize);
                this.Write(this.PreferredGroupSize);
                this.Write(this.MaximumGroupSize);
                this.Write(this.Prime);
                this.Write(this.SubGroup);
                this.Write(this.ClientExchangeValue);
                this.Write(this.ServerExchangeValue);
                this.Write(this.SharedKey);
            }
        }

    }
}
