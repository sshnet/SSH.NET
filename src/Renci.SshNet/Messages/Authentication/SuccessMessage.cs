namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_SUCCESS message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_SUCCESS", 52)]
    public class SuccessMessage : Message
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
            session.OnUserAuthenticationSuccessReceived(this);
        }
    }
}
