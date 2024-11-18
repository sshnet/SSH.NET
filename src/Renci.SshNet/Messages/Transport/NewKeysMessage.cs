namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_NEWKEYS message.
    /// </summary>
    public class NewKeysMessage : Message, IKeyExchangedAllowed
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_NEWKEYS";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 21;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
        }

        internal override void Process(Session session)
        {
            session.OnNewKeysReceived(this);
        }
    }
}
