namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_CLOSE message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_CLOSE", 97)]
    public class ChannelCloseMessage : ChannelMessage
    {
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
