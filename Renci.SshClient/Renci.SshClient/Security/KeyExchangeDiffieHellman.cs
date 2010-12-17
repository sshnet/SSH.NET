using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Security
{
    internal abstract class KeyExchangeDiffieHellman : KeyExchangeAlgorithm
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        protected abstract BigInteger Prime { get; }

        protected abstract BigInteger Group { get; }

        private string _clientPayload;

        private string _serverPayload;

        private BigInteger _clientExchangeValue;

        private BigInteger _serverExchangeValue;

        private string _hostKey;

        private string _signature;

        private BigInteger _randomValue;

        public override string Name
        {
            get { return "diffie-hellman-group1-sha1"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDiffieHellmanGroup1Sha1"/> class.
        /// </summary>
        /// <param name="sessionInfo">The session information.</param>
        public KeyExchangeDiffieHellman()
            : base()
        {
        }

        protected override void HandleMessage<T>(T message)
        {
            this.HandleMessage((dynamic)message);
        }

        public override bool ValidateExchangeHash()
        {
            var exchangeHash = this.CalculateHash();

            var hostKey = this._hostKey;

            var signature = this._signature;

            var bytes = hostKey.GetSshBytes();

            var length = (uint)(hostKey[0] << 24 | hostKey[1] << 16 | hostKey[2] << 8 | hostKey[3]);

            var algorithmName = bytes.Skip(4).Take((int)length).GetSshString();

            var data = bytes.Skip(4 + algorithmName.Length);

            CryptoPublicKey key = this.Session.HostKeyAlgorithms[algorithmName].CreateInstance<CryptoPublicKey>();

            key.Load(data);

            return key.VerifySignature(exchangeHash, signature.GetSshBytes());
        }

        private void HandleMessage(KeyExchangeInitMessage message)
        {
            this._serverPayload = message.GetBytes().GetSshString();
            this._clientPayload = this.Session.ClientInitMessage.GetBytes().GetSshString();
            var bytesArray = new byte[128];
            var clientExchangeValue = BigInteger.Zero;

            do
            {
                _randomizer.GetBytes(bytesArray);
                bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
                this._randomValue = new BigInteger(bytesArray);
                clientExchangeValue = System.Numerics.BigInteger.ModPow(this.Group, this._randomValue, this.Prime);

            } while (clientExchangeValue < 1 || clientExchangeValue > ((this.Prime - 1)));

            this._clientExchangeValue = clientExchangeValue;

            //  Register expected message replies
            this.Session.RegisterMessageType<KeyExchangeDhReplyMessage>(MessageTypes.KeyExchangeDhReply);

            this.SendMessage(new KeyExchangeDhInitMessage
            {
                E = this._clientExchangeValue,
            });
        }

        private void HandleMessage(KeyExchangeDhReplyMessage message)
        {
            //  Unregister message once received
            this.Session.UnRegisterMessageType(MessageTypes.KeyExchangeDhReply);

            this._serverExchangeValue = message.F;
            this._hostKey = message.HostKey;
            this.SharedKey = System.Numerics.BigInteger.ModPow(message.F, this._randomValue, this.Prime);
            this._signature = message.Signature;
        }

        protected override IEnumerable<byte> CalculateHash()
        {
            var hashData = new _ExchangeHashData
            {
                ClientVersion = this.Session.ClientVersion,
                ServerVersion = this.Session.ServerVersion,
                ClientPayload = this._clientPayload,
                ServerPayload = this._serverPayload,
                HostKey = this._hostKey,
                ClientExchangeValue = this._clientExchangeValue,
                ServerExchangeValue = this._serverExchangeValue,
                SharedKey = this.SharedKey,
            }.GetBytes();

            return this.Hash(hashData);
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
    }
}
