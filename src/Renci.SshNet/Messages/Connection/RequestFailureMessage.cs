
namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_REQUEST_FAILURE message.
    /// </summary>
    [Message("SSH_MSG_REQUEST_FAILURE", 82)]
    public class RequestFailureMessage : Message
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
            session.OnRequestFailureReceived(this);
        }
    }
}
