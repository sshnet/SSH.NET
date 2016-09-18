namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_PASSWD_CHANGEREQ message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", 60)]
    internal class PasswordChangeRequiredMessage : Message
    {
        /// <summary>
        /// Gets password change request message as UTF-8 encoded byte array.
        /// </summary>
        public byte[] Message { get; private set; }

        /// <summary>
        /// Gets message language as UTF-8 encoded byte array.
        /// </summary>
        public byte[] Language { get; private set; }

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
                capacity += Message.Length; // Message
                capacity += 4; // Language length
                capacity += Language.Length; // Language
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            Message = ReadBinary();
            Language = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(Message);
            WriteBinaryString(Language);
        }

        internal override void Process(Session session)
        {
            session.OnUserAuthenticationPasswordChangeRequiredReceived(this);
        }
    }
}
