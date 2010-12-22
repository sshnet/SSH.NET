using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;
using System.Globalization;

namespace Renci.SshClient.Security
{
    internal class KeyExchangeDiffieHellmanGroup1Sha1 : KeyExchangeDiffieHellman
    {
        public override string Name
        {
            get { return "diffie-hellman-group1-sha1"; }
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

        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            this.Session.RegisterMessageType<KeyExchangeDhReplyMessage>(MessageTypes.KeyExchangeDhReply);

            this.Session.MessageReceived += Session_MessageReceived;

            BigInteger prime;
            var secondOkleyGroup = @"00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE65381FFFFFFFFFFFFFFFF";
            BigInteger.TryParse(secondOkleyGroup, System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out prime);

            this._prime = prime;

            this._group = new BigInteger(new byte[] { 2 });

            this.PopulateClientExchangeValue();

            this.Session.SendMessage(new KeyExchangeDhInitMessage(this._clientExchangeValue));

        }
        
        public override void Finish()
        {
            base.Finish();

            this.Session.MessageReceived -= Session_MessageReceived;
        }

        private void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            var message = e.Message as KeyExchangeDhReplyMessage;
            if (message != null)
            {
                //  Unregister message once received
                this.Session.UnRegisterMessageType(MessageTypes.KeyExchangeDhReply);

                this.HandleServerDhReply(message.HostKey, message.F, message.Signature);
            }
        }

        private class _ExchangeHashData : SshData
        {
            public string ServerVersion { get; set; }

            public string ClientVersion { get; set; }

            public string ClientPayload { get; set; }

            public string ServerPayload { get; set; }

            public string HostKey { get; set; }

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
                this.Write(this.ClientExchangeValue);
                this.Write(this.ServerExchangeValue);
                this.Write(this.SharedKey);
            }
        }
    }
}
