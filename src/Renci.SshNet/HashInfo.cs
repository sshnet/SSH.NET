using System;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet
{
    /// <summary>
    /// Holds information about key size and cipher to use.
    /// </summary>
    public class HashInfo
    {
        /// <summary>
        /// Gets the size of the key.
        /// </summary>
        /// <value>
        /// The size of the key.
        /// </value>
        public int KeySize { get; private set; }

        /// <summary>
        /// Gets the cipher.
        /// </summary>
        public Func<byte[], HMAC> HMAC { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashInfo"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        /// <param name="hash">The hash algorithm to use for a given key.</param>
        public HashInfo(int keySize, Func<byte[], HMAC> hash)
        {
            KeySize = keySize;
            HMAC = key => hash(key.Take(KeySize / 8));
        }
    }
}
