namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_EOF message.
    /// </summary>
    [Message("SSH_MSG_CHANNEL_EOF", 96)]
    public class ChannelEofMessage : ChannelMessage
    {
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
