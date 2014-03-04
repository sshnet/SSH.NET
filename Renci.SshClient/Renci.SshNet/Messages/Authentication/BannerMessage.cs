namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_BANNER message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_BANNER", 53)]
    public class BannerMessage : Message
    {
        /// <summary>
        /// Gets banner message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets banner language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.Message = this.ReadString();
            this.Language = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.Message);
            this.Write(this.Language);
        }
    }
}
