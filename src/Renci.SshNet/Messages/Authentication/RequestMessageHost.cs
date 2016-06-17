namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "hostbased" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    internal class RequestMessageHost : RequestMessage
    {
        /// <summary>
        /// Gets the public key algorithm for host key as ASCII encoded byte array.
        /// </summary>
        public byte[] PublicKeyAlgorithm { get; private set; }

        /// <summary>
        /// Gets or sets the public host key and certificates for client host.
        /// </summary>
        /// <value>
        /// The public host key.
        /// </value>
        public byte[] PublicHostKey { get; private set; }

        /// <summary>
        /// Gets or sets the name of the client host as ASCII encoded byte array.
        /// </summary>
        /// <value>
        /// The name of the client host.
        /// </value>
        public byte[] ClientHostName { get; private set; }

        /// <summary>
        /// Gets or sets the client username on the client host as UTF-8 encoded byte array.
        /// </summary>
        /// <value>
        /// The client username.
        /// </value>
        public byte[] ClientUsername { get; private set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        public byte[] Signature { get; private set; }

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
                capacity += 4; // PublicKeyAlgorithm length
                capacity += PublicKeyAlgorithm.Length; // PublicKeyAlgorithm
                capacity += 4; // PublicHostKey length
                capacity += PublicHostKey.Length; // PublicHostKey
                capacity += 4; // ClientHostName length
                capacity += ClientHostName.Length; // ClientHostName
                capacity += 4; // ClientUsername length
                capacity += ClientUsername.Length; // ClientUsername
                capacity += 4; // Signature length
                capacity += Signature.Length; // Signature
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageHost"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="publicKeyAlgorithm">The public key algorithm.</param>
        /// <param name="publicHostKey">The public host key.</param>
        /// <param name="clientHostName">Name of the client host.</param>
        /// <param name="clientUsername">The client username.</param>
        /// <param name="signature">The signature.</param>
        public RequestMessageHost(ServiceName serviceName, string username, string publicKeyAlgorithm, byte[] publicHostKey, string clientHostName, string clientUsername, byte[] signature)
            : base(serviceName, username, "hostbased")
        {
            PublicKeyAlgorithm = Ascii.GetBytes(publicKeyAlgorithm);
            PublicHostKey = publicHostKey;
            ClientHostName = Ascii.GetBytes(clientHostName);
            ClientUsername = Utf8.GetBytes(clientUsername);
            Signature = signature;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(PublicKeyAlgorithm);
            WriteBinaryString(PublicHostKey);
            WriteBinaryString(ClientHostName);
            WriteBinaryString(ClientUsername);
            WriteBinaryString(Signature);
        }
    }
}
