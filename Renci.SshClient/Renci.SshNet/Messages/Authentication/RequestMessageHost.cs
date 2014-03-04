namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "hostbased" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal class RequestMessageHost : RequestMessage
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
                return "hostbased";
            }
        }

        /// <summary>
        /// Gets the public key algorithm for host key
        /// </summary>
        public string PublicKeyAlgorithm { get; private set; }

        /// <summary>
        /// Gets or sets the public host key and certificates for client host.
        /// </summary>
        /// <value>
        /// The public host key.
        /// </value>
        public byte[] PublicHostKey { get; private set; }

        /// <summary>
        /// Gets or sets the name of the client host.
        /// </summary>
        /// <value>
        /// The name of the client host.
        /// </value>
        public string ClientHostName { get; private set; }

        /// <summary>
        /// Gets or sets the client username on the client host
        /// </summary>
        /// <value>
        /// The client username.
        /// </value>
        public string ClientUsername { get; private set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageHost"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="publicKeyAlgorithm">The public key algorithm.</param>
        /// <param name="publicHostKey">The public host key.</param>
        /// <param name="clientHostName">Name of the client host.</param>
        /// <param name="clientUsername">The client username.</param>
        public RequestMessageHost(ServiceName serviceName, string username, string publicKeyAlgorithm, byte[] publicHostKey, string clientHostName, string clientUsername)
            : base(serviceName, username)
        {
            this.PublicKeyAlgorithm = publicKeyAlgorithm;
            this.PublicHostKey = publicHostKey;
            this.ClientHostName = clientHostName;
            this.ClientUsername = clientUsername;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.WriteAscii(this.PublicKeyAlgorithm);
            this.WriteBinaryString(this.PublicHostKey);
            this.Write(this.ClientHostName);
            this.Write(this.ClientUsername);

            if (this.Signature != null)
                this.WriteBinaryString(this.Signature);
        }
    }
}
