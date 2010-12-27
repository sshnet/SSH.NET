using System.Collections.Generic;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents base class for private keys
    /// </summary>
    public abstract class CryptoPrivateKey : CryptoKey
    {
        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <returns></returns>
        public abstract CryptoPublicKey GetPublicKey();

        /// <summary>
        /// Gets the signature.
        /// </summary>
        /// <param name="key">The key data bytes.</param>
        /// <returns></returns>
        public abstract IEnumerable<byte> GetSignature(IEnumerable<byte> key);

        /// <summary>
        /// Represents signature key data structure
        /// </summary>
        protected class SignatureKeyData : SshData
        {
            /// <summary>
            /// Gets or sets the name of the algorithm.
            /// </summary>
            /// <value>
            /// The name of the algorithm.
            /// </value>
            public string AlgorithmName { get; set; }

            /// <summary>
            /// Gets or sets the signature.
            /// </summary>
            /// <value>
            /// The signature.
            /// </value>
            public IEnumerable<byte> Signature { get; set; }

            /// <summary>
            /// Called when type specific data need to be loaded.
            /// </summary>
            protected override void LoadData()
            {
            }

            /// <summary>
            /// Called when type specific data need to be saved.
            /// </summary>
            protected override void SaveData()
            {
                this.Write(this.AlgorithmName);
                this.Write(this.Signature.GetSshString());
            }
        }
    }
}
