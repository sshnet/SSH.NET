
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelCloseMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelClose; }
        }
    }
}
