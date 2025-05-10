using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_FAILURE message.
    /// </summary>
    public class FailureMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_FAILURE";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 51;
            }
        }

        /// <summary>
        /// Gets or sets the allowed authentications if available.
        /// </summary>
        /// <value>
        /// The allowed authentications.
        /// </value>
        public string[] AllowedAuthentications { get; set; }

        /// <summary>
        /// Gets failure message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets a value indicating whether authentication is partially successful.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if partially successful; otherwise, <see langword="false"/>.
        /// </value>
        public bool PartialSuccess { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            AllowedAuthentications = ReadNamesList();
            PartialSuccess = ReadBoolean();
            if (PartialSuccess)
            {
#if NET
                Message = string.Join(',', AllowedAuthentications);
#else
                Message = string.Join(",", AllowedAuthentications);
#endif
            }
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
            session.OnUserAuthenticationFailureReceived(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"SSH_MSG_USERAUTH_FAILURE {string.Join(",", AllowedAuthentications)} ({nameof(PartialSuccess)}:{PartialSuccess})";
        }
    }
}
