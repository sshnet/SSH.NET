using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_GSSAPI_ERRTOK message.
    /// </summary>
    internal sealed class GssApiErrorTokenMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_GSSAPI_ERRTOK";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 65;
            }
        }

        /// <summary>
        /// Gets the GSS token.
        /// </summary>
        public byte[] Token { get; private set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // GSS token length
                capacity += Token.Length; // GSS token
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            Token = ReadBinary();
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
            throw new NotImplementedException();
        }
    }
}
