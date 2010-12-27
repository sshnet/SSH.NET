using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents base class for hash algoritm classes
    /// </summary>
    public abstract class HMac : Algorithm, IDisposable
    {
        /// <summary>
        /// Instance of initialized hash algorithm that being used
        /// </summary>
        protected System.Security.Cryptography.HMAC _hmac;
        //  TODO:   Refactor code so this member will not be protected

        /// <summary>
        /// Initializes algorithm with specified key.
        /// </summary>
        /// <param name="key">The hash key.</param>
        public abstract void Init(IEnumerable<byte> key);

        /// <summary>
        /// Computes the hash value for the specified data.
        /// </summary>
        /// <param name="hashData">The input to compute the hash code for.</param>
        /// <returns>The hash</returns>
        internal byte[] ComputeHash(byte[] hashData)
        {
            return this._hmac.ComputeHash(hashData);
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size of the hash.
        /// </value>
        public int HashSize
        {
            get
            {
                return this._hmac.HashSize;
            }
        }

        #region IDisposable Members

        private bool _disposed = false;

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
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._hmac != null)
                    {
                        this._hmac.Dispose();
                    }
                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="HMac"/> is reclaimed by garbage collection.
        /// </summary>
        ~HMac()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
