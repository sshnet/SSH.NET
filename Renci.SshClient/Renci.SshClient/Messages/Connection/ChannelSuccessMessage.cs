
namespace Renci.SshClient.Messages.Connection
{
    public class ChannelSuccessMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelSuccess; }
        }
    }
}
