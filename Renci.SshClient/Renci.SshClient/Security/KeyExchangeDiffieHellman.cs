using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Messages.Transport;
using System.Diagnostics;
using Renci.SshClient.Messages;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents base class for Diffie Hellman key exchange algorithm
    /// </summary>
    public abstract class KeyExchangeDiffieHellman : KeyExchange
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

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
        protected string _clientPayload;

        /// <summary>
        /// Specifies server payload
        /// </summary>
        protected string _serverPayload;

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
        protected string _hostKey;

        /// <summary>
        /// Specifies signature data.
        /// </summary>
        protected string _signature;

        /// <summary>
        /// Validates the exchange hash.
        /// </summary>
        /// <returns>
        /// true if exchange hash is valid; otherwise false.
        /// </returns>
        protected override bool ValidateExchangeHash()
        {
            var exchangeHash = this.CalculateHash();

            var hostKey = this._hostKey;

            var signature = this._signature;

            var bytes = hostKey.GetSshBytes();

            var length = (uint)(hostKey[0] << 24 | hostKey[1] << 16 | hostKey[2] << 8 | hostKey[3]);

            var algorithmName = bytes.Skip(4).Take((int)length).GetSshString();

            var data = bytes.Skip(4 + algorithmName.Length);

            CryptoPublicKey key = this.Session.ConnectionInfo.HostKeyAlgorithms[algorithmName].CreateInstance<CryptoPublicKey>();

            key.Load(data);

            return key.VerifySignature(exchangeHash, signature.GetSshBytes());
        }

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            this._serverPayload = message.GetBytes().GetSshString();
            this._clientPayload = this.Session.ClientInitMessage.GetBytes().GetSshString();
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

            var bytesArray = new byte[128];
            do
            {
                _randomizer.GetBytes(bytesArray);
                bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
                this._randomValue = new BigInteger(bytesArray);
                this._clientExchangeValue = System.Numerics.BigInteger.ModPow(this._group, this._randomValue, this._prime);

            } while (this._clientExchangeValue < 1 || this._clientExchangeValue > ((this._prime - 1)));
        }

        /// <summary>
        /// Handles the server DH reply message.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        /// <param name="serverExchangeValue">The server exchange value.</param>
        /// <param name="signature">The signature.</param>
        protected virtual void HandleServerDhReply(string hostKey, BigInteger serverExchangeValue, string signature)
        {
            this._serverExchangeValue = serverExchangeValue;
            this._hostKey = hostKey;
            this.SharedKey = System.Numerics.BigInteger.ModPow(serverExchangeValue, this._randomValue, this._prime);
            this._signature = signature;
        }
    }
}
