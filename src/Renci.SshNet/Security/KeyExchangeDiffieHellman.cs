using System;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for Diffie Hellman key exchange algorithm.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellman : KeyExchange
    {
        /// <summary>
        /// Specifies key exchange group number.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected BigInteger _group;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies key exchange prime number.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected BigInteger _prime;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies client payload.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _clientPayload;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies server payload.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _serverPayload;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies client exchange number.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _clientExchangeValue;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies server exchange number.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _serverExchangeValue;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies random generated number.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected BigInteger _privateExponent;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies host key data.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _hostKey;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies signature data.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _signature;
#pragma warning restore SA1401 // Fields should be private

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
            return ValidateExchangeHash(_hostKey, _signature);
        }

        /// <summary>
        /// Starts key exchange algorithm.
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
            {
                throw new ArgumentNullException("_group");
            }

            if (_prime.IsZero)
            {
                throw new ArgumentNullException("_prime");
            }

            // generate private exponent that is twice the hash size (RFC 4419) with a minimum
            // of 1024 bits (whatever is less)
            var privateExponentSize = Math.Max(HashSize * 2, 1024);

            BigInteger clientExchangeValue;

            do
            {
                // Create private component
                _privateExponent = BigInteger.Random(privateExponentSize);

                // Generate public component
                clientExchangeValue = BigInteger.ModPow(_group, _privateExponent, _prime);
            }
            while (clientExchangeValue < 1 || clientExchangeValue > (_prime - 1));

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
