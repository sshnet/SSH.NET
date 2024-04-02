using System;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet
{
    /// <summary>
    /// Holds information about key size and cipher to use.
    /// </summary>
    public class CipherInfo
    {
        /// <summary>
        /// Gets the size of the key.
        /// </summary>
        /// <value>
        /// The size of the key.
        /// </value>
        public int KeySize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the cipher is AEAD (Authenticated Encryption with Associated data).
        /// </summary>
        /// <value>
        /// <see langword="true"/> to indicate the cipher is AEAD, <see langword="false"/> to incidicate the cipher is not AEAD.
        /// </value>
        public bool IsAead { get; private set; }

        /// <summary>
        /// Gets the cipher.
        /// </summary>
        public Func<byte[], byte[], Cipher> Cipher { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherInfo"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        /// <param name="cipher">The cipher.</param>
        /// <param name="isAead"><see langword="true"/> to indicate the cipher is AEAD, <see langword="false"/> to incidicate the cipher is not AEAD.</param>
        public CipherInfo(int keySize, Func<byte[], byte[], Cipher> cipher, bool isAead = false)
        {
            KeySize = keySize;
            Cipher = (key, iv) => cipher(key.Take(KeySize / 8), iv);
            IsAead = isAead;
        }
    }
}
