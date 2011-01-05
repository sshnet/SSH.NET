using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents MD5 implementation of hashing algorithm
    /// </summary>
    internal class HMacMD5 : HMac
    {
        private HMACMD5 _hash;

        protected override HMAC Hash
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
            this._hash = new HMACMD5(key.Take(16).ToArray());
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
