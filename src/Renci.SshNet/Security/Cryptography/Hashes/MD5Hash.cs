using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// MD5 algorithm implementation
    /// </summary>
    public sealed class MD5Hash : HashAlgorithm
    {
        private readonly byte[] _buffer = new byte[4];
        private int _bufferOffset;
        private long _byteCount;
        private int H1, H2, H3, H4;         // IV's
        private readonly int[] _hashValue = new int[16];
        private int _offset;

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <returns>The size, in bits, of the computed hash code.</returns>
        public override int HashSize
        {
            get
            {
                return 128;
            }
        }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The input block size.</returns>
        public override int InputBlockSize
        {
            get
            {
                return 64;
            }
        }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The output block size.</returns>
        public override int OutputBlockSize
        {
            get
            {
                return 64;
            }
        }

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
        /// Initializes a new instance of the <see cref="MD5Hash"/> class.
        /// </summary>
        public MD5Hash()
        {
            this.InternalInitialize();
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            //  Fill the current word
            while ((this._bufferOffset != 0) && (cbSize > 0))
            {
                this.Update(array[ibStart]);
                ibStart++;
                cbSize--;
            }

            //  Process whole words.
            while (cbSize > this._buffer.Length)
            {
                this.ProcessWord(array, ibStart);

                ibStart += this._buffer.Length;
                cbSize -= this._buffer.Length;
                this._byteCount += this._buffer.Length;
            }

            //  Load in the remainder.
            while (cbSize > 0)
            {
                this.Update(array[ibStart]);

                ibStart++;
                cbSize--;
            }
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>
        /// The computed hash code.
        /// </returns>
        protected override byte[] HashFinal()
        {
            long bitLength = (this._byteCount << 3);

            //  Add the pad bytes.
            this.Update((byte)128);

            while (this._bufferOffset != 0)
                this.Update((byte)0);

            if (this._offset > 14)
            {
                this.ProcessBlock();
            }

            this._hashValue[14] = (int)(bitLength & 0xffffffff);
            this._hashValue[15] = (int)((ulong)bitLength >> 32);

            this.ProcessBlock();

            var output = new byte[16];

            this.UnpackWord(H1, output, 0);
            this.UnpackWord(H2, output, 0 + 4);
            this.UnpackWord(H3, output, 0 + 8);
            this.UnpackWord(H4, output, 0 + 12);

            this.Initialize();

            return output;
        }

        /// <summary>
        /// Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm"/> class.
        /// </summary>
        public override void Initialize()
        {
            this.InternalInitialize();
        }

        private void InternalInitialize()
        {
            this._byteCount = 0;
            this._bufferOffset = 0;
            for (var i = 0; i < 4; i++)
            {
                this._buffer[i] = 0;
            }

            H1 = unchecked((int)0x67452301);
            H2 = unchecked((int)0xefcdab89);
            H3 = unchecked((int)0x98badcfe);
            H4 = unchecked((int)0x10325476);

            this._offset = 0;
            for (var i = 0; i != this._hashValue.Length; i++)
            {
                this._hashValue[i] = 0;
            }
        }

        private void Update(byte input)
        {
            this._buffer[this._bufferOffset++] = input;

            if (this._bufferOffset == this._buffer.Length)
            {
                this.ProcessWord(this._buffer, 0);
                this._bufferOffset = 0;
            }

            this._byteCount++;
        }

        private void ProcessWord(byte[] input, int inOff)
        {
            this._hashValue[this._offset++] = (input[inOff] & 0xff) | ((input[inOff + 1] & 0xff) << 8)
                | ((input[inOff + 2] & 0xff) << 16) | ((input[inOff + 3] & 0xff) << 24);

            if (this._offset == 16)
            {
                ProcessBlock();
            }
        }

        private void UnpackWord(int word, byte[] outBytes, int outOff)
        {
            outBytes[outOff] = (byte)word;
            outBytes[outOff + 1] = (byte)((uint)word >> 8);
            outBytes[outOff + 2] = (byte)((uint)word >> 16);
            outBytes[outOff + 3] = (byte)((uint)word >> 24);
        }

        //
        // round 1 left rotates
        //
        private const int S11 = 7;
        private const int S12 = 12;
        private const int S13 = 17;
        private const int S14 = 22;

        //
        // round 2 left rotates
        //
        private const int S21 = 5;
        private const int S22 = 9;
        private const int S23 = 14;
        private const int S24 = 20;

        //
        // round 3 left rotates
        //
        private const int S31 = 4;
        private const int S32 = 11;
        private const int S33 = 16;
        private const int S34 = 23;

        //
        // round 4 left rotates
        //
        private const int S41 = 6;
        private const int S42 = 10;
        private const int S43 = 15;
        private const int S44 = 21;

        /*
        * rotate int x left n bits.
        */
        private static int RotateLeft(int x, int n)
        {
            return (x << n) | (int)((uint)x >> (32 - n));
        }

        /*
        * F, G, H and I are the basic MD5 functions.
        */
        private static int F(int u, int v, int w)
        {
            return (u & v) | (~u & w);
        }

        private static int G(int u, int v, int w)
        {
            return (u & w) | (v & ~w);
        }

        private static int H(int u, int v, int w)
        {
            return u ^ v ^ w;
        }

        private static int K(int u, int v, int w)
        {
            return v ^ (u | ~w);
        }

        private void ProcessBlock()
        {
            int a = H1;
            int b = H2;
            int c = H3;
            int d = H4;

            //
            // Round 1 - F cycle, 16 times.
            //
            a = RotateLeft((a + F(b, c, d) + this._hashValue[0] + unchecked((int)0xd76aa478)), S11) + b;
            d = RotateLeft((d + F(a, b, c) + this._hashValue[1] + unchecked((int)0xe8c7b756)), S12) + a;
            c = RotateLeft((c + F(d, a, b) + this._hashValue[2] + unchecked((int)0x242070db)), S13) + d;
            b = RotateLeft((b + F(c, d, a) + this._hashValue[3] + unchecked((int)0xc1bdceee)), S14) + c;
            a = RotateLeft((a + F(b, c, d) + this._hashValue[4] + unchecked((int)0xf57c0faf)), S11) + b;
            d = RotateLeft((d + F(a, b, c) + this._hashValue[5] + unchecked((int)0x4787c62a)), S12) + a;
            c = RotateLeft((c + F(d, a, b) + this._hashValue[6] + unchecked((int)0xa8304613)), S13) + d;
            b = RotateLeft((b + F(c, d, a) + this._hashValue[7] + unchecked((int)0xfd469501)), S14) + c;
            a = RotateLeft((a + F(b, c, d) + this._hashValue[8] + unchecked((int)0x698098d8)), S11) + b;
            d = RotateLeft((d + F(a, b, c) + this._hashValue[9] + unchecked((int)0x8b44f7af)), S12) + a;
            c = RotateLeft((c + F(d, a, b) + this._hashValue[10] + unchecked((int)0xffff5bb1)), S13) + d;
            b = RotateLeft((b + F(c, d, a) + this._hashValue[11] + unchecked((int)0x895cd7be)), S14) + c;
            a = RotateLeft((a + F(b, c, d) + this._hashValue[12] + unchecked((int)0x6b901122)), S11) + b;
            d = RotateLeft((d + F(a, b, c) + this._hashValue[13] + unchecked((int)0xfd987193)), S12) + a;
            c = RotateLeft((c + F(d, a, b) + this._hashValue[14] + unchecked((int)0xa679438e)), S13) + d;
            b = RotateLeft((b + F(c, d, a) + this._hashValue[15] + unchecked((int)0x49b40821)), S14) + c;

            //
            // Round 2 - G cycle, 16 times.
            //
            a = RotateLeft((a + G(b, c, d) + this._hashValue[1] + unchecked((int)0xf61e2562)), S21) + b;
            d = RotateLeft((d + G(a, b, c) + this._hashValue[6] + unchecked((int)0xc040b340)), S22) + a;
            c = RotateLeft((c + G(d, a, b) + this._hashValue[11] + unchecked((int)0x265e5a51)), S23) + d;
            b = RotateLeft((b + G(c, d, a) + this._hashValue[0] + unchecked((int)0xe9b6c7aa)), S24) + c;
            a = RotateLeft((a + G(b, c, d) + this._hashValue[5] + unchecked((int)0xd62f105d)), S21) + b;
            d = RotateLeft((d + G(a, b, c) + this._hashValue[10] + unchecked((int)0x02441453)), S22) + a;
            c = RotateLeft((c + G(d, a, b) + this._hashValue[15] + unchecked((int)0xd8a1e681)), S23) + d;
            b = RotateLeft((b + G(c, d, a) + this._hashValue[4] + unchecked((int)0xe7d3fbc8)), S24) + c;
            a = RotateLeft((a + G(b, c, d) + this._hashValue[9] + unchecked((int)0x21e1cde6)), S21) + b;
            d = RotateLeft((d + G(a, b, c) + this._hashValue[14] + unchecked((int)0xc33707d6)), S22) + a;
            c = RotateLeft((c + G(d, a, b) + this._hashValue[3] + unchecked((int)0xf4d50d87)), S23) + d;
            b = RotateLeft((b + G(c, d, a) + this._hashValue[8] + unchecked((int)0x455a14ed)), S24) + c;
            a = RotateLeft((a + G(b, c, d) + this._hashValue[13] + unchecked((int)0xa9e3e905)), S21) + b;
            d = RotateLeft((d + G(a, b, c) + this._hashValue[2] + unchecked((int)0xfcefa3f8)), S22) + a;
            c = RotateLeft((c + G(d, a, b) + this._hashValue[7] + unchecked((int)0x676f02d9)), S23) + d;
            b = RotateLeft((b + G(c, d, a) + this._hashValue[12] + unchecked((int)0x8d2a4c8a)), S24) + c;

            //
            // Round 3 - H cycle, 16 times.
            //
            a = RotateLeft((a + H(b, c, d) + this._hashValue[5] + unchecked((int)0xfffa3942)), S31) + b;
            d = RotateLeft((d + H(a, b, c) + this._hashValue[8] + unchecked((int)0x8771f681)), S32) + a;
            c = RotateLeft((c + H(d, a, b) + this._hashValue[11] + unchecked((int)0x6d9d6122)), S33) + d;
            b = RotateLeft((b + H(c, d, a) + this._hashValue[14] + unchecked((int)0xfde5380c)), S34) + c;
            a = RotateLeft((a + H(b, c, d) + this._hashValue[1] + unchecked((int)0xa4beea44)), S31) + b;
            d = RotateLeft((d + H(a, b, c) + this._hashValue[4] + unchecked((int)0x4bdecfa9)), S32) + a;
            c = RotateLeft((c + H(d, a, b) + this._hashValue[7] + unchecked((int)0xf6bb4b60)), S33) + d;
            b = RotateLeft((b + H(c, d, a) + this._hashValue[10] + unchecked((int)0xbebfbc70)), S34) + c;
            a = RotateLeft((a + H(b, c, d) + this._hashValue[13] + unchecked((int)0x289b7ec6)), S31) + b;
            d = RotateLeft((d + H(a, b, c) + this._hashValue[0] + unchecked((int)0xeaa127fa)), S32) + a;
            c = RotateLeft((c + H(d, a, b) + this._hashValue[3] + unchecked((int)0xd4ef3085)), S33) + d;
            b = RotateLeft((b + H(c, d, a) + this._hashValue[6] + unchecked((int)0x04881d05)), S34) + c;
            a = RotateLeft((a + H(b, c, d) + this._hashValue[9] + unchecked((int)0xd9d4d039)), S31) + b;
            d = RotateLeft((d + H(a, b, c) + this._hashValue[12] + unchecked((int)0xe6db99e5)), S32) + a;
            c = RotateLeft((c + H(d, a, b) + this._hashValue[15] + unchecked((int)0x1fa27cf8)), S33) + d;
            b = RotateLeft((b + H(c, d, a) + this._hashValue[2] + unchecked((int)0xc4ac5665)), S34) + c;

            //
            // Round 4 - K cycle, 16 times.
            //
            a = RotateLeft((a + K(b, c, d) + this._hashValue[0] + unchecked((int)0xf4292244)), S41) + b;
            d = RotateLeft((d + K(a, b, c) + this._hashValue[7] + unchecked((int)0x432aff97)), S42) + a;
            c = RotateLeft((c + K(d, a, b) + this._hashValue[14] + unchecked((int)0xab9423a7)), S43) + d;
            b = RotateLeft((b + K(c, d, a) + this._hashValue[5] + unchecked((int)0xfc93a039)), S44) + c;
            a = RotateLeft((a + K(b, c, d) + this._hashValue[12] + unchecked((int)0x655b59c3)), S41) + b;
            d = RotateLeft((d + K(a, b, c) + this._hashValue[3] + unchecked((int)0x8f0ccc92)), S42) + a;
            c = RotateLeft((c + K(d, a, b) + this._hashValue[10] + unchecked((int)0xffeff47d)), S43) + d;
            b = RotateLeft((b + K(c, d, a) + this._hashValue[1] + unchecked((int)0x85845dd1)), S44) + c;
            a = RotateLeft((a + K(b, c, d) + this._hashValue[8] + unchecked((int)0x6fa87e4f)), S41) + b;
            d = RotateLeft((d + K(a, b, c) + this._hashValue[15] + unchecked((int)0xfe2ce6e0)), S42) + a;
            c = RotateLeft((c + K(d, a, b) + this._hashValue[6] + unchecked((int)0xa3014314)), S43) + d;
            b = RotateLeft((b + K(c, d, a) + this._hashValue[13] + unchecked((int)0x4e0811a1)), S44) + c;
            a = RotateLeft((a + K(b, c, d) + this._hashValue[4] + unchecked((int)0xf7537e82)), S41) + b;
            d = RotateLeft((d + K(a, b, c) + this._hashValue[11] + unchecked((int)0xbd3af235)), S42) + a;
            c = RotateLeft((c + K(d, a, b) + this._hashValue[2] + unchecked((int)0x2ad7d2bb)), S43) + d;
            b = RotateLeft((b + K(c, d, a) + this._hashValue[9] + unchecked((int)0xeb86d391)), S44) + c;

            H1 += a;
            H2 += b;
            H3 += c;
            H4 += d;

            //
            // reset the offset and clean out the word buffer.
            //
            this._offset = 0;
            for (int i = 0; i != this._hashValue.Length; i++)
            {
                this._hashValue[i] = 0;
            }
        }
    }
}
