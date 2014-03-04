using System;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_SERVICE_ACCEPT message.
    /// </summary>
    [Message("SSH_MSG_SERVICE_ACCEPT", 6)]
    public class ServiceAcceptMessage : Message
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public ServiceName ServiceName { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            var serviceName = this.ReadAsciiString();
            switch (serviceName)
            {
                case "ssh-userauth":
                    this.ServiceName = ServiceName.UserAuthentication;
                    break;
                case "ssh-connection":
                    this.ServiceName = ServiceName.Connection;
                    break;
            }
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            throw new InvalidOperationException("Save data is not supported.");
        }
    }
}
