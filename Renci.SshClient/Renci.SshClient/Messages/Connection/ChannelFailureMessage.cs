namespace Renci.SshClient.Messages.Connection
{
    [Message("SSH_MSG_CHANNEL_FAILURE", 100)]
    public class ChannelFailureMessage : ChannelMessage
    {
        public ChannelFailureMessage()
        {

        }

        public ChannelFailureMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }
    }
}
