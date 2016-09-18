namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_FAILURE message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_FAILURE", 100)]
    public class ChannelFailureMessage : ChannelMessage
    {
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
