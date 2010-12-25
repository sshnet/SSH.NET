namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_CHANNEL_CLOSE", 97)]
    public class ChannelCloseMessage : ChannelMessage
    {
        public ChannelCloseMessage()
        {

        }

        public ChannelCloseMessage(uint localChannelNumber)
        {
            LocalChannelNumber = localChannelNumber;
        }
    }
}
