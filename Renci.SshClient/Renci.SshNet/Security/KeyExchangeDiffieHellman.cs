using System;
using System.Linq;
using System.Text;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for Diffie Hellman key exchange algorithm
    /// </summary>
    public abstract class KeyExchangeDiffieHellman : KeyExchange
    {
        /// <summary>
        /// Specifies key exchange group number.
        /// </summary>
        protected BigInteger _group;

        /// <summary>
        /// Specifies key exchange prime number.
        /// </summary>
        protected BigInteger _prime;

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

        /// <summary>
        /// Validates the exchange hash.
        /// </summary>
        /// <returns>
        /// true if exchange hash is valid; otherwise false.
        /// </returns>
        protected override bool ValidateExchangeHash()
        {
            var exchangeHash = this.CalculateHash();

            var length = (uint)(this._hostKey[0] << 24 | this._hostKey[1] << 16 | this._hostKey[2] << 8 | this._hostKey[3]);

            var algorithmName = Encoding.UTF8.GetString(this._hostKey, 4, (int)length);

            var key = this.Session.ConnectionInfo.HostKeyAlgorithms[algorithmName](this._hostKey);

            this.Session.ConnectionInfo.CurrentHostKeyAlgorithm = algorithmName;

            if (this.CanTrustHostKey(key))
            {

                return key.VerifySignature(exchangeHash, this._signature);
            }
            return false;
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
        }

        /// <summary>
        /// Populates the client exchange value.
        /// </summary>
        protected void PopulateClientExchangeValue()
        {
            if (this._group.IsZero)
                throw new ArgumentNullException("_group");

            if (this._prime.IsZero)
                throw new ArgumentNullException("_prime");

            var bitLength = this._prime.BitLength;

            do
            {
                this._randomValue = BigInteger.Random(bitLength);

                this._clientExchangeValue = BigInteger.ModPow(this._group, this._randomValue, this._prime);

            } while (this._clientExchangeValue < 1 || this._clientExchangeValue > ((this._prime - 1)));
        }

        /// <summary>
        /// Handles the server DH reply message.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        /// <param name="serverExchangeValue">The server exchange value.</param>
        /// <param name="signature">The signature.</param>
        protected virtual void HandleServerDhReply(byte[] hostKey, BigInteger serverExchangeValue, byte[] signature)
        {
            this._serverExchangeValue = serverExchangeValue;
            this._hostKey = hostKey;
            this.SharedKey = BigInteger.ModPow(serverExchangeValue, this._randomValue, this._prime);
            this._signature = signature;
        }
    }
}
