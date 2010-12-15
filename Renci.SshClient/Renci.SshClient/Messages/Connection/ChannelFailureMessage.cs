
namespace Renci.SshClient.Messages.Connection
{
    public class ChannelFailureMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelFailure; }
        }
    }
}
