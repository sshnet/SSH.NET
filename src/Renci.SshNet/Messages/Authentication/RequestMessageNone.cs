namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "none" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal sealed class RequestMessageNone : RequestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageNone"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        public RequestMessageNone(ServiceName serviceName, string username)
            : base(serviceName, username, "none")
        {
        }
    }
}
