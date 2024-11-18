namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_SUCCESS message.
    /// </summary>
    public class ChannelSuccessMessage : ChannelMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelSuccessMessage"/> class.
        /// </summary>
        public ChannelSuccessMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelSuccessMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        public ChannelSuccessMessage(uint localChannelNumber)
            : base(localChannelNumber)
        {
        }

        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_CHANNEL_SUCCESS";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 99;
            }
        }

        internal override void Process(Session session)
        {
            session.OnChannelSuccessReceived(this);
        }
    }
}
