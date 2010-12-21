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

        public string Language { get; set; }

        public string SubMethods { get; set; }

        public RequestMessageKeyboardInteractive()
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
