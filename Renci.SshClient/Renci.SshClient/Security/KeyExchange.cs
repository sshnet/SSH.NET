using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Common;
using Renci.SshClient.Compression;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Security
{
    public abstract class KeyExchange : Algorithm, IDisposable
    {
        private Type _clientCipherType;

        private Type _serverCipherType;

        private Type _cientHmacAlgorithmType;

        private Type _serverHmacAlgorithmType;

        private Type _compressionType;

        private Type _decompressionType;

        protected Session Session { get; set; }

        public BigInteger SharedKey { get; protected set; }

        private IEnumerable<byte> _exchangeHash;
        /// <summary>
        /// Gets the exchange hash.
        /// </summary>
        /// <value>The exchange hash.</value>
        public IEnumerable<byte> ExchangeHash
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

        public Cipher ServerCipher { get; private set; }

        public Cipher ClientCipher { get; private set; }

        public HMac ServerHMac { get; private set; }

        public HMac ClientHMac { get; private set; }

        public Compressor Compressor { get; private set; }

        public Compressor Decompressor { get; private set; }

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
                throw new SshConnectionException("Client encryption algorithm not found", DisconnectReasons.KeyExchangeFailed);
            }

            //  Determine encryption algorithm
            var serverDecryptionAlgorithmName = (from b in session.ConnectionInfo.Encryptions.Keys
                                                 from a in message.EncryptionAlgorithmsServerToClient
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverDecryptionAlgorithmName))
            {
                throw new SshConnectionException("Server decryption algorithm not found", DisconnectReasons.KeyExchangeFailed);
            }

            //  Determine client hmac algorithm
            var clientHmacAlgorithmName = (from b in session.ConnectionInfo.HmacAlgorithms.Keys
                                           from a in message.MacAlgorithmsClientToSserver
                                           where a == b
                                           select a).FirstOrDefault();
            if (string.IsNullOrEmpty(clientHmacAlgorithmName))
            {
                throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReasons.KeyExchangeFailed);
            }

            //  Determine server hmac algorithm
            var serverHmacAlgorithmName = (from b in session.ConnectionInfo.HmacAlgorithms.Keys
                                           from a in message.MacAlgorithmsServerToClient
                                           where a == b
                                           select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverHmacAlgorithmName))
            {
                throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReasons.KeyExchangeFailed);
            }

            //  Determine compression algorithm
            var compressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                            from a in message.CompressionAlgorithmsClientToServer
                                            where a == b
                                            select a).FirstOrDefault();
            if (string.IsNullOrEmpty(compressionAlgorithmName))
            {
                throw new SshConnectionException("Compression algorithm not found", DisconnectReasons.KeyExchangeFailed);
            }

            //  Determine decompression algorithm
            var decompressionAlgorithmName = (from b in session.ConnectionInfo.CompressionAlgorithms.Keys
                                              from a in message.CompressionAlgorithmsServerToClient
                                              where a == b
                                              select a).FirstOrDefault();
            if (string.IsNullOrEmpty(decompressionAlgorithmName))
            {
                throw new SshConnectionException("Decompression algorithm not found", DisconnectReasons.KeyExchangeFailed);
            }

            this._clientCipherType = session.ConnectionInfo.Encryptions[clientEncryptionAlgorithmName];
            this._serverCipherType = session.ConnectionInfo.Encryptions[clientEncryptionAlgorithmName];
            this._cientHmacAlgorithmType = session.ConnectionInfo.HmacAlgorithms[clientHmacAlgorithmName];
            this._serverHmacAlgorithmType = session.ConnectionInfo.HmacAlgorithms[serverHmacAlgorithmName];
            this._compressionType = session.ConnectionInfo.CompressionAlgorithms[compressionAlgorithmName];
            this._decompressionType = session.ConnectionInfo.CompressionAlgorithms[decompressionAlgorithmName];
        }

        public virtual void Finish()
        {
            //  Validate hash
            var validated = this.ValidateExchangeHash();

            if (validated)
            {
                this.SendMessage(new NewKeysMessage());
            }
            else
            {
                throw new SshConnectionException("Key exchange negotiation failed.", DisconnectReasons.KeyExchangeFailed);
            }

            //  Create server cipher
            this.ServerCipher = this._serverCipherType.CreateInstance<Cipher>();

            //  Calculate server to client initial IV
            var serverVector = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'B', this.Session.SessionId));

            //  Calculate server to client encryption
            var serverKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'D', this.Session.SessionId));

            serverKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, serverKey, this.ServerCipher.KeySize / 8);

            this.ServerCipher.Init(serverKey, serverVector);

            //  Create client cipher
            this.ClientCipher = this._clientCipherType.CreateInstance<Cipher>();

            //  Calculate client to server initial IV
            var clientVector = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'A', this.Session.SessionId));

            //  Calculate client to server encryption
            var clientKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'C', this.Session.SessionId));

            clientKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, clientKey, this.ClientCipher.KeySize / 8);

            this.ClientCipher.Init(clientKey, clientVector);

            //  Create server HMac
            this.ServerHMac = this._serverHmacAlgorithmType.CreateInstance<HMac>();

            this.ServerHMac.Init(this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'F', this.Session.SessionId)));

            //  Create client HMac
            this.ClientHMac = this._cientHmacAlgorithmType.CreateInstance<HMac>();

            this.ClientHMac.Init(this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'E', this.Session.SessionId)));

            if (this._compressionType != null)
            {
                var compressor = this._compressionType.CreateInstance<Compressor>();

                compressor.Init(this.Session);

                this.Compressor = compressor;
            }

            if (this._decompressionType != null)
            {
                var decompressor = this._decompressionType.CreateInstance<Compressor>();

                decompressor.Init(this.Session);

                this.Decompressor = decompressor;
            }
        }

        protected abstract bool ValidateExchangeHash();

        protected abstract IEnumerable<byte> CalculateHash();

        protected virtual IEnumerable<byte> Hash(IEnumerable<byte> hashBytes)
        {
            using (var md = new SHA1CryptoServiceProvider())
            {
                using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, md, System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    var hashData = hashBytes.ToArray();
                    cs.Write(hashData, 0, hashData.Length);
                }
                return md.Hash;
            }
        }

        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

        private IEnumerable<byte> GenerateSessionKey(BigInteger sharedKey, IEnumerable<byte> exchangeHash, IEnumerable<byte> key, int size)
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

            return result;
        }

        private IEnumerable<byte> GenerateSessionKey(BigInteger sharedKey, IEnumerable<byte> exchangeHash, char p, IEnumerable<byte> sessionId)
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
            public IEnumerable<byte> ExchangeHash { get; set; }
            public char Char { get; set; }
            public IEnumerable<byte> SessionId { get; set; }

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
            public IEnumerable<byte> ExcahngeHash { get; set; }
            public IEnumerable<byte> Key { get; set; }

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

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.ServerCipher != null)
                    {
                        this.ServerCipher.Dispose();
                    }

                    if (this.ClientCipher != null)
                    {
                        this.ClientCipher.Dispose();
                    }

                    if (this.ServerHMac != null)
                    {
                        this.ServerHMac.Dispose();
                    }

                    if (this.ClientHMac != null)
                    {
                        this.ClientHMac.Dispose();
                    }
                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

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
