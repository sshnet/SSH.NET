﻿using System;
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
    /// Represents base class for different key exchange algorithm implementations.
    /// </summary>
    public abstract class KeyExchange : Algorithm, IKeyExchange
    {
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

        /// <inheritdoc/>
        public virtual void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            Session = session;

            if (sendClientInitMessage)
            {
                SendMessage(session.ClientInitMessage);
            }

            // Determine encryption algorithm
            var clientEncryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsClientToServer
                                                 where a == b
                                                 select a).FirstOrDefault();

            if (string.IsNullOrEmpty(clientEncryptionAlgorithmName))
            {
                throw new SshConnectionException("Client encryption algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientEncryption = clientEncryptionAlgorithmName;
            _clientCipherInfo = session.ConnectionInfo.Encryptions[clientEncryptionAlgorithmName];

            // Determine encryption algorithm
            var serverDecryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsServerToClient
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverDecryptionAlgorithmName))
            {
                throw new SshConnectionException("Server decryption algorithm not found", DisconnectReason.KeyExchangeFailed);
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
                if (string.IsNullOrEmpty(clientHmacAlgorithmName))
                {
                    throw new SshConnectionException("Client HMAC algorithm not found", DisconnectReason.KeyExchangeFailed);
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
                if (string.IsNullOrEmpty(serverHmacAlgorithmName))
                {
                    throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReason.KeyExchangeFailed);
                }

                session.ConnectionInfo.CurrentServerHmacAlgorithm = serverHmacAlgorithmName;
                _serverHashInfo = session.ConnectionInfo.HmacAlgorithms[serverHmacAlgorithmName];
            }

            // Determine compression algorithm
            var compressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                            from a in message.CompressionAlgorithmsClientToServer
                                            where a == b
                                            select a).FirstOrDefault();
            if (string.IsNullOrEmpty(compressionAlgorithmName))
            {
                throw new SshConnectionException("Compression algorithm not found", DisconnectReason.KeyExchangeFailed);
            }

            session.ConnectionInfo.CurrentClientCompressionAlgorithm = compressionAlgorithmName;
            _compressorFactory = session.ConnectionInfo.CompressionAlgorithms[compressionAlgorithmName];

            // Determine decompression algorithm
            var decompressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                              from a in message.CompressionAlgorithmsServerToClient
                                              where a == b
                                              select a).FirstOrDefault();
            if (string.IsNullOrEmpty(decompressionAlgorithmName))
            {
                throw new SshConnectionException("Decompression algorithm not found", DisconnectReason.KeyExchangeFailed);
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
                throw new SshConnectionException("Key exchange negotiation failed.", DisconnectReason.KeyExchangeFailed);
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

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating {1} server cipher.",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentServerEncryption));

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

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating {1} client cipher.",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentClientEncryption));

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

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating {1} server hmac algorithm.",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentServerHmacAlgorithm));

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

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating {1} client hmac algorithm.",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentClientHmacAlgorithm));

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

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating {1} client compressor.",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentClientCompressionAlgorithm));

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

            DiagnosticAbstraction.Log(string.Format("[{0}] Creating {1} server decompressor.",
                                                    Session.ToHex(Session.SessionId),
                                                    Session.ConnectionInfo.CurrentServerCompressionAlgorithm));

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
        /// <returns>true if exchange hash is valid; otherwise false.</returns>
        protected abstract bool ValidateExchangeHash();

        private protected bool ValidateExchangeHash(byte[] encodedKey, byte[] encodedSignature)
        {
            var exchangeHash = CalculateHash();

            var signatureData = new KeyHostAlgorithm.SignatureKeyData();
            signatureData.Load(encodedSignature);

            var keyAlgorithm = Session.ConnectionInfo.HostKeyAlgorithms[signatureData.AlgorithmName](encodedKey);

            Session.ConnectionInfo.CurrentHostKeyAlgorithm = signatureData.AlgorithmName;

            if (CanTrustHostKey(keyAlgorithm))
            {
                // keyAlgorithm.VerifySignature decodes the signature data before verifying.
                // But as we have already decoded the data to find the signature algorithm,
                // we just verify the decoded data directly through the DigitalSignature.
                return keyAlgorithm.DigitalSignature.Verify(exchangeHash, signatureData.Signature);
            }

            return false;
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

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="KeyExchange"/> is reclaimed by garbage collection.
        /// </summary>
        ~KeyExchange()
        {
            Dispose(disposing: false);
        }

        #endregion
    }
}
