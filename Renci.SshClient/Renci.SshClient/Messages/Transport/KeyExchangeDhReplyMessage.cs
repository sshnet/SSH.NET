using System;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    internal class KeyExchangeDhReplyMessage : Message
    {
        /// <summary>
        /// Gets server public host key and certificates
        /// </summary>
        /// <value>The host key.</value>
        public string HostKey { get; private set; }

        public BigInteger F { get; private set; }

        /// <summary>
        /// Gets the signature of H.
        /// </summary>
        /// <value>The signature.</value>
        public string Signature { get; private set; }

        public override MessageTypes MessageType
        {
            get
            {
                return MessageTypes.KeyExchangeDhReply;
            }
        }

        protected override void LoadData()
        {
            this.ResetReader();
            this.HostKey = this.ReadString();
            this.F = this.ReadBigInteger();
            this.Signature = this.ReadString();
        }

        protected override void SaveData()
        {
            throw new NotSupportedException("SaveData is not supported for KeyExchangeDhReplyMessage class");
        }
    }
}
