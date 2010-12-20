using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    internal class KeyExchangeDhGroupExchangeReply : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.KeyExchangeDhGroupExchangeReply; }
        }

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

        protected override void LoadData()
        {
            this.HostKey = this.ReadString();
            this.F = this.ReadBigInteger();
            this.Signature = this.ReadString();
        }

        protected override void SaveData()
        {
            this.Write(this.HostKey);
            this.Write(this.F);
            this.Write(this.Signature);
        }
    }
}
