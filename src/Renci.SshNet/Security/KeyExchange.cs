using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for different key exchange algorithm implementations.
    /// </summary>
    public abstract class KeyExchange : Algorithm, IKeyExchange
    {
        private ILogger _logger;
        private Func<byte[], KeyHostAlgorithm> _hostKeyAlgorithmFactory;
        private CipherInfo _clientCipherInfo;
        private CipherInfo _serverCipherInfo;
        private HashInfo _clientHashInfo;
        private HashInfo _serverHashInfo;
        private Func<Compressor> _compressorFactory;
        private Func<Compressor> _decompressorFactory;

        /// <summary>
        /// Gets the session.
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
                _exchangeHash ??= CalculateHash();

                return _exchangeHash;
            }
        }

        /// <summary>
        /// Occurs when host key received.
        /// </summary>
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchange"/> class.
        /// </summary>
        protected KeyExchange()
        {
        }

        /// <inheritdoc/>
        public virtual void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            Session = session;
            _logger = Session.SessionLoggerFactory.CreateLogger(GetType());

            if (sendClientInitMessage)
            {
                SendMessage(session.ClientInitMessage);
            }

            // Determine host key algorithm
            var hostKeyAlgorithmName = (from b in session.ConnectionInfo.HostKeyAlgorithms.Keys
                                        from a in message.ServerHostKeyAlgorithms
                                        where a == b
                                        select a).FirstOrDefault();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("[{SessionId}] Host key algorithm: we offer {WeOffer}",
                    Session.SessionIdHex,
                    session.ConnectionInfo.HostKeyAlgorithms.Keys.Join(","));

                _logger.LogTrace("[{SessionId}] Host key algorithm: they offer {TheyOffer}",
                    Session.SessionIdHex,
                    message.ServerHostKeyAlgorithms.Join(","));
            }

            if (hostKeyAlgorithmName is null)
            {
                throw new SshConnectionException(
                    $"No matching host key algorithm (server offers {message.ServerHostKeyAlgorithms.Join(",")})",
                    DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentHostKeyAlgorithm = hostKeyAlgorithmName;
            _hostKeyAlgorithmFactory = session.ConnectionInfo.HostKeyAlgorithms[hostKeyAlgorithmName];

            // Determine client encryption algorithm
            var clientEncryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsClientToServer
                                                 where a == b
                                                 select a).FirstOrDefault();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("[{SessionId}] Encryption client to server: we offer {WeOffer}",
                    Session.SessionIdHex,
                    session.ConnectionInfo.Encryptions.Keys.Join(","));

                _logger.LogTrace("[{SessionId}] Encryption client to server: they offer {TheyOffer}",
                    Session.SessionIdHex,
                    message.EncryptionAlgorithmsClientToServer.Join(","));
            }

            if (clientEncryptionAlgorithmName is null)
            {
                throw new SshConnectionException(
                    $"No matching client encryption algorithm (server offers {message.EncryptionAlgorithmsClientToServer.Join(",")})",
                    DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientEncryption = clientEncryptionAlgorithmName;
            _clientCipherInfo = session.ConnectionInfo.Encryptions[clientEncryptionAlgorithmName];

            // Determine server encryption algorithm
            var serverDecryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsServerToClient
                                                 where a == b
                                                 select a).FirstOrDefault();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("[{SessionId}] Encryption server to client: we offer {WeOffer}",
                    Session.SessionIdHex,
                    session.ConnectionInfo.Encryptions.Keys.Join(","));

                _logger.LogTrace("[{SessionId}] Encryption server to client: they offer {TheyOffer}",
                    Session.SessionIdHex,
                    message.EncryptionAlgorithmsServerToClient.Join(","));
            }

            if (serverDecryptionAlgorithmName is null)
            {
                throw new SshConnectionException(
                    $"No matching server encryption algorithm (server offers {message.EncryptionAlgorithmsServerToClient.Join(",")})",
                    DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentServerEncryption = serverDecryptionAlgorithmName;
            _serverCipherInfo = session.ConnectionInfo.Encryptions[serverDecryptionAlgorithmName];

            if (!_clientCipherInfo.IsAead)
            {
                // Determine client hmac algorithm
                var clientHmacAlgorithmName = (from b in session.ConnectionInfo.HmacAlgorithms.Keys
                                               from a in message.MacAlgorithmsClientToServer
                                               where a == b
                                               select a).FirstOrDefault();

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("[{SessionId}] MAC client to server: we offer {WeOffer}",
                        Session.SessionIdHex,
                        session.ConnectionInfo.HmacAlgorithms.Keys.Join(","));

                    _logger.LogTrace("[{SessionId}] MAC client to server: they offer {TheyOffer}",
                        Session.SessionIdHex,
                        message.MacAlgorithmsClientToServer.Join(","));
                }

                if (clientHmacAlgorithmName is null)
                {
                    throw new SshConnectionException(
                        $"No matching client MAC algorithm (server offers {message.MacAlgorithmsClientToServer.Join(",")})",
                        DisconnectReason.KeyExchangeFailed);
                }

                session.ConnectionInfo.CurrentClientHmacAlgorithm = clientHmacAlgorithmName;
                _clientHashInfo = session.ConnectionInfo.HmacAlgorithms[clientHmacAlgorithmName];
            }

            if (!_serverCipherInfo.IsAead)
            {
                // Determine server hmac algorithm
                var serverHmacAlgorithmName = (from b in session.ConnectionInfo.HmacAlgorithms.Keys
                                               from a in message.MacAlgorithmsServerToClient
                                               where a == b
                                               select a).FirstOrDefault();

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("[{SessionId}] MAC server to client: we offer {WeOffer}",
                        Session.SessionIdHex,
                        session.ConnectionInfo.HmacAlgorithms.Keys.Join(","));

                    _logger.LogTrace("[{SessionId}] MAC server to client: they offer {TheyOffer}",
                        Session.SessionIdHex,
                        message.MacAlgorithmsServerToClient.Join(","));
                }

                if (serverHmacAlgorithmName is null)
                {
                    throw new SshConnectionException(
                        $"No matching server MAC algorithm (server offers {message.MacAlgorithmsServerToClient.Join(",")})",
                        DisconnectReason.KeyExchangeFailed);
                }

                session.ConnectionInfo.CurrentServerHmacAlgorithm = serverHmacAlgorithmName;
                _serverHashInfo = session.ConnectionInfo.HmacAlgorithms[serverHmacAlgorithmName];
            }

            // Determine compression algorithm
            var compressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                            from a in message.CompressionAlgorithmsClientToServer
                                            where a == b
                                            select a).FirstOrDefault();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("[{SessionId}] Compression client to server: we offer {WeOffer}",
                    Session.SessionIdHex,
                    session.ConnectionInfo.CompressionAlgorithms.Keys.Join(","));

                _logger.LogTrace("[{SessionId}] Compression client to server: they offer {TheyOffer}",
                    Session.SessionIdHex,
                    message.CompressionAlgorithmsClientToServer.Join(","));
            }

            if (compressionAlgorithmName is null)
            {
                throw new SshConnectionException(
                    $"No matching client compression algorithm (server offers {message.CompressionAlgorithmsClientToServer.Join(",")})",
                    DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientCompressionAlgorithm = compressionAlgorithmName;
            _compressorFactory = session.ConnectionInfo.CompressionAlgorithms[compressionAlgorithmName];

            // Determine decompression algorithm
            var decompressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                              from a in message.CompressionAlgorithmsServerToClient
                                              where a == b
                                              select a).FirstOrDefault();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("[{SessionId}] Compression server to client: we offer {WeOffer}",
                    Session.SessionIdHex,
                    session.ConnectionInfo.CompressionAlgorithms.Keys.Join(","));

                _logger.LogTrace("[{SessionId}] Compression server to client: they offer {TheyOffer}",
                    Session.SessionIdHex,
                    message.CompressionAlgorithmsServerToClient.Join(","));
            }

            if (decompressionAlgorithmName is null)
            {
                throw new SshConnectionException(
                    $"No matching server compression algorithm (server offers {message.CompressionAlgorithmsServerToClient.Join(",")})",
                    DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentServerCompressionAlgorithm = decompressionAlgorithmName;
            _decompressorFactory = session.ConnectionInfo.CompressionAlgorithms[decompressionAlgorithmName];
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public virtual void Finish()
        {
            if (!ValidateExchangeHash())
            {
                throw new SshConnectionException("Host key could not be verified.", DisconnectReason.KeyExchangeFailed);
            }

            SendMessage(new NewKeysMessage());
        }

        /// <summary>
        /// Creates the server side cipher to use.
        /// </summary>
        /// <param name="isAead"><see langword="true"/> to indicate the cipher is AEAD, <see langword="false"/> to indicate the cipher is not AEAD.</param>
        /// <returns>Server cipher.</returns>
        public Cipher CreateServerCipher(out bool isAead)
        {
            isAead = _serverCipherInfo.IsAead;

            // Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            // Calculate server to client initial IV
            var serverVector = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'B', sessionId));

            // Calculate server to client encryption
            var serverKey = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'D', sessionId));

            serverKey = GenerateSessionKey(SharedKey, ExchangeHash, serverKey, _serverCipherInfo.KeySize / 8);

            _logger.LogDebug("[{SessionId}] Creating {ServerEncryption} server cipher.",
                                                    Session.SessionIdHex,
                                                    Session.ConnectionInfo.CurrentServerEncryption);

            // Create server cipher
            return _serverCipherInfo.Cipher(serverKey, serverVector);
        }

        /// <summary>
        /// Creates the client side cipher to use.
        /// </summary>
        /// <param name="isAead"><see langword="true"/> to indicate the cipher is AEAD, <see langword="false"/> to indicate the cipher is not AEAD.</param>
        /// <returns>Client cipher.</returns>
        public Cipher CreateClientCipher(out bool isAead)
        {
            isAead = _clientCipherInfo.IsAead;

            // Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            // Calculate client to server initial IV
            var clientVector = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'A', sessionId));

            // Calculate client to server encryption
            var clientKey = Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'C', sessionId));

            clientKey = GenerateSessionKey(SharedKey, ExchangeHash, clientKey, _clientCipherInfo.KeySize / 8);

            _logger.LogDebug("[{SessionId}] Creating {ClientEncryption} client cipher.",
                                                    Session.SessionIdHex,
                                                    Session.ConnectionInfo.CurrentClientEncryption);

            // Create client cipher
            return _clientCipherInfo.Cipher(clientKey, clientVector);
        }

        /// <summary>
        /// Creates the server side hash algorithm to use.
        /// </summary>
        /// <param name="isEncryptThenMAC"><see langword="true"/> to enable encrypt-then-MAC, <see langword="false"/> to use encrypt-and-MAC.</param>
        /// <returns>
        /// The server-side hash algorithm.
        /// </returns>
        public HashAlgorithm CreateServerHash(out bool isEncryptThenMAC)
        {
            if (_serverHashInfo == null)
            {
                isEncryptThenMAC = false;
                return null;
            }

            isEncryptThenMAC = _serverHashInfo.IsEncryptThenMAC;

            // Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            var serverKey = GenerateSessionKey(SharedKey,
                                               ExchangeHash,
                                               Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'F', sessionId)),
                                               _serverHashInfo.KeySize / 8);

            _logger.LogDebug("[{SessionId}] Creating {ServerHmacAlgorithm} server hmac algorithm.",
                                                    Session.SessionIdHex,
                                                    Session.ConnectionInfo.CurrentServerHmacAlgorithm);

            return _serverHashInfo.HashAlgorithm(serverKey);
        }

        /// <summary>
        /// Creates the client side hash algorithm to use.
        /// </summary>
        /// <param name="isEncryptThenMAC"><see langword="true"/> to enable encrypt-then-MAC, <see langword="false"/> to use encrypt-and-MAC.</param>
        /// <returns>
        /// The client-side hash algorithm.
        /// </returns>
        public HashAlgorithm CreateClientHash(out bool isEncryptThenMAC)
        {
            if (_clientHashInfo == null)
            {
                isEncryptThenMAC = false;
                return null;
            }

            isEncryptThenMAC = _clientHashInfo.IsEncryptThenMAC;

            // Resolve Session ID
            var sessionId = Session.SessionId ?? ExchangeHash;

            var clientKey = GenerateSessionKey(SharedKey,
                                               ExchangeHash,
                                               Hash(GenerateSessionKey(SharedKey, ExchangeHash, 'E', sessionId)),
                                               _clientHashInfo.KeySize / 8);

            _logger.LogDebug("[{SessionId}] Creating {ClientHmacAlgorithm} client hmac algorithm.",
                                                    Session.SessionIdHex,
                                                    Session.ConnectionInfo.CurrentClientHmacAlgorithm);

            return _clientHashInfo.HashAlgorithm(clientKey);
        }

        /// <summary>
        /// Creates the compression algorithm to use to deflate data.
        /// </summary>
        /// <returns>
        /// The compression method.
        /// </returns>
        public Compressor CreateCompressor()
        {
            if (_compressorFactory is null)
            {
                return null;
            }

            _logger.LogDebug("[{SessionId}] Creating {CompressionAlgorithm} client compressor.",
                                                    Session.SessionIdHex,
                                                    Session.ConnectionInfo.CurrentClientCompressionAlgorithm);

            var compressor = _compressorFactory();

            compressor.Init(Session);

            return compressor;
        }

        /// <summary>
        /// Creates the compression algorithm to use to inflate data.
        /// </summary>
        /// <returns>
        /// The decompression method.
        /// </returns>
        public Compressor CreateDecompressor()
        {
            if (_decompressorFactory is null)
            {
                return null;
            }

            _logger.LogDebug("[{SessionId}] Creating {ServerCompressionAlgorithm} server decompressor.",
                                                    Session.SessionIdHex,
                                                    Session.ConnectionInfo.CurrentServerCompressionAlgorithm);

            var decompressor = _decompressorFactory();

            decompressor.Init(Session);

            return decompressor;
        }

        /// <summary>
        /// Determines whether the specified host key can be trusted.
        /// </summary>
        /// <param name="host">The host algorithm.</param>
        /// <returns>
        /// <see langword="true"/> if the specified host can be trusted; otherwise, <see langword="false"/>.
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
        /// <returns><see langword="true"/> if exchange hash is valid; otherwise <see langword="false"/>.</returns>
        protected abstract bool ValidateExchangeHash();

        private protected bool ValidateExchangeHash(byte[] encodedKey, byte[] encodedSignature)
        {
            var exchangeHash = CalculateHash();

            var keyAlgorithm = _hostKeyAlgorithmFactory(encodedKey);

            return keyAlgorithm.VerifySignature(exchangeHash, encodedSignature) && CanTrustHostKey(keyAlgorithm);
        }

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
        /// The hash of the data.
        /// </returns>
        protected abstract byte[] Hash(byte[] hashData);

        /// <summary>
        /// Sends SSH message to the server.
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
        /// <returns>
        /// The session key.
        /// </returns>
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
        /// <returns>
        /// The session key.
        /// </returns>
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

        private sealed class SessionKeyGeneration : SshData
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
                Write((byte)Char);
                Write(SessionId);
            }
        }

        private sealed class SessionKeyAdjustment : SshData
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
