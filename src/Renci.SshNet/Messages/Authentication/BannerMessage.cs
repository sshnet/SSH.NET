namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_BANNER message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_BANNER", 53)]
    public class BannerMessage : Message
    {
        private byte[] _message;
        private byte[] _language;

        /// <summary>
        /// Gets banner message.
        /// </summary>
        public string Message
        {
            get { return Utf8.GetString(_message, 0, _message.Length); }
        }

        /// <summary>
        /// Gets banner language.
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
                capacity += 4; // Message length
                capacity += _message.Length; // Message
                capacity += 4; // Language length
                capacity += _language.Length; // Language
                return capacity;
            }
        }

        internal override void Process(Session session)
        {
            session.OnUserAuthenticationBannerReceived(this);
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            _message = ReadBinary();
            _language = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(_message);
            WriteBinaryString(_language);
        }
    }
}
