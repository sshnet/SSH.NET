using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_GSSAPI_MIC message.
    /// </summary>
    internal sealed class GssApiMicMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_GSSAPI_MIC";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 66;
            }
        }

        /// <summary>
        /// Gets the MIC.
        /// </summary>
        public byte[] MIC
        {
            get; private set;
        }

        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // MIC length
                capacity += MIC.Length; // MIC
                return capacity;
            }
        }

        public GssApiMicMessage(byte[] mic)
        {
            MIC = mic;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            MIC = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(MIC);
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
