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
    public abstract class KeyExchangeAlgorithm : Algorithm
    {
        public BigInteger SharedKey { get; protected set; }

        protected Session Session { get; set; }

        private string _clientCipherTypeName;

        private string _serverCipherTypeName;

        private string _cientHmacAlgorithmTypeName;

        private string _serverHmacAlgorithmTypeName;

        private string _compressionTypeName;

        private string _decompressionTypeName;

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

        public abstract bool ValidateExchangeHash();

        protected abstract void HandleMessage<T>(T message) where T : Message;

        protected abstract IEnumerable<byte> CalculateHash();

        protected IEnumerable<byte> Hash(IEnumerable<byte> hashBytes)
        {
            using (var md = new System.Security.Cryptography.SHA1CryptoServiceProvider())
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

        public void Init(Session session, string clientEncryptionAlgorithmName, string serverDecryptionAlgorithmName, string clientHmacAlgorithmName, string serverHmacAlgorithmName, string compressionAlgorithmName, string decompressionAlgorithmName)
        {
            this.Session = session;
            this._clientCipherTypeName = session.Encryptions[clientEncryptionAlgorithmName];
            this._serverCipherTypeName = session.Encryptions[clientEncryptionAlgorithmName];
            this._cientHmacAlgorithmTypeName = session.HmacAlgorithms[clientHmacAlgorithmName];
            this._serverHmacAlgorithmTypeName = session.HmacAlgorithms[serverHmacAlgorithmName];
            this._compressionTypeName = session.CompressionAlgorithms[compressionAlgorithmName];
            this._decompressionTypeName = session.CompressionAlgorithms[decompressionAlgorithmName];

            session.MessageReceived += MessageHandler;
            session.KeyExchangeInitReceived += MessageHandler;
            session.NewKeysReceived += MessageHandler;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed by the session")]
        public Cipher CreateClientCipher()
        {
            //  Create client cipher
            var clientCipher = this._clientCipherTypeName.CreateInstance<Cipher>();

            var exchangeHash = this.ExchangeHash;

            var sharedKey = this.SharedKey;

            //  Calculate client to server initial IV
            var clientVector = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'A', this.Session.SessionId));

            //  Calculate client to server encryption
            var clientKey = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'C', this.Session.SessionId));

            clientKey = this.GenerateSessionKey(sharedKey, exchangeHash, clientKey, clientCipher.KeySize / 8);

            clientCipher.Init(clientKey, clientVector);

            return clientCipher;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed by the session")]
        public Cipher CreateServerCipher()
        {
            //  Create server cipher
            var serverCipher = this._serverCipherTypeName.CreateInstance<Cipher>();

            var exchangeHash = this.ExchangeHash;

            var sharedKey = this.SharedKey;


            //  Calculate server to client initial IV
            var serverVector = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'B', this.Session.SessionId));

            //  Calculate server to client encryption
            var serverKey = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'D', this.Session.SessionId));

            serverKey = this.GenerateSessionKey(sharedKey, exchangeHash, serverKey, serverCipher.KeySize / 8);

            serverCipher.Init(serverKey, serverVector);

            return serverCipher;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed by the session")]
        public HMac CreateClientMAC()
        {
            var MACc2s = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'E', this.Session.SessionId));
            var mac = this._cientHmacAlgorithmTypeName.CreateInstance<HMac>();
            
            mac.Init(MACc2s);
            
            return mac;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Will be disposed by the session")]
        public HMac CreateServerMAC()
        {
            var MACs2c = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'F', this.Session.SessionId));
            var mac = this._serverHmacAlgorithmTypeName.CreateInstance<HMac>();
            
            mac.Init(MACs2c);

            return mac;            
        }

        public Compressor CreateCompression()
        {
            if (string.IsNullOrEmpty(this._compressionTypeName))
            {
                return null;
            }
            var compressor = this._compressionTypeName.CreateInstance<Compressor>();

            compressor.Init(this.Session);

            return compressor;
        }

        public Compressor CreateDecompression()
        {
            if (string.IsNullOrEmpty(this._decompressionTypeName))
            {
                return null;
            }
            var compressor = this._decompressionTypeName.CreateInstance<Compressor>();

            compressor.Init(this.Session);

            return compressor;
        }

        private void MessageHandler(object sender, MessageEventArgs<Message> e)
        {
            this.HandleMessage((dynamic)e.Message);
        }

        private void MessageHandler(object sender, MessageEventArgs<KeyExchangeInitMessage> e)
        {
            this.HandleMessage((dynamic)e.Message);
        }

        private void MessageHandler(object sender, MessageEventArgs<NewKeysMessage> e)
        {
            this.Session.MessageReceived -= MessageHandler;
            this.Session.KeyExchangeInitReceived -= MessageHandler;
            this.Session.NewKeysReceived -= MessageHandler;
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
    }
}
