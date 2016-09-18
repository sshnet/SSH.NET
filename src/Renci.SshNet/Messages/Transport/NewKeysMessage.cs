namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_NEWKEYS message.
    /// </summary>
    [Message("SSH_MSG_NEWKEYS", 21)]
    public class NewKeysMessage : Message, IKeyExchangedAllowed
    {
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
