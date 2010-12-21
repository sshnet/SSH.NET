using System.Text;

namespace Renci.SshClient.Messages.Authentication
{
    internal class RequestMessagePassword : RequestMessage
    {
        public override string MethodName
        {
            get
            {
                return "password";
            }
        }

        public string Password { get; set; }

        public string NewPassword { get; set; }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(!string.IsNullOrEmpty(this.NewPassword));

            this.Write(this.Password, Encoding.UTF8);

            if (!string.IsNullOrEmpty(this.NewPassword))
            {
                this.Write(this.NewPassword, Encoding.UTF8);
            }
        }

    }
}
