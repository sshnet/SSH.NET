namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_DATA message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_DATA", 94)]
    public class ChannelDataMessage : ChannelMessage
    {
        /// <summary>
        /// Gets or sets message data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataMessage"/> class.
        /// </summary>
        public ChannelDataMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDataMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="data">Message data.</param>
        public ChannelDataMessage(uint localChannelNumber, byte[] data)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.Data = data;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            this.Data = this.ReadBinaryString();
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Data);
        }
    }
}
