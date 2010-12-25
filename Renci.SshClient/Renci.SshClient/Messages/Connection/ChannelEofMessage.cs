namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_CHANNEL_EOF", 96)]
    public class ChannelEofMessage : ChannelMessage
    {
        public ChannelEofMessage()
        {

        }

        public ChannelEofMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }
    }
}
