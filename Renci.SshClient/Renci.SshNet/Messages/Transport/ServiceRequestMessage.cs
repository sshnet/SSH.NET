using System;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_SERVICE_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_SERVICE_REQUEST", 5)]
    public class ServiceRequestMessage : Message
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public ServiceName ServiceName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRequestMessage"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public ServiceRequestMessage(ServiceName serviceName)
        {
            this.ServiceName = serviceName;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            throw new InvalidOperationException("Load data is not supported.");
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            switch (this.ServiceName)
            {
                case ServiceName.UserAuthentication:
                    this.WriteAscii("ssh-userauth");
                    break;
                case ServiceName.Connection:
                    this.WriteAscii("ssh-connection");
                    break;
                default:
                    throw new NotSupportedException("Not supported service name");
            }

        }
    }
}
