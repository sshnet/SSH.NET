namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_IGNORE message.
    /// </summary>
    [Message("SSH_MSG_IGNORE", 2)]
    public class IgnoreMessage : Message
    {
        /// <summary>
        /// Gets ignore message data if any.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreMessage"/> class.
        /// </summary>
        public IgnoreMessage()
        {
            Data = new byte[] { };
        }

#if TUNING
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
                capacity += 4; // Data length
                capacity += Data.Length; // Data
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreMessage"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public IgnoreMessage(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
#if TUNING
            Data = ReadBinary();
#else
            Data = ReadBinaryString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(Data);
        }
    }
}
