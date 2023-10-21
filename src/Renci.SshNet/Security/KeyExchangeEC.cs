using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    internal abstract class KeyExchangeEC : KeyExchange
    {
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
        /// Specifies client exchange.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _clientExchangeValue;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Specifies server exchange.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected byte[] _serverExchangeValue;
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
