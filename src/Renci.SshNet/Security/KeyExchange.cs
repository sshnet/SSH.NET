using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for different key exchange algorithm implementations
    /// </summary>
    public abstract class KeyExchange : Algorithm, IKeyExchange
    {
        private CipherInfo _clientCipherInfo;
        private CipherInfo _serverCipherInfo;
        private HashInfo _clientHashInfo;
        private HashInfo _serverHashInfo;
        private Type _compressionType;
        private Type _decompressionType;

        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        protected Session Session { get; private set; }

        /// <summary>
        /// Gets or sets key exchange shared key.
        /// </summary>
        /// <value>
        /// The shared key.
        /// </value>
        public byte[] SharedKey { get; protected set; }

        private byte[] _exchangeHash;

        /// <summary>
        /// Gets the exchange hash.
        /// </summary>
        /// <value>The exchange hash.</value>
        public byte[] ExchangeHash
        {
            get
            {
                if (_exchangeHash == null)
                {
                    _exchangeHash = CalculateHash();
                }
                return _exchangeHash;
            }
        }

        /// <summary>
        /// Occurs when host key received.
        /// </summary>
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public virtual void Start(Session session, KeyExchangeInitMessage message)
        {
            Session = session;

            SendMessage(session.ClientInitMessage);

            //  Determine encryption algorithm
            var clientEncryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsClientToServer
                                                 where a == b
                                                 select a).FirstOrDefault();

            if (string.IsNullOrEmpty(clientEncryptionAlgorithmName))
            {
                throw new SshConnectionException("Client encryption algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientEncryption = clientEncryptionAlgorithmName;

            //  Determine encryption algorithm
            var serverDecryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsServerToClient
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverDecryptionAlgorithmName))
            {
                throw new SshConnectionException("Server decryption algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentServerEncryption = serverDecryptionAlgorithmName;

            //  Determine client hmac algorithm
            var clientHmacAlgorithmName = (from b in session.ConnectionInfo.HmacAlgorithms.Keys
                                           from a in message.MacAlgorithmsClientToServer
                                           where a == b
                                           select a).FirstOrDefault();
            if (string.IsNullOrEmpty(clientHmacAlgorithmName))
            {
                throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientHmacAlgorithm = clientHmacAlgorithmName;

            //  Determine server hmac algorithm
            var serverHmacAlgorithmName = (from b in session.ConnectionInfo.HmacAlgorithms.Keys
                                           from a in message.MacAlgorithmsServerToClient
                                           where a == b
                                           select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverHmacAlgorithmName))
            {
                throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentServerHmacAlgorithm = serverHmacAlgorithmName;

            //  Determine compression algorithm
            var compressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                            from a in message.CompressionAlgorithmsClientToServer
                                            where a == b
                                            select a).LastOrDefault();
            if (string.IsNullOrEmpty(compressionAlgorithmName))
            {
                throw new SshConnectionException("Compression algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientCompressionAlgorithm = compressionAlgorithmName;

            //  Determine decompression algorithm
            var decompressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                              from a in message.CompressionAlgorithmsServerToClient
                                              where a == b
                                              select a).LastOrDefault();
            if (string.IsNullOrEmpty(decompressionAlgorithmName))
            {
                throw new SshConnectionException("Decompression algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentServerCompressionAlgorithm = decompressionAlgorithmName;

            _clientCipherInfo = session.ConnectionInfo.Encryptions[clientEncryptionAlgorithmName];
            _serverCipherInfo = session.ConnectionInfo.Encryptions[serverDecryptionAlgorithmName];
            _clientHashInfo = session.ConnectionInfo.HmacAlgorithms[clientHmacAlgorithmName];
            _serverHashInfo = session.ConnectionInfo.HmacAlgorithms[serverHmacAlgorithmName];
            _compressionType = session.ConnectionInfo.CompressionAlgorithms[compressionAlgorithmName];
            _decompressionType = session.ConnectionInfo.CompressionAlgorithms[decompressionAlgorithmName];
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public virtual void Finish()
        {
            //  Validate hash
            if (ValidateExchangeHash())
            {
                SendMessage(new NewKeysMessage());
            }
            else
            {
                throw new SshConnectionException("Key exchange negotiation failed.", DisconnectReason.KeyExchangeFailed);
            }
        }

        /// <summary>
        /// Creates the server side cipher to use.
        /// </summary>
        /// <returns>Server cipher.</returns>
        public Cipher CreateServerCipher()
        {
            //  Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            //  Calculate server to client initial IV
            var serverVector = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'B', sessionId));

            //  Calculate server to client encryption
            var serverKey = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'D', sessionId));

            serverKey = GenerateSessionKey(SharedKey, ExchangeHash, serverKey, _serverCipherInfo.KeySize / 8);

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating server cipher (Name:{1},Key:{2},IV:{3})",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentServerEncryption,
                                                    Session.ToHex(serverKey),
                                                    Session.ToHex(serverVector)));

            //  Create server cipher
            return _serverCipherInfo.Cipher(serverKey, serverVector);
        }

        /// <summary>
        /// Creates the client side cipher to use.
        /// </summary>
        /// <returns>Client cipher.</returns>
        public Cipher CreateClientCipher()
        {
            //  Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            //  Calculate client to server initial IV
            var clientVector = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'A', sessionId));

            //  Calculate client to server encryption
            var clientKey = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'C', sessionId));

            clientKey = GenerateSessionKey(SharedKey, ExchangeHash, clientKey, _clientCipherInfo.KeySize / 8);

            //  Create client cipher
            return _clientCipherInfo.Cipher(clientKey, clientVector);
        }

        /// <summary>
        /// Creates the server side hash algorithm to use.
        /// </summary>
        /// <returns>Hash algorithm</returns>
        public HashAlgorithm CreateServerHash()
        {
            //  Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            var serverKey = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'F', sessionId));

            serverKey = GenerateSessionKey(SharedKey, ExchangeHash, serverKey, _serverHashInfo.KeySize / 8);

            //return serverHMac;
            return _serverHashInfo.HashAlgorithm(serverKey);
        }

        /// <summary>
        /// Creates the client side hash algorithm to use.
        /// </summary>
        /// <returns>Hash algorithm</returns>
        public HashAlgorithm CreateClientHash()
        {
            //  Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            var clientKey = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'E', sessionId));
            
            clientKey = GenerateSessionKey(SharedKey, ExchangeHash, clientKey, _clientHashInfo.KeySize / 8);

            //return clientHMac;
            return _clientHashInfo.HashAlgorithm(clientKey);
        }

        /// <summary>
        /// Creates the compression algorithm to use to deflate data.
        /// </summary>
        /// <returns>Compression method.</returns>
        public Compressor CreateCompressor()
        {
            if (_compressionType == null)
                return null;

            var compressor = _compressionType.CreateInstance<Compressor>();

            compressor.Init(Session);

            return compressor;
        }

        /// <summary>
        /// Creates the compression algorithm to use to inflate data.
        /// </summary>
        /// <returns>Compression method.</returns>
        public Compressor CreateDecompressor()
        {
            if (_compressionType == null)
                return null;

            var decompressor = _decompressionType.CreateInstance<Compressor>();

            decompressor.Init(Session);

            return decompressor;
        }

        /// <summary>
        /// Determines whether the specified host key can be trusted.
        /// </summary>
        /// <param name="host">The host algorithm.</param>
        /// <returns>
        /// <c>true</c> if the specified host can be trusted; otherwise, <c>false</c>.
        /// </returns>
        protected bool CanTrustHostKey(KeyHostAlgorithm host)
        {
            var handlers = HostKeyReceived;
            if (handlers != null)
            {
                var args = new HostKeyEventArgs(host);
                handlers(this, args);
                return args.CanTrust;
            }

            return true;
        }

        /// <summary>
        /// Validates the exchange hash.
        /// </summary>
        /// <returns>true if exchange hash is valid; otherwise false.</returns>
        protected abstract bool ValidateExchangeHash();

        /// <summary>
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>Key exchange hash.</returns>
        protected abstract byte[] CalculateHash();

        /// <summary>
        /// Hashes the specified data bytes.
        /// </summary>
        /// <param name="hashData">The hash data.</param>
        /// <returns>
        /// Hashed bytes
        /// </returns>
        protected abstract byte[] Hash(byte[] hashData);

        /// <summary>
        /// Sends SSH message to the server
        /// </summary>
        /// <param name="message">The message.</param>
        protected void SendMessage(Message message)
        {
            Session.SendMessage(message);
        }

        /// <summary>
        /// Generates the session key.
        /// </summary>
        /// <param name="sharedKey">The shared key.</param>
        /// <param name="exchangeHash">The exchange hash.</param>
        /// <param name="key">The key.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private byte[] GenerateSessionKey(byte[] sharedKey, byte[] exchangeHash, byte[] key, int size)
        {
            var result = new List<byte>(key);

            while (size > result.Count)
            {
                var sessionKeyAdjustment = new SessionKeyAdjustment
                    {
                        SharedKey = sharedKey,
                        ExchangeHash = exchangeHash,
                        Key = key,
                    };

                result.AddRange(Hash(sessionKeyAdjustment.GetBytes()));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Generates the session key.
        /// </summary>
        /// <param name="sharedKey">The shared key.</param>
        /// <param name="exchangeHash">The exchange hash.</param>
        /// <param name="p">The p.</param>
        /// <param name="sessionId">The session id.</param>
        /// <returns></returns>
        private static byte[] GenerateSessionKey(byte[] sharedKey, byte[] exchangeHash, char p, byte[] sessionId)
        {
            var sessionKeyGeneration = new SessionKeyGeneration
                {
                    SharedKey = sharedKey,
                    ExchangeHash = exchangeHash,
                    Char = p,
                    SessionId = sessionId
                };
            return sessionKeyGeneration.GetBytes();
        }

        private class SessionKeyGeneration : SshData
        {
            public byte[] SharedKey { get; set; }

            public byte[] ExchangeHash { get; set; }

            public char Char { get; set; }

            public byte[] SessionId { get; set; }

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
                    capacity += 4; // SharedKey length
                    capacity += SharedKey.Length; // SharedKey
                    capacity += ExchangeHash.Length; // ExchangeHash
                    capacity += 1; // Char
                    capacity += SessionId.Length; // SessionId
                    return capacity;
                }
            }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                WriteBinaryString(SharedKey);
                Write(ExchangeHash);
                Write((byte) Char);
                Write(SessionId);
            }
        }

        private class SessionKeyAdjustment : SshData
        {
            public byte[] SharedKey { get; set; }

            public byte[] ExchangeHash { get; set; }

            public byte[] Key { get; set; }

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
                    capacity += 4; // SharedKey length
                    capacity += SharedKey.Length; // SharedKey
                    capacity += ExchangeHash.Length; // ExchangeHash
                    capacity += Key.Length; // Key
                    return capacity;
                }
            }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                WriteBinaryString(SharedKey);
                Write(ExchangeHash);
                Write(Key);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="KeyExchange"/> is reclaimed by garbage collection.
        /// </summary>
        ~KeyExchange()
        {
            Dispose(false);
        }

        #endregion
    }
}
