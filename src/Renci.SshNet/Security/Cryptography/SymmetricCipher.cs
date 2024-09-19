using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for symmetric cipher implementations.
    /// </summary>
    public abstract class SymmetricCipher : Cipher
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        protected byte[] Key { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        protected SymmetricCipher(byte[] key)
        {
            ThrowHelper.ThrowIfNull(key);

            Key = key;
        }
    }
}
