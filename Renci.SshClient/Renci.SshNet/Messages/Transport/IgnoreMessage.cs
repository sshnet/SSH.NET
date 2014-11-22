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
            Data = ReadBinaryString();
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
