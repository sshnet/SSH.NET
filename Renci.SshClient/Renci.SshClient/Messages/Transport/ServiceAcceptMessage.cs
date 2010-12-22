using System;

namespace Renci.SshClient.Messages.Transport
{
    /// <summary>
    /// SSH_MSG_SERVICE_ACCEPT
    /// </summary>
    public class ServiceAcceptMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ServiceAcceptRequest; }
        }

        public ServiceNames ServiceName { get; private set; }

        protected override void LoadData()
        {
            var serviceName = this.ReadString();
            switch (serviceName)
            {
                case "ssh-userauth":
                    this.ServiceName = ServiceNames.UserAuthentication;
                    break;
                case "ssh-connection":
                    this.ServiceName = ServiceNames.Connection;
                    break;
                default:
                    break;
            }
        }

        protected override void SaveData()
        {
            throw new InvalidOperationException("Save data is not supported.");
        }
    }
}
