
namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_DEBUG message.
    /// </summary>
    [Message("SSH_MSG_DEBUG", 4)]
    public class DebugMessage : Message
    {
        /// <summary>
        /// Gets a value indicating whether the message to be always displayed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the message always to be displayed; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlwaysDisplay { get; private set; }

        /// <summary>
        /// Gets debug message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.IsAlwaysDisplay = this.ReadBoolean();
            this.Message = this.ReadString();
            this.Language = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.IsAlwaysDisplay);
            this.Write(this.Message);
            this.Write(this.Language);
        }
    }
}
