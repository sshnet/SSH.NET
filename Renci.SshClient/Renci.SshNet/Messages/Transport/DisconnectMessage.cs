namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_DISCONNECT message.
    /// </summary>
    [Message("SSH_MSG_DISCONNECT", 1)]
    public class DisconnectMessage : Message, IKeyExchangedAllowed
    {
#if TUNING
        private byte[] _description;
        private byte[] _language;
#endif

        /// <summary>
        /// Gets disconnect reason code.
        /// </summary>
        public DisconnectReason ReasonCode { get; private set; }

        /// <summary>
        /// Gets disconnect description.
        /// </summary>
#if TUNING
        public string Description
        {
            get { return Utf8.GetString(_description, 0, _description.Length); }
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
            get { return Utf8.GetString(_language, 0, _language.Length); }
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
        /// Initializes a new instance of the <see cref="DisconnectMessage"/> class.
        /// </summary>
        public DisconnectMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectMessage"/> class.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="message">The message.</param>
        public DisconnectMessage(DisconnectReason reasonCode, string message)
        {
            ReasonCode = reasonCode;
            Description = message;
#if TUNING
            Language = "en";
#endif
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            ReasonCode = (DisconnectReason)ReadUInt32();
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
            Write((uint)ReasonCode);
#if TUNING
            WriteBinaryString(_description);
            WriteBinaryString(_language);
#else
            Write(Description);
            Write(Language ?? "en");
#endif
        }
    }
}
