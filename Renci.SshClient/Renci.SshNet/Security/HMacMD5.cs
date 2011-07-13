using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents MD5 implementation of hashing algorithm
    /// </summary>
    public class HMacMD5 : HMac
    {
        private KeyedHashAlgorithm _hash;

        /// <summary>
        /// Instance of initialized hash algorithm that being used
        /// </summary>
        protected override KeyedHashAlgorithm Hash
        {
            get { return this._hash; }
        }

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "hmac-md5"; }
        }

        /// <summary>
        /// Initializes algorithm with specified key.
        /// </summary>
        /// <param name="key">The hash key.</param>
        public override void Init(IEnumerable<byte> key)
        {
            this._hash = new Renci.SshNet.Security.Cryptography.HMAC<Renci.SshNet.Security.Cryptography.MD5>(key.Take(16).ToArray());
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected override void Dispose(bool disposing)
        {
            // Dispose managed ResourceMessages.
            if (this._hash != null)
            {
                this._hash.Dispose();
                this._hash = null;
            }

            base.Dispose(disposing);
        }
    }
}
