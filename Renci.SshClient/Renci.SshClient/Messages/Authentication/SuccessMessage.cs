
namespace Renci.SshClient.Messages.Authentication
{
    internal class SuccessMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationSuccess; }
        }

        protected override void LoadData()
        {
        }

        protected override void SaveData()
        {
        }
    }
}
