namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_SUCCESS message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_WINDOW_ADJUST", 93)]
    public class ChannelWindowAdjustMessage : ChannelMessage
    {
        /// <summary>
        /// Gets number of bytes to add to the window.
        /// </summary>
        public uint BytesToAdd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelWindowAdjustMessage"/> class.
        /// </summary>
        public ChannelWindowAdjustMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelWindowAdjustMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="bytesToAdd">The bytes to add.</param>
        public ChannelWindowAdjustMessage(uint localChannelNumber, uint bytesToAdd)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.BytesToAdd = bytesToAdd;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            this.BytesToAdd = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.BytesToAdd);
        }
    }
}
