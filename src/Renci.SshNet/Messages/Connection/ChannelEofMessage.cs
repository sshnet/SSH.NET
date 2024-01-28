namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_EOF message.
    /// </summary>
    public class ChannelEofMessage : ChannelMessage
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_CHANNEL_EOF";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 96;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEofMessage"/> class.
        /// </summary>
        public ChannelEofMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEofMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        public ChannelEofMessage(uint localChannelNumber)
            : base(localChannelNumber)
        {
        }

        internal override void Process(Session session)
        {
            session.OnChannelEofReceived(this);
        }
    }
}
