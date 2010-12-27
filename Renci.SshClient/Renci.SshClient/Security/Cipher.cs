using System.Collections.Generic;
using System.Linq;
using System;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents the abstract base class from which all implementations of cipher must inherit.
    /// </summary>
    public abstract class Cipher : Algorithm, IDisposable
    {
        /// <summary>
        /// Gets or sets the block size, in bits, of the cipher operation.
        /// </summary>
        /// <value>
        /// The block size, in bits.
        /// </value>
        public abstract int BlockSize { get; }

        /// <summary>
        /// Gets or sets the key size, in bits, of the secret key used by the cipher.
        /// </summary>
        /// <value>
        /// The key size, in bits.
        /// </value>
        public abstract int KeySize { get; }

        /// <summary>
        /// Gets the secret key for the cipher.
        /// </summary>
        protected byte[] Key { get; private set; }

        /// <summary>
        /// Gets the initialization vector (IV) for the cipher.
        /// </summary>
        protected byte[] Vector { get; private set; }

        /// <summary>
        /// Initializes the cipher.
        /// </summary>
        /// <param name="key">The secret key.</param>
        /// <param name="vector">The initialization vector.</param>
        public virtual void Init(IEnumerable<byte> key, IEnumerable<byte> vector)
        {
            this.Key = key.ToArray();
            this.Vector = vector.ToArray();
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Encrypted data</returns>
        public abstract IEnumerable<byte> Encrypt(IEnumerable<byte> data);

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Decrypted data</returns>
        public abstract IEnumerable<byte> Decrypt(IEnumerable<byte> data);
        
        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Cipher"/> is reclaimed by garbage collection.
        /// </summary>
        ~Cipher()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}
