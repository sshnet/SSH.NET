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

        public string Password { get; private set; }

        public string NewPassword { get; private set; }


        public RequestMessagePassword(ServiceNames serviceName, string username, string password)
            : base(serviceName, username)
        {
            this.Password = password ?? string.Empty;
        }

        public RequestMessagePassword(ServiceNames serviceName, string username, string password, string newPassword)
            : this(serviceName, username, password)
        {
            this.NewPassword = newPassword ?? string.Empty;
        }

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
