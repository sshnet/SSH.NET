using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_REPLY message.
    /// </summary>
    [Message("SSH_MSG_KEX_DH_GEX_REPLY", 33)]
    internal class KeyExchangeDhGroupExchangeReply : Message
    {
        /// <summary>
        /// Gets server public host key and certificates
        /// </summary>
        /// <value>The host key.</value>
        public string HostKey { get; private set; }

        /// <summary>
        /// Gets the F value.
        /// </summary>
        public BigInteger F { get; private set; }

        /// <summary>
        /// Gets the signature of H.
        /// </summary>
        /// <value>The signature.</value>
        public string Signature { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.HostKey = this.ReadString();
            this.F = this.ReadBigInteger();
            this.Signature = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.HostKey);
            this.Write(this.F);
            this.Write(this.Signature);
        }
    }
}
