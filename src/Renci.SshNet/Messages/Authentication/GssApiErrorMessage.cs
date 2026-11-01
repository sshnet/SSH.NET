using System;
namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_GSSAPI_ERROR message.
    /// </summary>
    internal sealed class GssApiErrorMessage : Message
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_USERAUTH_GSSAPI_ERROR";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 64;
            }
        }

        /// <summary>
        /// Gets the major status.
        /// </summary>
        public uint MajorStatus { get; private set; }

        /// <summary>
        /// Gets the minor status.
        /// </summary>
        public uint MinorStatus { get; private set; }

        /// <summary>
        /// Gets the message encoded in UTF8.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the language tag encoded in ASCII.
        /// </summary>
        public string LanguageTag { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            MajorStatus = ReadUInt32();
            MinorStatus = ReadUInt32();

            // The message text MUST be encoded in the UTF-8 encoding
            Message = ReadString(Utf8);
            LanguageTag = ReadString(Ascii);
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
