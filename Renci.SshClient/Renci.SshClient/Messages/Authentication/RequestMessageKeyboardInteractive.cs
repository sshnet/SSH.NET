using System.Text;

namespace Renci.SshClient.Messages.Authentication
{
    internal class RequestMessageKeyboardInteractive : RequestMessage
    {
        public override string MethodName
        {
            get
            {
                return "keyboard-interactive";
            }
        }

        public string Language { get; private set; }

        public string SubMethods { get; private set; }

        public RequestMessageKeyboardInteractive(ServiceNames serviceName, string username)
            : base(serviceName, username)
        {
            this.Language = string.Empty;
            this.SubMethods = string.Empty;
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.Language);

            this.Write(this.SubMethods, Encoding.UTF8);
        }
    }
}
