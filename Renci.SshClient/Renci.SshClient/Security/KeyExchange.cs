using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Security
{
    internal class KeyExchange : Algorithm, IDisposable
    {
        private KeyExchangeAlgorithm _keyExchangeAlgorithm;

        /// <summary>
        /// Specifies negotiated algorithm to encrypt information when sent to the server
        /// </summary>
        private Func<Cipher> _clientCipher;

        /// <summary>
        /// Specifies negotiated algorithm to decrypt information from the server
        /// </summary>
        private Func<Cipher> _serverCipher;

        /// <summary>
        /// Specifies negotiated HMAC algorithm to use for client
        /// </summary>
        private Func<IEnumerable<byte>, HMAC> _clientHmacAlgorithm;

        /// <summary>
        /// Specifies negotiated HMAC algorithm to use for server
        /// </summary>
        private Func<IEnumerable<byte>, HMAC> _serverHmacAlgorithm;

        /// <summary>
        /// Gets the key exchange algorithm name.
        /// </summary>
        /// <value>Key exchange algorithm name or empty if name not yet defined.</value>
        public override string Name
        {
            get
            {
                if (this._keyExchangeAlgorithm == null)
                    return string.Empty;
                else
                    return this._keyExchangeAlgorithm.Name;
            }
        }

        private EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Gets the wait handle that signals that key exchange completed
        /// </summary>
        /// <value>The wait handle.</value>
        public EventWaitHandle WaitHandle
        {
            get
            {
                return this._waitHandle;
            }
        }

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        /// <value>The session id.</value>
        public IEnumerable<byte> SessionId { get; private set; }

        /// <summary>
        /// Gets or sets the server mac algorithm to use.
        /// </summary>
        /// <value>The server mac.</value>
        public HMAC ServerMac { get; private set; }

        /// <summary>
        /// Gets or sets the client mac algorithm to use.
        /// </summary>
        /// <value>The client mac.</value>
        public HMAC ClientMac { get; private set; }

        /// <summary>
        /// Gets or sets the client cipher algorithm to use.
        /// </summary>
        /// <value>The client cipher.</value>
        public Cipher ClientCipher { get; private set; }

        /// <summary>
        /// Gets or sets the server cipher algorithm to use.
        /// </summary>
        /// <value>The server cipher.</value>
        public Cipher ServerCipher { get; private set; }

        public Compression ServerDecompression { get; private set; }

        public Compression ClientCompression { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether key exchange is in progress.
        /// </summary>
        /// <value><c>true</c> if [in progress]; otherwise, <c>false</c>.</value>
        public bool InProgress { get; protected set; }

        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        /// <value>The session.</value>
        protected Session Session { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchange"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public KeyExchange(Session session)
        {
            this.Session = session;
            this.ServerDecompression = Compression.None;
            this.ClientCompression = Compression.None;
        }

        public void HandleMessage(KeyExchangeInitMessage message)
        {
            this._waitHandle.Reset();

            this.InProgress = true;

            this.SendMessage(this.Session.ClientInitMessage);

            var keyExchangeAlgorithm = (from s in message.KeyExchangeAlgorithms
                                        from c in Settings.KeyExchangeAlgorithms.Keys
                                        where s == c
                                        select c).FirstOrDefault();

            //  TODO:   If dont agree on algorithms then send disconnect message
            if (keyExchangeAlgorithm == null)
            {
                throw new InvalidOperationException("Failed to negotiate key exchange algorithm.");
            }

            //  Determine encryption algorithm
            var clientEncryptionAlgorithmName = (from a in message.EncryptionAlgorithmsClientToServer
                                                 from b in Settings.Encryptions.Keys
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(clientEncryptionAlgorithmName))
            {
                throw new InvalidOperationException("Client encryption algorithm not found");
            }
            this._clientCipher = Settings.Encryptions[clientEncryptionAlgorithmName];

            //  Determine encryption algorithm
            var serverDecryptionAlgorithmName = (from a in message.EncryptionAlgorithmsServerToClient
                                                 from b in Settings.Encryptions.Keys
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverDecryptionAlgorithmName))
            {
                throw new InvalidOperationException("Server decryption algorithm not found");
            }
            this._serverCipher = Settings.Encryptions[clientEncryptionAlgorithmName];

            //  Determine client hmac algorithm
            var clientHmacAlgorithmName = (from a in message.MacAlgorithmsClientToSserver
                                           from b in Settings.HmacAlgorithms.Keys
                                           where a == b
                                           select a).FirstOrDefault();
            if (string.IsNullOrEmpty(clientHmacAlgorithmName))
            {
                throw new InvalidOperationException("Server HMAC algorithm not found");
            }
            this._clientHmacAlgorithm = Settings.HmacAlgorithms[clientHmacAlgorithmName];

            //  Determine server hmac algorithm
            var serverHmacAlgorithmName = (from a in message.MacAlgorithmsServerToClient
                                           from b in Settings.HmacAlgorithms.Keys
                                           where a == b
                                           select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverHmacAlgorithmName))
            {
                throw new InvalidOperationException("Server HMAC algorithm not found");
            }
            this._serverHmacAlgorithm = Settings.HmacAlgorithms[serverHmacAlgorithmName];

            this._keyExchangeAlgorithm = Settings.KeyExchangeAlgorithms[keyExchangeAlgorithm](this.Session);

            this._keyExchangeAlgorithm.HandleMessage(message);
        }

        public void HandleMessage(NewKeysMessage message)
        {
            //  Validate hash
            var validated = this._keyExchangeAlgorithm.ValidateExchangeHash();

            if (validated)
            {
                this.SendMessage(new NewKeysMessage());
            }
            else
            {
                throw new InvalidOperationException("Key exchange negotiation failed.");
            }

            var exchangeHash = this._keyExchangeAlgorithm.ExchangeHash;
            var sharedKey = this._keyExchangeAlgorithm.SharedKey;

            //  Initialize new encryption algorithms
            if (this.SessionId == null)
            {
                this.SessionId = exchangeHash;
            }

            //  Initialize client cipher
            var clientCipher = this._clientCipher();
            //  Calculate client to server initial IV
            var clientVector = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'A', this.SessionId));

            //  Calculate client to server encryption
            var clientKey = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'C', this.SessionId));

            clientKey = this.GenerateSessionKey(sharedKey, exchangeHash, clientKey, clientCipher.KeySize / 8);

            clientCipher.Init(clientKey, clientVector);

            //  Initilize server cipher
            var serverCipher = this._serverCipher();

            //  Calculate server to client initial IV
            var serverVector = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'B', this.SessionId));

            //  Calculate server to client encryption
            var serverKey = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'D', this.SessionId));

            serverKey = this.GenerateSessionKey(sharedKey, exchangeHash, serverKey, serverCipher.KeySize / 8);

            serverCipher.Init(serverKey, serverVector);

            //  Calculate client to server integrity
            var MACc2s = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'E', this.SessionId));
            var clientMac = this._clientHmacAlgorithm(MACc2s);

            //  Calculate server to client integrity
            var MACs2c = this.Hash(this.GenerateSessionKey(sharedKey, exchangeHash, 'F', this.SessionId));
            var serverMac = this._serverHmacAlgorithm(MACs2c);

            //  TODO:   Create compression and decompression objects if any

            this.ServerCipher = serverCipher;
            this.ClientCipher = clientCipher;
            this.ServerDecompression = Compression.None;
            this.ClientCompression = Compression.None;
            this.ServerMac = serverMac;
            this.ClientMac = clientMac;

            this.InProgress = false;

            //  Signal that key exchange completed
            this._waitHandle.Set();
        }

        public void HandleMessage<T>(T message) where T : Message
        {
            this._keyExchangeAlgorithm.HandleMessage(message);
        }

        private void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

        private IEnumerable<byte> Hash(IEnumerable<byte> hashBytes)
        {
            using (var md = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, md, System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    var hashData = hashBytes.ToArray();
                    cs.Write(hashData, 0, hashData.Length);
                    cs.Close();
                    return md.Hash;
                }
            }
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

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._waitHandle != null)
                    {
                        this._waitHandle.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
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
