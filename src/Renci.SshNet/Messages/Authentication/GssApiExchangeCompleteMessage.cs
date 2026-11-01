using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_GSSAPI_EXCHANGE_COMPLETE message.
    /// </summary>
    internal sealed class GssApiExchangeCompleteMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_GSSAPI_EXCHANGE_COMPLETE";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 63;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
