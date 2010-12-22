
namespace Renci.SshClient.Messages.Authentication
{
    internal class PasswordChangeRequiredMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationPasswordChangeRequired; }
        }

        public string Message { get; private set; }

        public string Language { get; private set; }

        protected override void LoadData()
        {
            this.Message = this.ReadString();
            this.Language = this.ReadString();
        }

        protected override void SaveData()
        {
            this.Write(this.Message);
            this.Write(this.Language);
        }
    }
}
