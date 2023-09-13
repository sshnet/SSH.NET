using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal abstract class KeyExchangeEC : KeyExchange
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
        /// Specifies client exchange.
        /// </summary>
        protected byte[] _clientExchangeValue;

        /// <summary>
        /// Specifies server exchange.
        /// </summary>
        protected byte[] _serverExchangeValue;

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
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>
        /// Key exchange hash.
        /// </returns>
        protected override byte[] CalculateHash()
        {
            var hashData = new KeyExchangeHashData
                {
                    ClientVersion = Session.ClientVersion,
                    ServerVersion = Session.ServerVersion,
                    ClientPayload = _clientPayload,
                    ServerPayload = _serverPayload,
                    HostKey = _hostKey,
                    ClientExchangeValue = _clientExchangeValue,
                    ServerExchangeValue = _serverExchangeValue,
                    SharedKey = SharedKey,
                };

            return Hash(hashData.GetBytes());
        }

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
   }
}
