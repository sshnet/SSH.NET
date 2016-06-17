namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents "publickey" SSH_MSG_USERAUTH_REQUEST message.
    /// </summary>
    public class RequestMessagePublicKey : RequestMessage
    {
        /// <summary>
        /// Gets the name of the public key algorithm as ASCII encoded byte array.
        /// </summary>
        /// <value>
        /// The name of the public key algorithm.
        /// </value>
        public byte[] PublicKeyAlgorithmName { get; private set; }

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
                capacity += 1; // Signature flag
                capacity += 4; // PublicKeyAlgorithmName length
                capacity += PublicKeyAlgorithmName.Length; // PublicKeyAlgorithmName
                capacity += 4; // PublicKeyData length
                capacity += PublicKeyData.Length; // PublicKeyData

                if (Signature != null)
                {
                    capacity += 4; // Signature length
                    capacity += Signature.Length; // Signature
                }

                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessagePublicKey"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyAlgorithmName">Name of private key algorithm.</param>
        /// <param name="keyData">Private key data.</param>
        public RequestMessagePublicKey(ServiceName serviceName, string username, string keyAlgorithmName, byte[] keyData)
            : base(serviceName, username, "publickey")
        {
            PublicKeyAlgorithmName = Ascii.GetBytes(keyAlgorithmName);
            PublicKeyData = keyData;
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
            Signature = signature;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(Signature != null);
            WriteBinaryString(PublicKeyAlgorithmName);
            WriteBinaryString(PublicKeyData);
            if (Signature != null)
                WriteBinaryString(Signature);
        }
    }
}
