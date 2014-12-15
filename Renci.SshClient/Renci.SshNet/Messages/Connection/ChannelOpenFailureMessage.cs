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

#if TUNING
        private byte[] _description;
        private byte[] _language;
#endif

        /// <summary>
        /// Gets failure reason code.
        /// </summary>
        public uint ReasonCode { get; private set; }

        /// <summary>
        /// Gets description for failure.
        /// </summary>
#if TUNING
        public string Description
        {
            get { return Utf8.GetString(_description); }
            private set { _description = Utf8.GetBytes(value); }
        }
#else
        public string Description { get; private set; }
#endif

        /// <summary>
        /// Gets message language.
        /// </summary>
#if TUNING
        public string Language
        {
            get { return Utf8.GetString(_language); }
            private set { _language = Utf8.GetBytes(value); }
        }
#else
        public string Language { get; private set; }
#endif

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
                capacity += 4; // ReasonCode
                capacity += 4; // Description length
                capacity += _description.Length; // Description
                capacity += 4; // Language length
                capacity += _language.Length; // Language
                return capacity;
            }
        }
#endif

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
#if TUNING
            _description = ReadBinary();
            _language = ReadBinary();
#else
            Description = ReadString();
            Language = ReadString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            Write(ReasonCode);
#if TUNING
            WriteBinaryString(_description);
            WriteBinaryString(_language);
#else
            Write(Description ?? string.Empty);
            Write(Language ?? "en");
#endif
        }
    }
}
