using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Algorithms
{
    internal abstract class KeyExchange : Algorithm
    {
        /// <summary>
        /// Creates the key exchange algorithm to be used for key exchange.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        internal static KeyExchange Create(KeyExchangeInitMessage message, SessionInfo sessionInfo)
        {

            //  TODO:   Determine key exchange algorithm
            var keyExchangeAlgorithm = (from s in message.KeyExchangeAlgorithms
                                        from c in Settings.KeyExchangeAlgorithms.Keys
                                        where s == c
                                        select c).FirstOrDefault();

            //  TODO:   If dont agree on algorithms then send disconnect message
            if (keyExchangeAlgorithm == null)
            {
                throw new InvalidDataException("Failed to negotiate key exchange algorithm.");
            }

            return Settings.KeyExchangeAlgorithms[keyExchangeAlgorithm](sessionInfo);
        }

        /// <summary>
        /// Specifies negotiated algorithm to encrypt information when sent to the server
        /// </summary>
        private Func<SymmetricAlgorithm> _clientEncryptionAlgorithm;

        /// <summary>
        /// Specifies negotiated algorithm to decrypt information from the server
        /// </summary>
        private Func<SymmetricAlgorithm> _serverDecryptionAlgorithm;

        /// <summary>
        /// Specifies negotiated HMAC algorithm to use for client
        /// </summary>
        private Func<IEnumerable<byte>, HMAC> _clientHmacAlgorithm;

        /// <summary>
        /// Specifies negotiated HMAC algorithm to use for server
        /// </summary>
        private Func<IEnumerable<byte>, HMAC> _serverHmacAlgorithm;

        private IEnumerable<byte> _exchangeHash;
        /// <summary>
        /// Gets hash value
        /// </summary>
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

        public HMAC ServerMac { get; set; }

        public HMAC ClientMac { get; set; }

        public ICryptoTransform Encryption { get; set; }

        public ICryptoTransform Decryption { get; set; }

        public Compression ServerDecompression { get; set; }

        public Compression ClientCompression { get; set; }

        public bool IsCompleted { get; protected set; }

        public bool IsSuccessed { get; protected set; }

        protected SessionInfo SessionInfo { get; private set; }

        protected string ClientPayload { get; set; }

        protected string ServerPayload { get; set; }

        protected string HostKey { get; set; }

        protected BigInteger ClientExchangeValue { get; set; }

        protected BigInteger ServerExchangeValue { get; set; }

        protected BigInteger SharedKey { get; set; }

        protected string Signature { get; set; }

        public event EventHandler<KeyExchangeCompletedEventArgs> Completed;

        public event EventHandler<KeyExchangeFailedEventArgs> Failed;

        public KeyExchange(SessionInfo sessionInfo)
        {
            this.SessionInfo = sessionInfo;
            this.ServerDecompression = Compression.None;
            this.ClientCompression = Compression.None;
        }

        public virtual void Start()
        {
            //  TODO:   If key exchange initiated by the client no need to send client message again
            var clientMessage = new KeyExchangeInitMessage()
            {
                KeyExchangeAlgorithms = Settings.KeyExchangeAlgorithms.Keys,
                ServerHostKeyAlgorithms = Settings.HostKeyAlgorithms.Keys,
                EncryptionAlgorithmsClientToServer = Settings.Encryptions.Keys,
                EncryptionAlgorithmsServerToClient = Settings.Encryptions.Keys,
                MacAlgorithmsClientToSserver = Settings.HmacAlgorithms.Keys,
                MacAlgorithmsServerToClient = Settings.HmacAlgorithms.Keys,
                CompressionAlgorithmsClientToServer = new string[] { "none" },
                CompressionAlgorithmsServerToClient = new string[] { "none" },
                LanguagesClientToServer = new string[] { string.Empty },
                LanguagesServerToClient = new string[] { string.Empty },
                FirstKexPacketFollows = false,
                Reserved = 0,
            };

            this.ClientPayload = clientMessage.GetBytes().GetSshString();

            this.SendMessage(clientMessage);
        }

        public virtual void Start(KeyExchangeInitMessage message)
        {
            this.Start();

            //  Determine encryption algorithm
            var clientEncryptionAlgorithmName = (from a in message.EncryptionAlgorithmsClientToServer
                                                 from b in Settings.Encryptions.Keys
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(clientEncryptionAlgorithmName))
            {
                throw new InvalidOperationException("Client encryption algorithm not found");
            }
            this._clientEncryptionAlgorithm = Settings.Encryptions[clientEncryptionAlgorithmName];

            //  Determine encryption algorithm
            var serverDecryptionAlgorithmName = (from a in message.EncryptionAlgorithmsServerToClient
                                                 from b in Settings.Encryptions.Keys
                                                 where a == b
                                                 select a).FirstOrDefault();
            if (string.IsNullOrEmpty(serverDecryptionAlgorithmName))
            {
                throw new InvalidOperationException("Server decryption algorithm not found");
            }
            this._serverDecryptionAlgorithm = Settings.Encryptions[clientEncryptionAlgorithmName];

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

        }

        public virtual void Finish()
        {
            //  TODO:   Validate that all required properties are set
            if (this.SessionInfo.SessionId == null)
            {
                this.SessionInfo.SessionId = this.ExchangeHash;
            }

            //  Set encryption 
            ICryptoTransform encryption;
            using (var clientAlgorithm = this._clientEncryptionAlgorithm())
            {
                //  Calculate client to server initial IV
                var clientValue = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'A', this.SessionInfo.SessionId));

                //  Calculate client to server encryption
                var clientKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'C', this.SessionInfo.SessionId));

                clientKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, clientKey, clientAlgorithm.KeySize / 8);

                clientAlgorithm.Mode = System.Security.Cryptography.CipherMode.CBC;
                clientAlgorithm.Padding = System.Security.Cryptography.PaddingMode.None;

                encryption = clientAlgorithm.CreateEncryptor(clientKey.Take(clientAlgorithm.KeySize / 8).ToArray(), clientValue.Take(clientAlgorithm.BlockSize / 8).ToArray());
            }

            //  Set decryption
            ICryptoTransform decryption;
            using (var serverAlgorithm = this._serverDecryptionAlgorithm())
            {
                //  Calculate server to client initial IV
                var serverValue = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'B', this.SessionInfo.SessionId));

                //  Calculate server to client encryption
                var serverKey = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'D', this.SessionInfo.SessionId));

                serverKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, serverKey, serverAlgorithm.KeySize / 8);

                serverAlgorithm.Mode = System.Security.Cryptography.CipherMode.CBC;
                serverAlgorithm.Padding = System.Security.Cryptography.PaddingMode.None;

                decryption = serverAlgorithm.CreateDecryptor(serverKey.Take(serverAlgorithm.KeySize / 8).ToArray(), serverValue.Take(serverAlgorithm.BlockSize / 8).ToArray());
            }

            //  Calculate client to server integrity
            var MACc2s = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'E', this.SessionInfo.SessionId));
            var clientMac = this._clientHmacAlgorithm(MACc2s);

            //  Calculate server to client integrity
            var MACs2c = this.Hash(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'F', this.SessionInfo.SessionId));
            var serverMac = this._serverHmacAlgorithm(MACs2c);

            //  TODO:   Create compression and decompression objects if any

            this.Decryption = decryption;
            this.Encryption = encryption;
            this.ServerDecompression = Compression.None;
            this.ClientCompression = Compression.None;
            this.ServerMac = serverMac;
            this.ClientMac = clientMac;

            this.IsCompleted = true;
            this.RaiseCompleted();
        }

        /// <summary>
        /// Raises the Completed event.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="decryption">The decryption to be used.</param>
        /// <param name="encryption">The encryption to be used.</param>
        /// <param name="serverDecompression">The server decompression.</param>
        /// <param name="clientCompression">The client compression.</param>
        /// <param name="serverMac">The server mac.</param>
        /// <param name="clientMac">The client mac.</param>
        protected void RaiseCompleted()
        {
            if (this.Completed != null)
            {
                this.Completed(this, new KeyExchangeCompletedEventArgs());
            }
        }

        /// <summary>
        /// Raises the Failed event.
        /// </summary>
        /// <param name="message">The fail reason message.</param>
        protected void RaiseFailed(string message)
        {
            if (this.Failed != null)
            {
                this.Failed(this, new KeyExchangeFailedEventArgs(message));
            }
        }

        protected virtual IEnumerable<byte> Hash(IEnumerable<byte> hashBytes)
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

        protected bool ValidateExchangeHash()
        {
            var bytes = this.HostKey.GetSshBytes();

            var length = BitConverter.ToUInt32(bytes.Take(4).Reverse().ToArray(), 0);

            var algorithmName = bytes.Skip(4).Take((int)length).GetSshString();

            var data = bytes.Skip(4 + algorithmName.Length);

            var signature = Settings.HostKeyAlgorithms[algorithmName](data);

            return signature.ValidateSignature(this.ExchangeHash, this.Signature.GetSshBytes());
        }

        protected void SendMessage(Message message)
        {
            this.SessionInfo.SendMessage(message);
        }

        private IEnumerable<byte> CalculateHash()
        {
            var hashData = new _ExchangeHashData
            {
                ClientVersion = this.SessionInfo.ClientVersion,
                ServerVersion = this.SessionInfo.ServerVersion,
                ClientPayload = this.ClientPayload,
                ServerPayload = this.ServerPayload,
                HostKey = this.HostKey,
                ClientExchangeValue = this.ClientExchangeValue,
                ServerExchangeValue = this.ServerExchangeValue,
                SharedKey = this.SharedKey,
            }.GetBytes();

            return this.Hash(hashData);
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

        private class _ExchangeHashData : SshData
        {
            public string ServerVersion { get; set; }

            public string ClientVersion { get; set; }

            public string ClientPayload { get; set; }

            public string ServerPayload { get; set; }

            public string HostKey { get; set; }

            public UInt32? MinimumGroupSize { get; set; }

            public UInt32? PreferredGroupSize { get; set; }

            public UInt32? MaximumGroupSize { get; set; }

            public IEnumerable<byte> Prime { get; set; }

            public BigInteger ClientExchangeValue { get; set; }

            public BigInteger ServerExchangeValue { get; set; }

            public BigInteger SharedKey { get; set; }

            protected override void LoadData()
            {
                throw new System.NotImplementedException();
            }

            protected override void SaveData()
            {
                this.Write(this.ClientVersion);
                this.Write(this.ServerVersion);
                this.Write(this.ClientPayload);
                this.Write(this.ServerPayload);
                this.Write(this.HostKey);
                if (this.MinimumGroupSize.HasValue)
                    this.Write(this.MinimumGroupSize.Value);
                if (this.PreferredGroupSize.HasValue)
                    this.Write(this.PreferredGroupSize.Value);
                if (this.MaximumGroupSize.HasValue)
                    this.Write(this.MaximumGroupSize.Value);
                if (this.Prime != null)
                    this.Write(this.Prime);
                this.Write(this.ClientExchangeValue);
                this.Write(this.ServerExchangeValue);
                this.Write(this.SharedKey);
            }
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
