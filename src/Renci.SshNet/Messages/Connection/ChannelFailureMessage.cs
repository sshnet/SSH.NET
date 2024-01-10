namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_FAILURE message.
    /// </summary>
    public class ChannelFailureMessage : ChannelMessage
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_CHANNEL_FAILURE";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFailureMessage"/> class.
        /// </summary>
        public ChannelFailureMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFailureMessage"/> class.
        /// </summary>
        /// <param name="localChannelNumber">The local channel number.</param>
        public ChannelFailureMessage(uint localChannelNumber)
            : base(localChannelNumber)
        {
        }

        internal override void Process(Session session)
        {
            session.OnChannelFailureReceived(this);
        }
    }
}
