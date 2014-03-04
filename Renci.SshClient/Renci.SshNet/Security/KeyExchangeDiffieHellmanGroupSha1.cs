using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group1-sha1" algorithm implementation.
    /// </summary>
    public abstract class KeyExchangeDiffieHellmanGroupSha1 : KeyExchangeDiffieHellman
    {
        /// <summary>
        /// Gets the group prime.
        /// </summary>
        /// <value>
        /// The group prime.
        /// </value>
        public abstract BigInteger GroupPrime { get; }

        /// <summary>
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>
        /// Key exchange hash.
        /// </returns>
        protected override byte[] CalculateHash()
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

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            this.Session.RegisterMessage("SSH_MSG_KEXDH_REPLY");

            this.Session.MessageReceived += Session_MessageReceived;

            this._prime = this.GroupPrime;

            this._group = new BigInteger(new byte[] { 2 });

            this.PopulateClientExchangeValue();

            this.SendMessage(new KeyExchangeDhInitMessage(this._clientExchangeValue));

        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
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
                this.Session.UnRegisterMessage("SSH_MSG_KEXDH_REPLY");

                this.HandleServerDhReply(message.HostKey, message.F, message.Signature);

                //  When SSH_MSG_KEXDH_REPLY received key exchange is completed
                this.Finish();
            }
        }

        private class _ExchangeHashData : SshData
        {
            public string ServerVersion { get; set; }

            public string ClientVersion { get; set; }

            public byte[] ClientPayload { get; set; }

            public byte[] ServerPayload { get; set; }

            public byte[] HostKey { get; set; }

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
                this.WriteBinaryString(this.ClientPayload);
                this.WriteBinaryString(this.ServerPayload);
                this.WriteBinaryString(this.HostKey);
                this.Write(this.ClientExchangeValue);
                this.Write(this.ServerExchangeValue);
                this.Write(this.SharedKey);
            }
        }
    }
}
