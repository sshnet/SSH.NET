using System.Text;

namespace Renci.SshClient.Messages.Authentication
{
    [Message("SSH_MSG_USERAUTH_BANNER", 53)]
    public class BannerMessage : Message
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
            this.Write(this.Message, Encoding.UTF8);
            this.Write(this.Language);
        }
    }
}
