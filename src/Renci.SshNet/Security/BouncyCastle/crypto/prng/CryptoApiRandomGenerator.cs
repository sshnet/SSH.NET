using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Org.BouncyCastle.Crypto.Prng
{
    internal class CryptoApiRandomGenerator
        : IRandomGenerator
    {
        private readonly RandomNumberGenerator rndProv;

        public CryptoApiRandomGenerator()
#if FEATURE_RNG_CREATE || FEATURE_RNG_CSP
            : this(Abstractions.CryptoAbstraction.CreateRandomNumberGenerator())
#endif
        {
        }

        public CryptoApiRandomGenerator(RandomNumberGenerator rng)
        {
            this.rndProv = rng;
        }

        #region IRandomGenerator Members

        public virtual void AddSeedMaterial(byte[] seed)
        {
            // We don't care about the seed
        }

        public virtual void AddSeedMaterial(long seed)
        {
            // We don't care about the seed
        }

        public virtual void NextBytes(byte[] bytes)
        {
#if FEATURE_RNG_CREATE || FEATURE_RNG_CSP
            rndProv.GetBytes(bytes);
#else
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            var buffer = Windows.Security.Cryptography.CryptographicBuffer.GenerateRandom((uint)bytes.Length);
            System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.CopyTo(buffer, bytes);
#endif
        }

        public virtual void NextBytes(byte[] bytes, int start, int len)
        {
            if (start < 0)
                throw new ArgumentException("Start offset cannot be negative", "start");
            if (bytes.Length < (start + len))
                throw new ArgumentException("Byte array too small for requested offset and length");

            if (bytes.Length == len && start == 0) 
            {
                NextBytes(bytes);
            }
            else 
            {
                byte[] tmpBuf = new byte[len];
                NextBytes(tmpBuf);
                Array.Copy(tmpBuf, 0, bytes, start, len);
            }
        }

        #endregion
    }
}
