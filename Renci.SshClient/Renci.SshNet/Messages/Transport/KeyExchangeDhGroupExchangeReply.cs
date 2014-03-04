using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
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
        public byte[] HostKey { get; private set; }

        /// <summary>
        /// Gets the F value.
        /// </summary>
        public BigInteger F { get; private set; }

        /// <summary>
        /// Gets the signature of H.
        /// </summary>
        /// <value>The signature.</value>
        public byte[] Signature { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.HostKey = this.ReadBinaryString();
            this.F = this.ReadBigInt();
            this.Signature = this.ReadBinaryString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.WriteBinaryString(this.HostKey);
            this.Write(this.F);
            this.WriteBinaryString(this.Signature);
        }
    }
}
