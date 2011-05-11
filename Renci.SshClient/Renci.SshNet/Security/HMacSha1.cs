using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents SHA1 implementation of hashing algorithm
    /// </summary>
    internal class HMacSha1 : HMac
    {
        private HMACSHA1 _hash;

        protected override HMAC Hash
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
