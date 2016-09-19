namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_SUCCESS message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_SUCCESS", 99)]
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

        internal override void Process(Session session)
        {
            session.OnChannelSuccessReceived(this);
        }
    }
}
