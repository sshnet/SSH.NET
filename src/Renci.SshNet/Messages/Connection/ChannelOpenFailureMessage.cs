namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_OPEN_FAILURE message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_OPEN_FAILURE", 92)]
    public class ChannelOpenFailureMessage : ChannelMessage
    {
        internal const uint AdministrativelyProhibited = 1;
        internal const uint ConnectFailed = 2;
        internal const uint UnknownChannelType = 3;
        internal const uint ResourceShortage = 4;

        private byte[] _description;
        private byte[] _language;

        /// <summary>
        /// Gets failure reason code.
        /// </summary>
        public uint ReasonCode { get; private set; }

        /// <summary>
        /// Gets description for failure.
        /// </summary>
        public string Description
        {
            get { return Utf8.GetString(_description, 0, _description.Length); }
            private set { _description = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language
        {
            get { return Utf8.GetString(_language, 0, _language.Length); }
            private set { _language = Utf8.GetBytes(value); }
        }

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
                capacity += 4; // ReasonCode
                capacity += 4; // Description length
                capacity += _description.Length; // Description
                capacity += 4; // Language length
                capacity += _language.Length; // Language
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenFailureMessage"/> class.
        /// </summary>
        public ChannelOpenFailureMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenFailureMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="description">The description.</param>
        /// <param name="reasonCode">The reason code.</param>
        public ChannelOpenFailureMessage(uint localChannelNumber, string description, uint reasonCode)
            : this(localChannelNumber, description, reasonCode, "en")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenFailureMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="description">The description.</param>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="language">The language (RFC3066).</param>
        public ChannelOpenFailureMessage(uint localChannelNumber, string description, uint reasonCode, string language)
            : base(localChannelNumber)
        {
            Description = description;
            ReasonCode = reasonCode;
            Language = language;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            ReasonCode = ReadUInt32();
            _description = ReadBinary();
            _language = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            Write(ReasonCode);
            WriteBinaryString(_description);
            WriteBinaryString(_language);
        }

        internal override void Process(Session session)
        {
            session.OnChannelOpenFailureReceived(this);
        }
    }
}
