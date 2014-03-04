using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Provides HMAC algorithm implementation.
    /// </summary>
    /// <typeparam name="T">Class that implements <see cref="T:System.Security.Cryptography.HashAlgorithm" />.</typeparam>
    public class HMac<T> : KeyedHashAlgorithm where T : HashAlgorithm, new()
    {
        private HashAlgorithm _hash;
        //private bool _isHashing;
        private byte[] _innerPadding;
        private byte[] _outerPadding;

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        protected int BlockSize
        {
            get
            {
                return this._hash.InputBlockSize;
            }
        }

        private HMac()
        {
            // Create the hash algorithms.
            this._hash = new T();
            this.HashSizeValue = this._hash.HashSize;
        }

        /// <summary>
        /// Rfc 2104.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="hashSizeValue">The size, in bits, of the computed hash code.</param>
        public HMac(byte[] key, int hashSizeValue)
            : this(key)
        {
            this.HashSizeValue = hashSizeValue;
        }

        /// <summary>
        /// Rfc 2104.
        /// </summary>
        /// <param name="key">The key.</param>
        public HMac(byte[] key)
            : this()
        {
            base.KeyValue = key;

            this.InternalInitialize();
        }


        /// <summary>
        /// Gets or sets the key to use in the hash algorithm.
        /// </summary>
        /// <returns>The key to use in the hash algorithm.</returns>
        public override byte[] Key
        {
            get
            {
                return (byte[])base.KeyValue.Clone();
            }
            set
            {
                this.SetKey(value);
            }
        }

        /// <summary>
        /// Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.
        /// </summary>
        public override void Initialize()
        {
            this.InternalInitialize();
        }

        /// <summary>
        /// Hashes the core.
        /// </summary>
        /// <param name="rgb">The RGB.</param>
        /// <param name="ib">The ib.</param>
        /// <param name="cb">The cb.</param>
        protected override void HashCore(byte[] rgb, int ib, int cb)
        {
            this._hash.TransformBlock(rgb, ib, cb, rgb, ib);
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>
        /// The computed hash code.
        /// </returns>
        protected override byte[] HashFinal()
        {
            // Finalize the original hash.
            this._hash.TransformFinalBlock(new byte[0], 0, 0);

            var hashValue = this._hash.Hash;

            // Write the outer array.
            this._hash.TransformBlock(this._outerPadding, 0, this.BlockSize, this._outerPadding, 0);

            // Write the inner hash and finalize the hash.            
            this._hash.TransformFinalBlock(hashValue, 0, hashValue.Length);

            return this._hash.Hash.Take(this.HashSize / 8).ToArray();
        }

        private void InternalInitialize()
        {
            this.SetKey(base.KeyValue);
        }

        private void SetKey(byte[] value)
        {
            this._hash.Initialize();

            if (value.Length > this.BlockSize)
            {
                this.KeyValue = this._hash.ComputeHash(value);
                // No need to call Initialize, ComputeHash does it automatically.
            }
            else
            {
                this.KeyValue = (byte[]) value.Clone();
            }

            this._innerPadding = new byte[this.BlockSize];
            this._outerPadding = new byte[this.BlockSize];

            // Compute inner and outer padding.
            for (var i = 0; i < this.KeyValue.Length; i++)
            {
                this._innerPadding[i] = (byte)(0x36 ^ this.KeyValue[i]);
                this._outerPadding[i] = (byte)(0x5C ^ this.KeyValue[i]);
            }
            for (var i = this.KeyValue.Length; i < this.BlockSize; i++)
            {
                this._innerPadding[i] = 0x36;
                this._outerPadding[i] = 0x5C;
            }

            this._hash.TransformBlock(this._innerPadding, 0, this.BlockSize, this._innerPadding, 0);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._hash != null)
            {
                this._hash.Clear();
                this._hash = null;
            }
        }
    }
}
