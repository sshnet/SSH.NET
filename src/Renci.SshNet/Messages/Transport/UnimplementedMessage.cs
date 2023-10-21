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
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }

        internal override void Process(Session session)
        {
            session.OnUnimplementedReceived(this);
        }
    }
}
