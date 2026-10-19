using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_GSSAPI_RESPONSE message.
    /// </summary>
    internal sealed class GssApiResponseMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_GSSAPI_RESPONSE";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 60;
            }
        }

        /// <summary>
        /// Gets the selected mechanism OID.
        /// </summary>
        public byte[] SelectedMechanismOid { get; private set; }

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
                capacity += 4; // Selected mechanism oid length
                capacity += SelectedMechanismOid.Length; // Selected mechanism oid
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            SelectedMechanismOid = ReadBinary();
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
            session.OnUserAuthenticationGssApiResponseReceived(this);
        }
    }
}
