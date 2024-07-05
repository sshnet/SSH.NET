using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements key support for host algorithm.
    /// </summary>
    public class KeyHostAlgorithm : HostAlgorithm
    {
        /// <summary>
        /// Gets the key used in this host key algorithm.
        /// </summary>
        public Key Key { get; private set; }

        /// <summary>
        /// Gets the signature implementation used in this host key algorithm.
        /// </summary>
        public DigitalSignature DigitalSignature { get; private set; }

        /// <summary>
        /// Gets the encoded public key data.
        /// </summary>
        /// <value>
        /// The encoded public key data.
        /// </value>
        public override byte[] Data
        {
            get
            {
                var keyFormatIdentifier = Key is RsaKey ? "ssh-rsa" : Name;
                return new SshKeyData(keyFormatIdentifier, Key.Public).GetBytes();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The signature format identifier.</param>
        /// <param name="key">The key used in this host key algorithm.</param>
        public KeyHostAlgorithm(string name, Key key)
            : base(name)
        {
            Key = key;
            DigitalSignature = key.DigitalSignature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The signature format identifier.</param>
        /// <param name="key">The key used in this host key algorithm.</param>
        /// <param name="digitalSignature">The signature implementation used in this host key algorithm.</param>
        /// <remarks>
        /// The key used by <paramref name="digitalSignature"/> is intended to be equal to <paramref name="key"/>.
        /// This is not verified.
        /// </remarks>
        public KeyHostAlgorithm(string name, Key key, DigitalSignature digitalSignature)
            : base(name)
        {
            Key = key;
            DigitalSignature = digitalSignature;
        }

        /// <summary>
        /// Signs and encodes the specified data.
        /// </summary>
        /// <param name="data">The data to be signed.</param>
        /// <returns>
        /// The encoded signature.
        /// </returns>
        public override byte[] Sign(byte[] data)
        {
            return new SignatureKeyData(Name, DigitalSignature.Sign(data)).GetBytes();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data to verify the signature against.</param>
        /// <param name="signature">The encoded signature data.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="signature"/> is the result of signing <paramref name="data"/>
        /// with the corresponding private key to <see cref="Key"/>.
        /// </returns>
        public override bool VerifySignature(byte[] data, byte[] signature)
        {
            var signatureData = new SignatureKeyData();
            signatureData.Load(signature);

            return DigitalSignature.Verify(data, signatureData.Signature);
        }

        internal sealed class SignatureKeyData : SshData
        {
            /// <summary>
            /// Gets or sets the signature format identifier.
            /// </summary>
            /// <value>
            /// The signature format identifier.
            /// </value>
            public string AlgorithmName { get; set; }

            /// <summary>
            /// Gets the signature.
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
                    capacity += 4; // AlgorithmName length
                    capacity += Encoding.UTF8.GetByteCount(AlgorithmName); // AlgorithmName
                    capacity += 4; // Signature length
                    capacity += Signature.Length; // Signature
                    return capacity;
                }
            }

            public SignatureKeyData()
            {
            }

            public SignatureKeyData(string name, byte[] signature)
            {
                AlgorithmName = name;
                Signature = signature;
            }

            /// <summary>
            /// Called when type specific data need to be loaded.
            /// </summary>
            protected override void LoadData()
            {
                AlgorithmName = ReadString();
                Signature = ReadBinary();
            }

            /// <summary>
            /// Called when type specific data need to be saved.
            /// </summary>
            protected override void SaveData()
            {
                Write(AlgorithmName);
                WriteBinaryString(Signature);
            }
        }
    }
}
