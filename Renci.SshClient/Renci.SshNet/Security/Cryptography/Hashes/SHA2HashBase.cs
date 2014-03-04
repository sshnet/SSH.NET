using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// SHA256 algorithm implementation.
    /// </summary>
    public abstract class SHA2HashBase : HashAlgorithm
    {
        protected ulong H1, H2, H3, H4, H5, H6, H7, H8;

        private readonly ulong[] X = new ulong[80];

        private int _offset;

        private readonly byte[] _buffer;

        private int _bufferOffset;

        private long _byteCount1;

        private long _byteCount2;

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>Always true.</returns>
        public override bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        public override bool CanTransformMultipleBlocks
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SHA512Hash" /> class.
        /// </summary>
        public SHA2HashBase()
        {
            this._buffer = new byte[8];

            this.Initialize();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            // Fill the current word
            while ((this._bufferOffset != 0) && (cbSize > 0))
            {
                this.Update(array[ibStart]);

                ibStart++;
                cbSize--;
            }

            // Process whole words.
            while (cbSize > this._buffer.Length)
            {
                ProcessWord(array, ibStart);

                ibStart += this._buffer.Length;
                cbSize -= this._buffer.Length;
                this._byteCount1 += this._buffer.Length;
            }

            // Load in the remainder.
            while (cbSize > 0)
            {
                this.Update(array[ibStart]);

                ibStart++;
                cbSize--;
            }
        }

        public override void Initialize()
        {
            this._byteCount1 = 0;
            this._byteCount2 = 0;

            this._bufferOffset = 0;
            for (int i = 0; i < this._buffer.Length; i++)
            {
                this._buffer[i] = 0;
            }

            this._offset = 0;
            for (int i = 0; i < this.X.Length; i++)
            {
                this.X[i] = 0;
            }
        }

        protected void Finish()
        {
            this.AdjustByteCounts();

            long lowBitLength = this._byteCount1 << 3;
            long hiBitLength = this._byteCount2;

            //
            // add the pad bytes.
            //
            this.Update((byte)128);

            while (this._bufferOffset != 0)
            {
                this.Update((byte)0);
            }

            this.ProcessLength(lowBitLength, hiBitLength);

            this.ProcessBlock();
        }

        private void Update(byte input)
        {
            this._buffer[_bufferOffset++] = input;

            if (this._bufferOffset == this._buffer.Length)
            {
                this.ProcessWord(this._buffer, 0);
                this._bufferOffset = 0;
            }

            this._byteCount1++;
        }

        private void ProcessWord(byte[] input, int inOff)
        {
            this.X[_offset] = SHA512Hash.BE_To_UInt64(input, inOff);

            if (++_offset == 16)
            {
                ProcessBlock();
            }
        }

        internal void ProcessLength(long lowW, long hiW)
        {
            if (_offset > 14)
            {
                this.ProcessBlock();
            }

            this.X[14] = (ulong)hiW;
            this.X[15] = (ulong)lowW;
        }

        private void ProcessBlock()
        {
            this.AdjustByteCounts();

            //
            // expand 16 word block into 80 word blocks.
            //
            for (int ti = 16; ti <= 79; ++ti)
            {
                X[ti] = Sigma1(X[ti - 2]) + X[ti - 7] + Sigma0(X[ti - 15]) + X[ti - 16];
            }

            //
            // set up working variables.
            //
            ulong a = H1;
            ulong b = H2;
            ulong c = H3;
            ulong d = H4;
            ulong e = H5;
            ulong f = H6;
            ulong g = H7;
            ulong h = H8;

            int t = 0;
            for (int i = 0; i < 10; i++)
            {
                // t = 8 * i
                h += Sum1(e) + Ch(e, f, g) + K[t] + X[t++];
                d += h;
                h += Sum0(a) + Maj(a, b, c);

                // t = 8 * i + 1
                g += Sum1(d) + Ch(d, e, f) + K[t] + X[t++];
                c += g;
                g += Sum0(h) + Maj(h, a, b);

                // t = 8 * i + 2
                f += Sum1(c) + Ch(c, d, e) + K[t] + X[t++];
                b += f;
                f += Sum0(g) + Maj(g, h, a);

                // t = 8 * i + 3
                e += Sum1(b) + Ch(b, c, d) + K[t] + X[t++];
                a += e;
                e += Sum0(f) + Maj(f, g, h);

                // t = 8 * i + 4
                d += Sum1(a) + Ch(a, b, c) + K[t] + X[t++];
                h += d;
                d += Sum0(e) + Maj(e, f, g);

                // t = 8 * i + 5
                c += Sum1(h) + Ch(h, a, b) + K[t] + X[t++];
                g += c;
                c += Sum0(d) + Maj(d, e, f);

                // t = 8 * i + 6
                b += Sum1(g) + Ch(g, h, a) + K[t] + X[t++];
                f += b;
                b += Sum0(c) + Maj(c, d, e);

                // t = 8 * i + 7
                a += Sum1(f) + Ch(f, g, h) + K[t] + X[t++];
                e += a;
                a += Sum0(b) + Maj(b, c, d);
            }

            H1 += a;
            H2 += b;
            H3 += c;
            H4 += d;
            H5 += e;
            H6 += f;
            H7 += g;
            H8 += h;

            //
            // reset the offset and clean out the word buffer.
            //
            this._offset = 0;
            for (int i = 0; i < this.X.Length; i++)
            {
                this.X[i] = 0;
            }
        }

        /// <summary>
        /// Adjust the byte counts so that byteCount2 represents the upper long (less 3 bits) word of the byte count.
        /// </summary>
        private void AdjustByteCounts()
        {
            if (this._byteCount1 > 0x1fffffffffffffffL)
            {
                this._byteCount2 += (long)((ulong)this._byteCount1 >> 61);
                this._byteCount1 &= 0x1fffffffffffffffL;
            }
        }

        /* SHA-384 and SHA-512 functions (as for SHA-256 but for longs) */
        private static ulong Ch(ulong x, ulong y, ulong z)
        {
            return (x & y) ^ (~x & z);
        }

        private static ulong Maj(ulong x, ulong y, ulong z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }

        private static ulong Sum0(ulong x)
        {
            return ((x << 36) | (x >> 28)) ^ ((x << 30) | (x >> 34)) ^ ((x << 25) | (x >> 39));
        }

        private static ulong Sum1(ulong x)
        {
            return ((x << 50) | (x >> 14)) ^ ((x << 46) | (x >> 18)) ^ ((x << 23) | (x >> 41));
        }

        private static ulong Sigma0(ulong x)
        {
            return ((x << 63) | (x >> 1)) ^ ((x << 56) | (x >> 8)) ^ (x >> 7);
        }

        private static ulong Sigma1(ulong x)
        {
            return ((x << 45) | (x >> 19)) ^ ((x << 3) | (x >> 61)) ^ (x >> 6);
        }

        /* SHA-384 and SHA-512 Constants
         * (represent the first 64 bits of the fractional parts of the
         * cube roots of the first sixty-four prime numbers)
         */
        private static readonly ulong[] K =
		{
			0x428a2f98d728ae22, 0x7137449123ef65cd, 0xb5c0fbcfec4d3b2f, 0xe9b5dba58189dbbc,
			0x3956c25bf348b538, 0x59f111f1b605d019, 0x923f82a4af194f9b, 0xab1c5ed5da6d8118,
			0xd807aa98a3030242, 0x12835b0145706fbe, 0x243185be4ee4b28c, 0x550c7dc3d5ffb4e2,
			0x72be5d74f27b896f, 0x80deb1fe3b1696b1, 0x9bdc06a725c71235, 0xc19bf174cf692694,
			0xe49b69c19ef14ad2, 0xefbe4786384f25e3, 0x0fc19dc68b8cd5b5, 0x240ca1cc77ac9c65,
			0x2de92c6f592b0275, 0x4a7484aa6ea6e483, 0x5cb0a9dcbd41fbd4, 0x76f988da831153b5,
			0x983e5152ee66dfab, 0xa831c66d2db43210, 0xb00327c898fb213f, 0xbf597fc7beef0ee4,
			0xc6e00bf33da88fc2, 0xd5a79147930aa725, 0x06ca6351e003826f, 0x142929670a0e6e70,
			0x27b70a8546d22ffc, 0x2e1b21385c26c926, 0x4d2c6dfc5ac42aed, 0x53380d139d95b3df,
			0x650a73548baf63de, 0x766a0abb3c77b2a8, 0x81c2c92e47edaee6, 0x92722c851482353b,
			0xa2bfe8a14cf10364, 0xa81a664bbc423001, 0xc24b8b70d0f89791, 0xc76c51a30654be30,
			0xd192e819d6ef5218, 0xd69906245565a910, 0xf40e35855771202a, 0x106aa07032bbd1b8,
			0x19a4c116b8d2d0c8, 0x1e376c085141ab53, 0x2748774cdf8eeb99, 0x34b0bcb5e19b48a8,
			0x391c0cb3c5c95a63, 0x4ed8aa4ae3418acb, 0x5b9cca4f7763e373, 0x682e6ff3d6b2b8a3,
			0x748f82ee5defb2fc, 0x78a5636f43172f60, 0x84c87814a1f0ab72, 0x8cc702081a6439ec,
			0x90befffa23631e28, 0xa4506cebde82bde9, 0xbef9a3f7b2c67915, 0xc67178f2e372532b,
			0xca273eceea26619c, 0xd186b8c721c0c207, 0xeada7dd6cde0eb1e, 0xf57d4f7fee6ed178,
			0x06f067aa72176fba, 0x0a637dc5a2c898a6, 0x113f9804bef90dae, 0x1b710b35131c471b,
			0x28db77f523047d84, 0x32caab7b40c72493, 0x3c9ebe0a15c9bebc, 0x431d67c49c100d4c,
			0x4cc5d4becb3e42b6, 0x597f299cfc657e2a, 0x5fcb6fab3ad6faec, 0x6c44198c4a475817
		};

        protected static void UInt32_To_BE(uint n, byte[] bs, int off)
        {
            bs[off] = (byte)(n >> 24);
            bs[++off] = (byte)(n >> 16);
            bs[++off] = (byte)(n >> 8);
            bs[++off] = (byte)(n);
        }
        protected static void UInt64_To_BE(ulong n, byte[] bs, int off)
        {
            UInt32_To_BE((uint)(n >> 32), bs, off);
            UInt32_To_BE((uint)(n), bs, off + 4);
        }
        protected static ulong BE_To_UInt64(byte[] bs)
        {
            uint hi = BE_To_UInt32(bs);
            uint lo = BE_To_UInt32(bs, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }
        protected static ulong BE_To_UInt64(byte[] bs, int off)
        {
            uint hi = BE_To_UInt32(bs, off);
            uint lo = BE_To_UInt32(bs, off + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }
        protected static uint BE_To_UInt32(byte[] bs, int off)
        {
            uint n = (uint)bs[off] << 24;
            n |= (uint)bs[++off] << 16;
            n |= (uint)bs[++off] << 8;
            n |= (uint)bs[++off];
            return n;
        }
        protected static uint BE_To_UInt32(byte[] bs)
        {
            uint n = (uint)bs[0] << 24;
            n |= (uint)bs[1] << 16;
            n |= (uint)bs[2] << 8;
            n |= (uint)bs[3];
            return n;
        }
    }
}
