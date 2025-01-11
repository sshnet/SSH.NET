using System;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_UNIMPLEMENTED message.
    /// </summary>
    public class UnimplementedMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_UNIMPLEMENTED";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 3;
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
            throw new NotImplementedException();
        }

        internal override void Process(Session session)
        {
            session.OnUnimplementedReceived(this);
        }
    }
}
