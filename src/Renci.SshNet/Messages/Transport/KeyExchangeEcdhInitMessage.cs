using System;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXECDH_INIT message.
    /// </summary>
    internal sealed class KeyExchangeEcdhInitMessage : Message, IKeyExchangedAllowed
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_KEX_ECDH_INIT";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 30;
            }
        }

        /// <summary>
        /// Gets the client's ephemeral contribution to the ECDH exchange, encoded as an octet string.
        /// </summary>
        public byte[] QC { get; private set; }

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
                capacity += 4; // QC length
                capacity += QC.Length; // QC
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeEcdhInitMessage"/> class.
        /// </summary>
        public KeyExchangeEcdhInitMessage(byte[] q)
        {
            QC = q;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            QC = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(QC);
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
