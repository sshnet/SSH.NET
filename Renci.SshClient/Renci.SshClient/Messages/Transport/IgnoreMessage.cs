namespace Renci.SshClient.Messages.Transport
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
        public string Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreMessage"/> class.
        /// </summary>
        public IgnoreMessage()
        {
            this.Data = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreMessage"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public IgnoreMessage(string data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.Data = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.Data);
        }
    }
}
