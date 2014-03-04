namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "keyboard-interactive" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal class RequestMessageKeyboardInteractive : RequestMessage
    {
        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public override string MethodName
        {
            get
            {
                return "keyboard-interactive";
            }
        }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Gets authentication sub methods.
        /// </summary>
        public string SubMethods { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageKeyboardInteractive"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        public RequestMessageKeyboardInteractive(ServiceName serviceName, string username)
            : base(serviceName, username)
        {
            this.Language = string.Empty;
            this.SubMethods = string.Empty;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.Language);

            this.Write(this.SubMethods);
        }
    }
}
