
namespace Renci.SshClient.Messages.Connection
{
    internal class RequestFailureMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.RequestFailure; }
        }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}
