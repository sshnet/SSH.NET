namespace Renci.SshClient.Messages.Connection
{
    public class ChannelEofMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelEof; }
        }

        public ChannelEofMessage()
        {

        }

        public ChannelEofMessage(uint localChannelNumber)
        {
            this.LocalChannelNumber = localChannelNumber;
        }
    }
}
