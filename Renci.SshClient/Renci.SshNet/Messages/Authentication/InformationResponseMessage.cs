using System;
using System.Collections.Generic;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_INFO_RESPONSE message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_INFO_RESPONSE", 61)]
    internal class InformationResponseMessage : Message
    {
        /// <summary>
        /// Gets authentication responses.
        /// </summary>
        public IList<string> Responses { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationResponseMessage"/> class.
        /// </summary>
        public InformationResponseMessage()
        {
            this.Responses = new List<string>();
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
            this.Write((UInt32)this.Responses.Count);
            foreach (var response in this.Responses)
            {
                this.Write(response);
            }
        }
    }
}
