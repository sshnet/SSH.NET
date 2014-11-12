namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_OPEN_FAILURE message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_OPEN_FAILURE", 92)]
    public class ChannelOpenFailureMessage : ChannelMessage
    {
        internal const uint AdministrativelyProhibited = 1;
        internal const uint ConnectFailed = 2;
        internal const uint UnknownChannelType = 3;
        internal const uint ResourceShortage = 4;

        /// <summary>
        /// Gets failure reason code.
        /// </summary>
        public uint ReasonCode { get; private set; }

        /// <summary>
        /// Gets description for failure.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenFailureMessage"/> class.
        /// </summary>
        public ChannelOpenFailureMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenFailureMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="description">The description.</param>
        /// <param name="reasonCode">The reason code.</param>
        public ChannelOpenFailureMessage(uint localChannelNumber, string description, uint reasonCode)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.Description = description;
            this.ReasonCode = reasonCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenFailureMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="description">The description.</param>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="language">The language (RFC3066).</param>
        public ChannelOpenFailureMessage(uint localChannelNumber, string description, uint reasonCode, string language)
        {
            LocalChannelNumber = localChannelNumber;
            Description = description;
            ReasonCode = reasonCode;
            Language = language;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            this.ReasonCode = this.ReadUInt32();
            this.Description = this.ReadString();
            this.Language = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.ReasonCode);
            this.Write(this.Description ?? string.Empty);
            this.Write(this.Language ?? "en");
        }
    }
}
