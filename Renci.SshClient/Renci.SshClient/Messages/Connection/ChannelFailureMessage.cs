
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelFailureMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelFailure; }
        }
    }
}
