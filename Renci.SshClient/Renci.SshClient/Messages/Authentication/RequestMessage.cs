using System;
using System.Text;

namespace Renci.SshClient.Messages.Authentication
{
    public class RequestMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.UserAuthenticationRequest; }
        }

        public string Username { get; set; }

        public ServiceNames ServiceName { get; set; }

        public virtual string MethodName { get { return "none"; } }

        protected override void LoadData()
        {
            throw new InvalidOperationException("Load data is not supported.");
        }

        protected override void SaveData()
        {
            this.Write(this.Username, Encoding.UTF8);
            switch (this.ServiceName)
            {
                case ServiceNames.UserAuthentication:
                    this.Write("ssh-userauth", Encoding.UTF8);
                    break;
                case ServiceNames.Connection:
                    this.Write("ssh-connection", Encoding.UTF8);
                    break;
                default:
                    throw new NotSupportedException("Not supported service name");
            }
            this.Write(this.MethodName, Encoding.ASCII);
        }
    }
}

