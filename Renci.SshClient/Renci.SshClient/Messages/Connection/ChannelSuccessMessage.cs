
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelSuccessMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelSuccess; }
        }
    }
}
