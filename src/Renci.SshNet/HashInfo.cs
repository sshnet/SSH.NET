using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

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
        /// Gets a value indicating whether enable encrypt-then-MAC or use encrypt-and-MAC.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to enable encrypt-then-MAC, <see langword="false"/> to use encrypt-and-MAC.
        /// </value>
        public bool IsEncryptThenMAC { get; private set; }

        /// <summary>
        /// Gets the cipher.
        /// </summary>
        public Func<byte[], HashAlgorithm> HashAlgorithm { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashInfo"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        /// <param name="hash">The hash algorithm to use for a given key.</param>
        /// <param name="isEncryptThenMAC"><see langword="true"/> to enable encrypt-then-MAC, <see langword="false"/> to use encrypt-and-MAC.</param>
        public HashInfo(int keySize, Func<byte[], HashAlgorithm> hash, bool isEncryptThenMAC = false)
        {
            KeySize = keySize;
            HashAlgorithm = key => hash(key.Take(KeySize / 8));
            IsEncryptThenMAC = isEncryptThenMAC;
        }
    }
}
