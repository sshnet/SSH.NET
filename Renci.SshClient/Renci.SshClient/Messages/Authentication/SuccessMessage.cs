
namespace Renci.SshClient.Messages.Authentication
{
    public class SuccessMessage : Message
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
