using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents MD5 implementation of hashing algorithm
    /// </summary>
    internal class HMacMD5 : HMac
    {
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
            this._hmac = new System.Security.Cryptography.HMACMD5(key.Take(16).ToArray());
        }
    }
}
