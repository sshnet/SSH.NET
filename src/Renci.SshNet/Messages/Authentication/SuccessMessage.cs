namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_SUCCESS message.
    /// </summary>
    public class SuccessMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_SUCCESS";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 52;
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
            session.OnUserAuthenticationSuccessReceived(this);
        }
    }
}
