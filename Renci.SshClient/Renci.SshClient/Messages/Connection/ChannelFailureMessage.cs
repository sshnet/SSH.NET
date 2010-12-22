namespace Renci.SshClient.Messages.Connection
{
    public class ChannelFailureMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelFailure; }
        }

        public ChannelFailureMessage()
        {

        }

        public ChannelFailureMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }
    }
}
