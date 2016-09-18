namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_DEBUG message.
    /// </summary>
    [Message("SSH_MSG_DEBUG", 4)]
    public class DebugMessage : Message
    {
        private byte[] _message;
        private byte[] _language;

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
        public string Message
        {
            get { return Utf8.GetString(_message, 0, _message.Length); }
        }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language
        {
            get { return Utf8.GetString(_language, 0, _language.Length); }
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
                capacity += 1; // IsAlwaysDisplay
                capacity += 4; // Message length
                capacity += _message.Length; // Message
                capacity += 4; // Language length
                capacity += _language.Length; // Language
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            IsAlwaysDisplay = ReadBoolean();
            _message = ReadBinary();
            _language = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            Write(IsAlwaysDisplay);
            WriteBinaryString(_message);
            WriteBinaryString(_language);
        }

        internal override void Process(Session session)
        {
            session.OnDebugReceived(this);
        }
    }
}
