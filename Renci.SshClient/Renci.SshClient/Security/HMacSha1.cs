using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents SHA1 implementation of hashing algorithm
    /// </summary>
    internal class HMacSha1 : HMac
    {
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
            this._hmac = new System.Security.Cryptography.HMACSHA1(key.Take(20).ToArray());
        }
    }
}
