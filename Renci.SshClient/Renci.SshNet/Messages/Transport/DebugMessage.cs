namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_DEBUG message.
    /// </summary>
    [Message("SSH_MSG_DEBUG", 4)]
    public class DebugMessage : Message
    {
#if TUNING
        private byte[] _message;
        private byte[] _language;
#endif

        /// <summary>
        /// Gets a value indicating whether the message to be always displayed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the message always to be displayed; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlwaysDisplay { get; private set; }

        /// <summary>
        /// Gets debug message.
        /// </summary>
#if TUNING
        public string Message
        {
            get { return Utf8.GetString(_message); }
        }
#else
        public string Message { get; private set; }
#endif

        /// <summary>
        /// Gets message language.
        /// </summary>
#if TUNING
        public string Language
        {
            get { return Utf8.GetString(_language); }
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
                capacity += 1; // IsAlwaysDisplay
                capacity += 4; // Message length
                capacity += _message.Length; // Message
                capacity += 4; // Language length
                capacity += _language.Length; // Language
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            IsAlwaysDisplay = ReadBoolean();
#if TUNING
            _message = ReadBinary();
            _language = ReadBinary();
#else
            Message = ReadString();
            Language = ReadString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            Write(IsAlwaysDisplay);
#if TUNING
            WriteBinaryString(_message);
            WriteBinaryString(_language);
#else
            Write(Message);
            Write(Language);
#endif
        }
    }
}
