namespace Renci.SshClient.Messages.Connection
{
    public class ChannelSuccessMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelSuccess; }
        }

        public ChannelSuccessMessage()
        {

        }

        public ChannelSuccessMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }
    }
}
