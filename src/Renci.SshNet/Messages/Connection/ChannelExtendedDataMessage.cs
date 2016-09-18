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
                capacity += 4; // DataTypeCode
                capacity += 4; // Data length
                capacity += Data.Length; // Data
                return capacity;
            }
        }

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
            : base(localChannelNumber)
        {
            DataTypeCode = dataTypeCode;
            Data = data;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            DataTypeCode = ReadUInt32();
            Data = ReadBinary();
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            Write(DataTypeCode);
            WriteBinaryString(Data);
        }

        internal override void Process(Session session)
        {
            session.OnChannelExtendedDataReceived(this);
        }
    }
}
