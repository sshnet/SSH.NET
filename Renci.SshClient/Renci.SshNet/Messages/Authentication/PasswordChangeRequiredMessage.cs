namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_PASSWD_CHANGEREQ message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", 60)]
    internal class PasswordChangeRequiredMessage : Message
    {
#if TUNING
        /// <summary>
        /// Gets password change request message as UTF-8 encoded byte array.
        /// </summary>
        public byte[] Message { get; private set; }
#else
        /// <summary>
        /// Gets password change request message.
        /// </summary>
        public string Message { get; private set; }
#endif


#if TUNING
        /// <summary>
        /// Gets message language as UTF-8 encoded byte array.
        /// </summary>
        public byte[] Language { get; private set; }
#else
        /// <summary>
        /// Gets message language.
        /// </summary>
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
                capacity += 4; // Message length
                capacity += Message.Length; // Message
                capacity += 4; // Language length
                capacity += Language.Length; // Language
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
#if TUNING
            Message = ReadBinary();
            Language = ReadBinary();
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
#if TUNING
            WriteBinaryString(Message);
            WriteBinaryString(Language);
#else
            Write(Message);
            Write(Language);
#endif
        }
    }
}
