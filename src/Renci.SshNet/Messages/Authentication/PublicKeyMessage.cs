namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_PK_OK message.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_PK_OK", 60)]
    internal class PublicKeyMessage : Message
    {
#if TUNING
        /// <summary>
        /// Gets the name of the public key algorithm as ASCII encoded byte array.
        /// </summary>
        /// <value>
        /// The name of the public key algorithm.
        /// </value>
        public byte[] PublicKeyAlgorithmName { get; private set; }
#else
        /// <summary>
        /// Gets the name of the public key algorithm.
        /// </summary>
        /// <value>
        /// The name of the public key algorithm.
        /// </value>
        public string PublicKeyAlgorithmName { get; private set; }
#endif

        /// <summary>
        /// Gets the public key data.
        /// </summary>
        public byte[] PublicKeyData { get; private set; }

#if TUNING
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
#endif

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
#if TUNING
            PublicKeyAlgorithmName = ReadBinary();
            PublicKeyData = ReadBinary();
#else
            PublicKeyAlgorithmName = ReadAsciiString();
            PublicKeyData = ReadBinaryString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
#if TUNING
            WriteBinaryString(PublicKeyAlgorithmName);
            WriteBinaryString(PublicKeyData);
#else
            WriteAscii(PublicKeyAlgorithmName);
            WriteBinaryString(PublicKeyData);
#endif
        }
    }
}
