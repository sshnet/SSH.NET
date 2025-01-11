using System;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_HYBRID_INIT message.
    /// </summary>
    internal sealed class KeyExchangeHybridInitMessage : Message, IKeyExchangedAllowed
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_KEX_HYBRID_INIT";
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
        /// Gets the client init data.
        /// </summary>
        /// <remarks>
        /// The init data is the concatenation of C_PK2 and C_PK1 (C_INIT = C_PK2 || C_PK1, where || depicts concatenation).
        /// C_PK1 and C_PK2 represent the ephemeral client public keys used for each key exchange of the PQ/T Hybrid mechanism.
        /// Typically, C_PK1 represents a traditional / classical (i.e., ECDH) key exchange public key.
        /// C_PK2 represents the 'pk' output of the corresponding post-quantum KEM's 'KeyGen' at the client.
        /// </remarks>
        public byte[] CInit { get; private set; }

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
                capacity += 4; // CInit length
                capacity += CInit.Length; // CInit
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeHybridInitMessage"/> class.
        /// </summary>
        public KeyExchangeHybridInitMessage(byte[] init)
        {
            CInit = init;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            CInit = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(CInit);
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
