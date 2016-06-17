namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "password" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal class RequestMessagePassword : RequestMessage
    {
        /// <summary>
        /// Gets authentication password.
        /// </summary>
        public byte[] Password { get; private set; }

        /// <summary>
        /// Gets new authentication password.
        /// </summary>
        public byte[] NewPassword { get; private set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 1; // NewPassword flag
                capacity += 4; // Password length
                capacity += Password.Length; // Password

                if (NewPassword != null)
                {
                    capacity += 4; // NewPassword length
                    capacity += NewPassword.Length; // NewPassword
                }

                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessagePassword"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        public RequestMessagePassword(ServiceName serviceName, string username, byte[] password)
            : base(serviceName, username, "password")
        {
            Password = password;
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
            NewPassword = newPassword;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(NewPassword != null);
            WriteBinaryString(Password);
            if (NewPassword != null)
            {
                WriteBinaryString(NewPassword);
            }
        }
    }
}
