
namespace Renci.SshClient.Messages.Authentication
{
    [Message("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", 60)]
    internal class PasswordChangeRequiredMessage : Message
    {
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
