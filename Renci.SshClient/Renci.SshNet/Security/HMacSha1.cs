using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents SHA1 implementation of hashing algorithm
    /// </summary>
    public class HMacSha1 : HMac
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
            get { return "hmac-sha1"; }
        }

        /// <summary>
        /// Initializes algorithm with specified key.
        /// </summary>
        /// <param name="key">The hash key.</param>
        public override void Init(IEnumerable<byte> key)
        {
            this._hash = new HMACSHA1(key.Take(20).ToArray());
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
