using System;
using System.Text;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Common;
using Renci.SshNet.Abstractions;

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
        /// Hashes the specified data bytes.
        /// </summary>
        /// <param name="hashData">The hash data.</param>
        /// <returns>
        /// Hashed bytes
        /// </returns>
        protected override byte[] Hash(byte[] hashData)
        {
            using (var sha256 = CryptoAbstraction.CreateSHA256())
            {
                return sha256.ComputeHash(hashData, 0, hashData.Length);
            }
        }

        /// <summary>
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>
        /// Key exchange hash.
        /// </returns>
        protected override byte[] CalculateHash()
        {
            var hashData = new _ExchangeHashData
            {
                ClientVersion = Session.ClientVersion,
                ServerVersion = Session.ServerVersion,
                ClientPayload = _clientPayload,
                ServerPayload = _serverPayload,
                HostKey = _hostKey,
                ClientExchangeValue = _clientExchangeValue,
                ServerExchangeValue = _serverExchangeValue,
                SharedKey = SharedKey,
            }.GetBytes();

            return Hash(hashData);
        }

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
        /// Handles the server DH reply message.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        /// <param name="serverExchangeValue">The server exchange value.</param>
        /// <param name="signature">The signature.</param>
        protected virtual void HandleServerEcdhReply(byte[] hostKey, byte[] serverExchangeValue, byte[] signature)
        {
        }

        internal class _ExchangeHashData : SshData
        {
            private byte[] _serverVersion;
            private byte[] _clientVersion;
            private byte[] _sharedKey;

            public string ServerVersion
            {
                private get { return Utf8.GetString(_serverVersion, 0, _serverVersion.Length); }
                set { _serverVersion = Utf8.GetBytes(value); }
            }

            public string ClientVersion
            {
                private get { return Utf8.GetString(_clientVersion, 0, _clientVersion.Length); }
                set { _clientVersion = Utf8.GetBytes(value); }
            }

            public byte[] ClientPayload { get; set; }

            public byte[] ServerPayload { get; set; }

            public byte[] HostKey { get; set; }

            public byte[] ClientExchangeValue { get; set; }

            public byte[] ServerExchangeValue { get; set; }

            public BigInteger SharedKey
            {
                private get { return _sharedKey.ToBigInteger(); }
                set { _sharedKey = value.ToByteArray().Reverse(); }
            }
            /// <summary>
            /// Gets the size of the message in bytes.
            /// </summary>
            /// <value>
            /// The size of the messages in bytes.
            /// </value>
            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // ClientVersion length
                    capacity += _clientVersion.Length; // ClientVersion
                    capacity += 4; // ServerVersion length
                    capacity += _serverVersion.Length; // ServerVersion
                    capacity += 4; // ClientPayload length
                    capacity += ClientPayload.Length; // ClientPayload
                    capacity += 4; // ServerPayload length
                    capacity += ServerPayload.Length; // ServerPayload
                    capacity += 4; // HostKey length
                    capacity += HostKey.Length; // HostKey
                    capacity += 4; // ClientExchangeValue length
                    capacity += ClientExchangeValue.Length; // ClientExchangeValue
                    capacity += 4; // ServerExchangeValue length
                    capacity += ServerExchangeValue.Length; // ServerExchangeValue
                    capacity += 4; // SharedKey length
                    capacity += _sharedKey.Length; // SharedKey
                    return capacity;
                }
            }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                WriteBinaryString(_clientVersion);
                WriteBinaryString(_serverVersion);
                WriteBinaryString(ClientPayload);
                WriteBinaryString(ServerPayload);
                WriteBinaryString(HostKey);
                WriteBinaryString(ClientExchangeValue);
                WriteBinaryString(ServerExchangeValue);
                WriteBinaryString(_sharedKey);
           }
        }
    }
}