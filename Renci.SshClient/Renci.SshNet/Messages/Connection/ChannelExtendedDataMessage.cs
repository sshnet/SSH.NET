namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_EXTENDED_DATA message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_EXTENDED_DATA", 95)]
    public class ChannelExtendedDataMessage : ChannelMessage
    {
        /// <summary>
        /// Gets message data type code.
        /// </summary>
        public uint DataTypeCode { get; private set; }

        /// <summary>
        /// Gets message data.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelExtendedDataMessage"/> class.
        /// </summary>
        public ChannelExtendedDataMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelExtendedDataMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="dataTypeCode">The message data type code.</param>
        /// <param name="data">The message data.</param>
        public ChannelExtendedDataMessage(uint localChannelNumber, uint dataTypeCode, byte[] data)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.DataTypeCode = dataTypeCode;
            this.Data = data;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            this.DataTypeCode = this.ReadUInt32();
            this.Data = this.ReadBinaryString();
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.DataTypeCode);
            this.WriteBinaryString(this.Data);
        }
    }
}
