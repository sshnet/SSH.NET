using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_FAILURE message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_FAILURE", 51)]
    public class FailureMessage : Message
    {
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
        ///   <c>true</c> if partially successful; otherwise, <c>false</c>.
        /// </value>
        public bool PartialSuccess { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.AllowedAuthentications = this.ReadNamesList();
            this.PartialSuccess = this.ReadBoolean();
            if (this.PartialSuccess)
            {
                this.Message = string.Join(",", this.AllowedAuthentications);
            }
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
