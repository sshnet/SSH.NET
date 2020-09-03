namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXDH_INIT message.
    /// </summary>
    [Message("SSH_MSG_KEXDH_INIT", 30)]
    internal class KeyExchangeDhInitMessage : Message, IKeyExchangedAllowed
    {
        /// <summary>
        /// Gets the E value.
        /// </summary>
        public byte[] E { get; private set; }

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
                capacity += 4; // E length
                capacity += E.Length; // E
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDhInitMessage"/> class.
        /// </summary>
        /// <param name="clientExchangeValue">The client exchange value.</param>
        public KeyExchangeDhInitMessage(byte[] clientExchangeValue)
        {
            E = clientExchangeValue;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            E = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(E);
        }

        internal override void Process(Session session)
        {
            throw new System.NotImplementedException();
        }
    }
}
