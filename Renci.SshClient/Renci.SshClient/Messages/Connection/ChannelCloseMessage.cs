
namespace Renci.SshClient.Messages.Connection
{
    public class ChannelCloseMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelClose; }
        }
    }
}
