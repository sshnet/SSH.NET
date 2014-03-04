namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "password" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal class RequestMessagePassword : RequestMessage
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
                return "password";
            }
        }

        /// <summary>
        /// Gets authentication password.
        /// </summary>
        public byte[] Password { get; private set; }

        /// <summary>
        /// Gets new authentication password.
        /// </summary>
        public byte[] NewPassword { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessagePassword"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        public RequestMessagePassword(ServiceName serviceName, string username, byte[] password)
            : base(serviceName, username)
        {
            this.Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessagePassword"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <param name="newPassword">New authentication password.</param>
        public RequestMessagePassword(ServiceName serviceName, string username, byte[] password, byte[] newPassword)
            : this(serviceName, username, password)
        {
            this.NewPassword = newPassword;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.NewPassword != null);

            this.Write((uint)this.Password.Length);
            this.Write(this.Password);

            if (this.NewPassword != null)
            {
                this.Write((uint)this.NewPassword.Length);
                this.Write(this.NewPassword);
            }
        }
    }
}
