
namespace Renci.SshClient.Messages.Transport
{
    internal class UnimplementedMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.Unimplemented; }
        }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}
