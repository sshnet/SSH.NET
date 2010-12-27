using System;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXDH_REPLY message.
    /// </summary>
    [Message("SSH_MSG_KEXDH_REPLY", 31)]
    public class KeyExchangeDhReplyMessage : Message
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
            this.ResetReader();
            this.HostKey = this.ReadString();
            this.F = this.ReadBigInteger();
            this.Signature = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            throw new NotSupportedException("SaveData is not supported for KeyExchangeDhReplyMessage class");
        }
    }
}
