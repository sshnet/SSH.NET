namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_CLOSE message.
    /// </summary>
    public class ChannelCloseMessage : ChannelMessage
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_CHANNEL_CLOSE";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 97;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCloseMessage"/> class.
        /// </summary>
        public ChannelCloseMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCloseMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        public ChannelCloseMessage(uint localChannelNumber)
            : base(localChannelNumber)
        {
        }

        internal override void Process(Session session)
        {
            session.OnChannelCloseReceived(this);
        }
    }
}
