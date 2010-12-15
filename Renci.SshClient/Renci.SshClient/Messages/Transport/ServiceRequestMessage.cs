using System;

namespace Renci.SshClient.Messages.Transport
{
    /// <summary>
    /// Contains SSH_MSG_SERVICE_REQUEST message information
    /// </summary>
    public class ServiceRequestMessage : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ServiceRequest; }
        }

        public ServiceNames ServiceName { get; set; }

        protected override void LoadData()
        {
            throw new InvalidOperationException("Load data is not supported.");
        }

        protected override void SaveData()
        {
            switch (this.ServiceName)
            {
                case ServiceNames.UserAuthentication:
                    this.Write("ssh-userauth");
                    break;
                case ServiceNames.Connection:
                    this.Write("ssh-connection");
                    break;
                default:
                    throw new NotSupportedException("Not supported service name");
            }

        }
    }
}
