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
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// <c>-1</c> to indicate that the size of the message cannot be determined,
        /// or is too costly to calculate.
        /// </value>
        protected override int BufferCapacity
        {
            get { return -1; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationResponseMessage"/> class.
        /// </summary>
        public InformationResponseMessage()
        {
            Responses = new List<string>();
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
            Write((uint) Responses.Count);
            foreach (var response in Responses)
            {
                Write(response);
            }
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
