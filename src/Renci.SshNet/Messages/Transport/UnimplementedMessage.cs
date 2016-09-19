using System;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_UNIMPLEMENTED message.
    /// </summary>
    [Message("SSH_MSG_UNIMPLEMENTED", 3)]
    public class UnimplementedMessage : Message
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
            throw new NotImplementedException();
        }

        internal override void Process(Session session)
        {
            session.OnUnimplementedReceived(this);
        }
    }
}
