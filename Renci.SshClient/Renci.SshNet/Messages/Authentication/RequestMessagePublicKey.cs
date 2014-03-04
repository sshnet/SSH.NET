namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "publickey" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    public class RequestMessagePublicKey : RequestMessage
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
                return "publickey";
            }
        }

        /// <summary>
        /// Gets the name of the public key algorithm.
        /// </summary>
        /// <value>
        /// The name of the public key algorithm.
        /// </value>
        public string PublicKeyAlgorithmName { get; private set; }

        /// <summary>
        /// Gets the public key data.
        /// </summary>
        public byte[] PublicKeyData { get; private set; }

        /// <summary>
        /// Gets or sets public key signature.
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessagePublicKey"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyAlgorithmName">Name of private key algorithm.</param>
        /// <param name="keyData">Private key data.</param>
        public RequestMessagePublicKey(ServiceName serviceName, string username, string keyAlgorithmName, byte[] keyData)
            : base(serviceName, username)
        {
            this.PublicKeyAlgorithmName = keyAlgorithmName;
            this.PublicKeyData = keyData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessagePublicKey"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyAlgorithmName">Name of private key algorithm.</param>
        /// <param name="keyData">Private key data.</param>
        /// <param name="signature">Private key signature.</param>
        public RequestMessagePublicKey(ServiceName serviceName, string username, string keyAlgorithmName, byte[] keyData, byte[] signature)
            : this(serviceName, username, keyAlgorithmName, keyData)
        {
            this.Signature = signature;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            if (this.Signature == null)
            {
                this.Write(false);
            }
            else
            {
                this.Write(true);
            }
            this.WriteAscii(this.PublicKeyAlgorithmName);
            this.WriteBinaryString(this.PublicKeyData);
            if (this.Signature != null)
                this.WriteBinaryString(this.Signature);
        }
    }
}
