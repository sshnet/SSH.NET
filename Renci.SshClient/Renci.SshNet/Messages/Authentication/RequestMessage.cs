using System;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_REQUEST message. Server as a base message for other user authentication requests.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_REQUEST", 50)]
    public class RequestMessage : Message
    {
        /// <summary>
        /// Gets authentication username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public ServiceName ServiceName { get; private set; }

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public virtual string MethodName { get { return "none"; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessage"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        public RequestMessage(ServiceName serviceName, string username)
        {
            this.ServiceName = serviceName;
            this.Username = username;
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
            this.Write(this.Username);
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
            this.WriteAscii(this.MethodName);
        }
    }
}

