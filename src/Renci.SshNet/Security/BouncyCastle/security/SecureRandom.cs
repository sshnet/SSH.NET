using System;
using System.Threading;

using Renci.SshNet.Security.Org.BouncyCastle.Crypto;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Digests;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Prng;
using Renci.SshNet.Security.Org.BouncyCastle.Utilities;

namespace Renci.SshNet.Security.Org.BouncyCastle.Security
{
    internal class SecureRandom
        : Random
    {
        private static long counter = Times.NanoTime();

       private static long NextCounterValue()
        {
            return Interlocked.Increment(ref counter);
        }

        private static readonly SecureRandom master = new SecureRandom(new CryptoApiRandomGenerator());
        private static SecureRandom Master
        {
            get { return master; }
        }

        private static DigestRandomGenerator CreatePrng(IDigest digest, bool autoSeed)
        {
            DigestRandomGenerator prng = new DigestRandomGenerator(digest);
            if (autoSeed)
            {
                prng.AddSeedMaterial(NextCounterValue());
                prng.AddSeedMaterial(GetNextBytes(Master, digest.GetDigestSize()));
            }
            return prng;
        }

        public static byte[] GetNextBytes(SecureRandom secureRandom, int length)
        {
            byte[] result = new byte[length];
            secureRandom.NextBytes(result);
            return result;
        }

        protected readonly IRandomGenerator generator;

        public SecureRandom()
            : this(CreatePrng(new Sha256Digest(), true))
        {
        }

        /// <summary>Use the specified instance of IRandomGenerator as random source.</summary>
        /// <remarks>
        /// This constructor performs no seeding of either the <c>IRandomGenerator</c> or the
        /// constructed <c>SecureRandom</c>. It is the responsibility of the client to provide
        /// proper seed material as necessary/appropriate for the given <c>IRandomGenerator</c>
        /// implementation.
        /// </remarks>
        /// <param name="generator">The source to generate all random bytes from.</param>
        public SecureRandom(IRandomGenerator generator)
            : base(0)
        {
            this.generator = generator;
        }

        public virtual byte[] GenerateSeed(int length)
        {
            return GetNextBytes(Master, length);
        }

        public virtual void SetSeed(byte[] seed)
        {
            generator.AddSeedMaterial(seed);
        }

        public virtual void SetSeed(long seed)
        {
            generator.AddSeedMaterial(seed);
        }

        public override int Next()
        {
            return NextInt() & int.MaxValue;
        }

        public override int Next(int maxValue)
        {

            if (maxValue < 2)
            {
                if (maxValue < 0)
                    throw new ArgumentOutOfRangeException("maxValue", "cannot be negative");

                return 0;
            }

            int bits;

            // Test whether maxValue is a power of 2
            if ((maxValue & (maxValue - 1)) == 0)
            {
                bits = NextInt() & int.MaxValue;
                return (int)(((long)bits * maxValue) >> 31);
            }

            int result;
            do
            {
                bits = NextInt() & int.MaxValue;
                result = bits % maxValue;
            }
            while (bits - result + (maxValue - 1) < 0); // Ignore results near overflow

            return result;
        }

        public override int Next(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
            {
                if (maxValue == minValue)
                    return minValue;

                throw new ArgumentException("maxValue cannot be less than minValue");
            }

            int diff = maxValue - minValue;
            if (diff > 0)
                return minValue + Next(diff);

            for (;;)
            {
                int i = NextInt();

                if (i >= minValue && i < maxValue)
                    return i;
            }
        }

        public override void NextBytes(byte[] buf)
        {
            generator.NextBytes(buf);
        }

        public virtual void NextBytes(byte[] buf, int off, int len)
        {
            generator.NextBytes(buf, off, len);
        }

        private static readonly double DoubleScale = System.Math.Pow(2.0, 64.0);

        public override double NextDouble()
        {
            return Convert.ToDouble((ulong) NextLong()) / DoubleScale;
        }

        public virtual int NextInt()
        {
            byte[] bytes = new byte[4];
            NextBytes(bytes);

            uint result = bytes[0];
            result <<= 8;
            result |= bytes[1];
            result <<= 8;
            result |= bytes[2];
            result <<= 8;
            result |= bytes[3];
            return (int)result;
        }

        public virtual long NextLong()
        {
            return ((long)(uint) NextInt() << 32) | (long)(uint) NextInt();
        }
    }
}
