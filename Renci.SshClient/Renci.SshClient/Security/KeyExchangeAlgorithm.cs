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
    internal abstract class KeyExchangeAlgorithm : Algorithm
    {
        public BigInteger SharedKey { get; protected set; }

        protected Session Session { get; set; }

        private Func<Cipher> _clientCipher;

        private Func<Cipher> _serverCipher;

        private Func<IEnumerable<byte>, HMAC> _cientHmacAlgorithm;

        private Func<IEnumerable<byte>, HMAC> _serverHmacAlgorithm;

        private Func<Session, Compressor> _compression;

        private Func<Session, Compressor> _decompression;

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
            this._clientCipher = Settings.Encryptions[clientEncryptionAlgorithmName];
            this._serverCipher = Settings.Encryptions[clientEncryptionAlgorithmName];
            this._cientHmacAlgorithm = Settings.HmacAlgorithms[clientHmacAlgorithmName];
            this._serverHmacAlgorithm = Settings.HmacAlgorithms[serverHmacAlgorithmName];
            this._compression = Settings.CompressionAlgorithms[compressionAlgorithmName];
            this._decompression = Settings.CompressionAlgorithms[decompressionAlgorithmName];

            session.MessageReceived += MessageHandler;
            session.KeyExchangeInitReceived += MessageHandler;
            session.NewKeysReceived += MessageHandler;
        }

        public Cipher CreateClientCipher()
        {
            var clientCipher = this._clientCipher();

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

        public Cipher CreateServerCipher()
        {
            //  Initilize server cipher
            var serverCipher = this._serverCipher();

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

        public HMAC CreateClientMAC()
        {
            var MACc2s = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'E', this.Session.SessionId));
            return this._cientHmacAlgorithm(MACc2s);
        }

        public HMAC CreateServerMAC()
        {
            var MACs2c = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'F', this.Session.SessionId));
            return this._serverHmacAlgorithm(MACs2c);
        }

        public Compressor CreateCompression()
        {
            return this._compression(this.Session);
        }

        public Compressor CreateDecompression()
        {
            return this._decompression(this.Session);
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
