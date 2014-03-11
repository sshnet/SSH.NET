using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    public abstract class KeyExchange : Algorithm, IDisposable
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
        public BigInteger SharedKey { get; protected set; }

        private byte[] _exchangeHash;
        /// <summary>
        /// Gets the exchange hash.
        /// </summary>
        /// <value>The exchange hash.</value>
        public byte[] ExchangeHash
        {
            get
            {
                if (this._exchangeHash == null)
                {
                    this._exchangeHash = this.CalculateHash();
                }
                return this._exchangeHash;
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
            this.Session = session;

            this.SendMessage(session.ClientInitMessage);

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

            this._clientCipherInfo = session.ConnectionInfo.Encryptions[clientEncryptionAlgorithmName];
            this._serverCipherInfo = session.ConnectionInfo.Encryptions[serverDecryptionAlgorithmName];
            this._clientHashInfo = session.ConnectionInfo.HmacAlgorithms[clientHmacAlgorithmName];
            this._serverHashInfo = session.ConnectionInfo.HmacAlgorithms[serverHmacAlgorithmName];
            this._compressionType = session.ConnectionInfo.CompressionAlgorithms[compressionAlgorithmName];
            this._decompressionType = session.ConnectionInfo.CompressionAlgorithms[decompressionAlgorithmName];
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public virtual void Finish()
        {
            //  Validate hash
            if (this.ValidateExchangeHash())
            {
                this.SendMessage(new NewKeysMessage());
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
            var sessionId = this.Session.SessionId ?? this.ExchangeHash;

            //  Calculate server to client initial IV
            var serverVector = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'B', sessionId));

            //  Calculate server to client encryption
            var serverKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'D', sessionId));

            serverKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, serverKey, this._serverCipherInfo.KeySize / 8);
            
            //  Create server cipher
            return this._serverCipherInfo.Cipher(serverKey, serverVector);
        }

        /// <summary>
        /// Creates the client side cipher to use.
        /// </summary>
        /// <returns>Client cipher.</returns>
        public Cipher CreateClientCipher()
        {
            //  Resolve Session ID
            var sessionId = this.Session.SessionId ?? this.ExchangeHash;

            //  Calculate client to server initial IV
            var clientVector = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'A', sessionId));

            //  Calculate client to server encryption
            var clientKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'C', sessionId));

            clientKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, clientKey, this._clientCipherInfo.KeySize / 8);

            //  Create client cipher
            return this._clientCipherInfo.Cipher(clientKey, clientVector);
        }

        /// <summary>
        /// Creates the server side hash algorithm to use.
        /// </summary>
        /// <returns>Hash algorithm</returns>
        public HashAlgorithm CreateServerHash()
        {
            //  Resolve Session ID
            var sessionId = this.Session.SessionId ?? this.ExchangeHash;

            var serverKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'F', sessionId));

            serverKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, serverKey, this._serverHashInfo.KeySize / 8);

            //return serverHMac;
            return this._serverHashInfo.HashAlgorithm(serverKey);
        }

        /// <summary>
        /// Creates the client side hash algorithm to use.
        /// </summary>
        /// <returns>Hash algorithm</returns>
        public HashAlgorithm CreateClientHash()
        {
            //  Resolve Session ID
            var sessionId = this.Session.SessionId ?? this.ExchangeHash;

            var clientKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'E', sessionId));
            
            clientKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, clientKey, this._clientHashInfo.KeySize / 8);

            //return clientHMac;
            return this._clientHashInfo.HashAlgorithm(clientKey);
        }

        /// <summary>
        /// Creates the compression algorithm to use to deflate data.
        /// </summary>
        /// <returns>Compression method.</returns>
        public Compressor CreateCompressor()
        {
            if (this._compressionType == null)
                return null;

            var compressor = this._compressionType.CreateInstance<Compressor>();

            compressor.Init(this.Session);

            return compressor;
        }

        /// <summary>
        /// Creates the compression algorithm to use to inflate data.
        /// </summary>
        /// <returns>Compression method.</returns>
        public Compressor CreateDecompressor()
        {
            if (this._compressionType == null)
                return null;

            var decompressor = this._decompressionType.CreateInstance<Compressor>();

            decompressor.Init(this.Session);

            return decompressor;
        }

        /// <summary>
        /// Determines whether the specified host key can be trusted.
        /// </summary>
        /// <param name="host">The host algorithm.</param>
        /// <returns>
        ///   <c>true</c> if the specified host can be trusted; otherwise, <c>false</c>.
        /// </returns>
        protected bool CanTrustHostKey(KeyHostAlgorithm host)
        {
            var args = new HostKeyEventArgs(host);

            if (this.HostKeyReceived != null)
            {
                this.HostKeyReceived(this, args);
            }

            return args.CanTrust;
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
        protected virtual byte[] Hash(byte[] hashData)
        {
            using (var sha1 = new SHA1Hash())
            {
                return sha1.ComputeHash(hashData, 0, hashData.Length);
            }
        }

        /// <summary>
        /// Sends SSH message to the server
        /// </summary>
        /// <param name="message">The message.</param>
        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

        /// <summary>
        /// Generates the session key.
        /// </summary>
        /// <param name="sharedKey">The shared key.</param>
        /// <param name="exchangeHash">The exchange hash.</param>
        /// <param name="key">The key.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private byte[] GenerateSessionKey(BigInteger sharedKey, byte[] exchangeHash, byte[] key, int size)
        {
            var result = new List<byte>(key);
            while (size > result.Count)
            {
                result.AddRange(this.Hash(new _SessionKeyAdjustment
                {
                    SharedKey = sharedKey,
                    ExcahngeHash = exchangeHash,
                    Key = key,
                }.GetBytes()));
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
        private byte[] GenerateSessionKey(BigInteger sharedKey, byte[] exchangeHash, char p, byte[] sessionId)
        {
            return new _SessionKeyGeneration
            {
                SharedKey = sharedKey,
                ExchangeHash = exchangeHash,
                Char = p,
                SessionId = sessionId,
            }.GetBytes();
        }

        private class _SessionKeyGeneration : SshData
        {
            public BigInteger SharedKey { get; set; }
            public byte[] ExchangeHash { get; set; }
            public char Char { get; set; }
            public byte[] SessionId { get; set; }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                this.Write(this.SharedKey);
                this.Write(this.ExchangeHash);
                this.Write((byte)this.Char);
                this.Write(this.SessionId);
            }
        }

        private class _SessionKeyAdjustment : SshData
        {
            public BigInteger SharedKey { get; set; }
            public byte[] ExcahngeHash { get; set; }
            public byte[] Key { get; set; }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                this.Write(this.SharedKey);
                this.Write(this.ExcahngeHash);
                this.Write(this.Key);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="KeyExchange"/> is reclaimed by garbage collection.
        /// </summary>
        ~KeyExchange()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
