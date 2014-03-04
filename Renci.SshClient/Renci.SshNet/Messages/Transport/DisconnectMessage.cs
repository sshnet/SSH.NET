namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_DISCONNECT message.
    /// </summary>
    [Message("SSH_MSG_DISCONNECT", 1)]
    public class DisconnectMessage : Message, IKeyExchangedAllowed
    {
        /// <summary>
        /// Gets disconnect reason code.
        /// </summary>
        public DisconnectReason ReasonCode { get; private set; }

        /// <summary>
        /// Gets disconnect description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectMessage"/> class.
        /// </summary>
        public DisconnectMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectMessage"/> class.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="message">The message.</param>
        public DisconnectMessage(DisconnectReason reasonCode, string message)
        {
            this.ReasonCode = reasonCode;
            this.Description = message;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.ReasonCode = (DisconnectReason)this.ReadUInt32();
            this.Description = this.ReadString();
            this.Language = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write((uint)this.ReasonCode);
            this.Write(this.Description);
            this.Write(this.Language ?? "en");
        }
    }
}
