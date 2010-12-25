namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_CHANNEL_SUCCESS", 99)]
    public class ChannelSuccessMessage : ChannelMessage
    {
        public ChannelSuccessMessage()
        {

        }

        public ChannelSuccessMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }
    }
}
