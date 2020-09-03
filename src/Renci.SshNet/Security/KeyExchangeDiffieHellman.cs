using System;
using System.Text;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for Diffie Hellman key exchange algorithm
    /// </summary>
    internal abstract class KeyExchangeDiffieHellman : KeyExchange
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
        protected byte[] _clientExchangeValue;

        /// <summary>
        /// Specifies server exchange number.
        /// </summary>
        protected byte[] _serverExchangeValue;

        /// <summary>
        /// Specifies random generated number.
        /// </summary>
        protected BigInteger _privateExponent;

        /// <summary>
        /// Specifies host key data.
        /// </summary>
        protected byte[] _hostKey;

        /// <summary>
        /// Specifies signature data.
        /// </summary>
        protected byte[] _signature;

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected abstract int HashSize { get; }

        /// <summary>
        /// Validates the exchange hash.
        /// </summary>
        /// <returns>
        /// true if exchange hash is valid; otherwise false.
        /// </returns>
        protected override bool ValidateExchangeHash()
        {
            var exchangeHash = CalculateHash();

            var length = Pack.BigEndianToUInt32(_hostKey);
            var algorithmName = Encoding.UTF8.GetString(_hostKey, 4, (int)length);
            var key = Session.ConnectionInfo.HostKeyAlgorithms[algorithmName](_hostKey);

            Session.ConnectionInfo.CurrentHostKeyAlgorithm = algorithmName;

            if (CanTrustHostKey(key))
            {
                return key.VerifySignature(exchangeHash, _signature);
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

            _serverPayload = message.GetBytes();
            _clientPayload = Session.ClientInitMessage.GetBytes();
        }

        /// <summary>
        /// Populates the client exchange value.
        /// </summary>
        protected void PopulateClientExchangeValue()
        {
            if (_group.IsZero)
                throw new ArgumentNullException("_group");

            if (_prime.IsZero)
                throw new ArgumentNullException("_prime");

            // generate private exponent that is twice the hash size (RFC 4419) with a minimum
            // of 1024 bits (whatever is less)
            var privateExponentSize = Math.Max(HashSize * 2, 1024);

            BigInteger clientExchangeValue;

            do
            {
                // create private component
                _privateExponent = BigInteger.Random(privateExponentSize);
                // generate public component
                clientExchangeValue = BigInteger.ModPow(_group, _privateExponent, _prime);
            } while (clientExchangeValue < 1 || clientExchangeValue > (_prime - 1));

            _clientExchangeValue = clientExchangeValue.ToByteArray().Reverse();
        }

        /// <summary>
        /// Handles the server DH reply message.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        /// <param name="serverExchangeValue">The server exchange value.</param>
        /// <param name="signature">The signature.</param>
        protected virtual void HandleServerDhReply(byte[] hostKey, byte[] serverExchangeValue, byte[] signature)
        {
            _serverExchangeValue = serverExchangeValue;
            _hostKey = hostKey;
            SharedKey = BigInteger.ModPow(serverExchangeValue.ToBigInteger(), _privateExponent, _prime).ToByteArray().Reverse();
            _signature = signature;
        }
    }
}
