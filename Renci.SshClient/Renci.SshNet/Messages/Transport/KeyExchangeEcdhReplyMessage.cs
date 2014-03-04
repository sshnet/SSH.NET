namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXECDH_REPLY message.
    /// </summary>
    [Message("SSH_MSG_KEXECDH_REPLY", 31)]
    public class KeyExchangeEcdhReplyMessage : Message
    {
        /// <summary>
        /// Gets a string encoding an X.509v3 certificate containing the server's ECDSA public host key
        /// </summary>
        /// <value>The host key.</value>
        public byte[] KS { get; private set; }

        /// <summary>
        /// Gets the the server's ephemeral contribution to the ECDH exchange, encoded as an octet string.
        /// </summary>
        public byte[] QS { get; private set; }

        /// <summary>
        /// Gets the an octet string containing the server's signature of the newly established exchange hash value.
        /// </summary>
        /// <value>The signature.</value>
        public byte[] Signature { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.ResetReader();
            this.KS = this.ReadBinaryString();
            this.QS = this.ReadBinaryString();
            this.Signature = this.ReadBinaryString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.WriteBinaryString(this.KS);
            this.WriteBinaryString(this.QS);
            this.WriteBinaryString(this.Signature);
        }
    }
}
