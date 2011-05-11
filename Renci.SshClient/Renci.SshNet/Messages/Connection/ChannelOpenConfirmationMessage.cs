namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_OPEN_CONFIRMATION message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_OPEN_CONFIRMATION", 91)]
    public class ChannelOpenConfirmationMessage : ChannelMessage
    {
        /// <summary>
        /// Gets the remote channel number.
        /// </summary>
        public uint RemoteChannelNumber { get; private set; }

        /// <summary>
        /// Gets the initial size of the window.
        /// </summary>
        /// <value>
        /// The initial size of the window.
        /// </value>
        public uint InitialWindowSize { get; private set; }

        /// <summary>
        /// Gets the maximum size of the packet.
        /// </summary>
        /// <value>
        /// The maximum size of the packet.
        /// </value>
        public uint MaximumPacketSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenConfirmationMessage"/> class.
        /// </summary>
        public ChannelOpenConfirmationMessage()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOpenConfirmationMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        public ChannelOpenConfirmationMessage(uint localChannelNumber, uint initialWindowSize, uint maximumPacketSize, uint remoteChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
            this.InitialWindowSize = initialWindowSize;
            this.MaximumPacketSize = maximumPacketSize;
            this.RemoteChannelNumber = remoteChannelNumber;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();
            this.RemoteChannelNumber = this.ReadUInt32();
            this.InitialWindowSize = this.ReadUInt32();
            this.MaximumPacketSize = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.RemoteChannelNumber);
            this.Write(this.InitialWindowSize);
            this.Write(this.MaximumPacketSize);
        }
    }
}
