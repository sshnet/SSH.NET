namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_PK_OK message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_PK_OK", 60)]
    internal class PublicKeyMessage : Message
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
                capacity += 4; // PublicKeyAlgorithmName length
                capacity += PublicKeyAlgorithmName.Length; // PublicKeyAlgorithmName
                capacity += 4; // PublicKeyData length
                capacity += PublicKeyData.Length; // PublicKeyData
                return capacity;
            }
        }

        internal override void Process(Session session)
        {
            session.OnUserAuthenticationPublicKeyReceived(this);
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            PublicKeyAlgorithmName = ReadBinary();
            PublicKeyData = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(PublicKeyAlgorithmName);
            WriteBinaryString(PublicKeyData);
        }
    }
}
