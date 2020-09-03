using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_REPLY message.
    /// </summary>
    [Message("SSH_MSG_KEX_DH_GEX_REPLY", MessageNumber)]
    internal class KeyExchangeDhGroupExchangeReply : Message
    {
        internal const byte MessageNumber = 33;

        /// <summary>
        /// Gets server public host key and certificates
        /// </summary>
        /// <value>The host key.</value>
        public byte[] HostKey { get; private set; }

        /// <summary>
        /// Gets the F value.
        /// </summary>
        public byte[] F { get; private set; }

        /// <summary>
        /// Gets the signature of H.
        /// </summary>
        /// <value>The signature.</value>
        public byte[] Signature { get; private set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // HostKey length
                capacity += HostKey.Length; // HostKey
                capacity += 4; // F length
                capacity += F.Length; // F
                capacity += 4; // Signature length
                capacity += Signature.Length; // Signature
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            HostKey = ReadBinary();
            F = ReadBinary();
            Signature = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(HostKey);
            WriteBinaryString(F);
            WriteBinaryString(Signature);
        }

        internal override void Process(Session session)
        {
            session.OnKeyExchangeDhGroupExchangeReplyReceived(this);
        }
    }
}
