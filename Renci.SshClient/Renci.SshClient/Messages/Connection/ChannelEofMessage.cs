namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelEofMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelEof; }
        }
    }
}
