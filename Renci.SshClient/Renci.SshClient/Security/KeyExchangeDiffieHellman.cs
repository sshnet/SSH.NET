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
    internal class KeyExchangeDiffieHellman : KeyExchangeAlgorithm
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private static BigInteger _prime = new BigInteger(new byte[] { (byte)0x00,
											  (byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF, 
											  (byte)0xC9,(byte)0x0F,(byte)0xDA,(byte)0xA2,(byte)0x21,(byte)0x68,(byte)0xC2,(byte)0x34,
											  (byte)0xC4,(byte)0xC6,(byte)0x62,(byte)0x8B,(byte)0x80,(byte)0xDC,(byte)0x1C,(byte)0xD1,
											  (byte)0x29,(byte)0x02,(byte)0x4E,(byte)0x08,(byte)0x8A,(byte)0x67,(byte)0xCC,(byte)0x74,
											  (byte)0x02,(byte)0x0B,(byte)0xBE,(byte)0xA6,(byte)0x3B,(byte)0x13,(byte)0x9B,(byte)0x22,
											  (byte)0x51,(byte)0x4A,(byte)0x08,(byte)0x79,(byte)0x8E,(byte)0x34,(byte)0x04,(byte)0xDD,
											  (byte)0xEF,(byte)0x95,(byte)0x19,(byte)0xB3,(byte)0xCD,(byte)0x3A,(byte)0x43,(byte)0x1B,
											  (byte)0x30,(byte)0x2B,(byte)0x0A,(byte)0x6D,(byte)0xF2,(byte)0x5F,(byte)0x14,(byte)0x37,
											  (byte)0x4F,(byte)0xE1,(byte)0x35,(byte)0x6D,(byte)0x6D,(byte)0x51,(byte)0xC2,(byte)0x45,
											  (byte)0xE4,(byte)0x85,(byte)0xB5,(byte)0x76,(byte)0x62,(byte)0x5E,(byte)0x7E,(byte)0xC6,
											  (byte)0xF4,(byte)0x4C,(byte)0x42,(byte)0xE9,(byte)0xA6,(byte)0x37,(byte)0xED,(byte)0x6B,
											  (byte)0x0B,(byte)0xFF,(byte)0x5C,(byte)0xB6,(byte)0xF4,(byte)0x06,(byte)0xB7,(byte)0xED,
											  (byte)0xEE,(byte)0x38,(byte)0x6B,(byte)0xFB,(byte)0x5A,(byte)0x89,(byte)0x9F,(byte)0xA5,
											  (byte)0xAE,(byte)0x9F,(byte)0x24,(byte)0x11,(byte)0x7C,(byte)0x4B,(byte)0x1F,(byte)0xE6,
											  (byte)0x49,(byte)0x28,(byte)0x66,(byte)0x51,(byte)0xEC,(byte)0xE6,(byte)0x53,(byte)0x81,
											  (byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF}.Reverse().ToArray());

        private static BigInteger _group = new BigInteger(new byte[] { 2 });

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
        /// Initializes a new instance of the <see cref="KeyExchangeDiffieHellman"/> class.
        /// </summary>
        /// <param name="sessionInfo">The session information.</param>
        internal KeyExchangeDiffieHellman()
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

            CryptoPublicKey key = Settings.HostKeyAlgorithms[algorithmName]();

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
                clientExchangeValue = System.Numerics.BigInteger.ModPow(KeyExchangeDiffieHellman._group, this._randomValue, KeyExchangeDiffieHellman._prime);

            } while (clientExchangeValue < 1 || clientExchangeValue > ((KeyExchangeDiffieHellman._prime - 1)));

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
            this.SharedKey = System.Numerics.BigInteger.ModPow(message.F, this._randomValue, KeyExchangeDiffieHellman._prime);
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
