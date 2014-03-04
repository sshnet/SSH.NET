using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
	/// <summary>
	/// SHA256 algorithm implementation.
	/// </summary>
	public class SHA256Hash : HashAlgorithm
	{
		private const int DIGEST_SIZE = 32;

		private uint H1, H2, H3, H4, H5, H6, H7, H8;

		private readonly uint[] X = new uint[64];

		private int _offset;

		private readonly byte[] _buffer;

		private int _bufferOffset;

		private long _byteCount;

		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <returns>The size, in bits, of the computed hash code.</returns>
		public override int HashSize
		{
			get
			{
				return DIGEST_SIZE * 8;
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
		/// Initializes a new instance of the <see cref="SHA1"/> class.
		/// </summary>
		public SHA256Hash()
		{
			this._buffer = new byte[4];
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
			var output = new byte[DIGEST_SIZE];
			long bitLength = (this._byteCount << 3);

			//
			// add the pad bytes.
			//
			this.Update((byte)128);

			while (this._bufferOffset != 0)
				this.Update((byte)0);

			if (this._offset > 14)
			{
				this.ProcessBlock();
			}

			X[14] = (uint)((ulong)bitLength >> 32);
			X[15] = (uint)((ulong)bitLength);


			this.ProcessBlock();

			UInt32_To_BE((uint)H1, output, 0);
			UInt32_To_BE((uint)H2, output, 0 + 4);
			UInt32_To_BE((uint)H3, output, 0 + 8);
			UInt32_To_BE((uint)H4, output, 0 + 12);
			UInt32_To_BE((uint)H5, output, 0 + 16);
			UInt32_To_BE((uint)H6, output, 0 + 20);
			UInt32_To_BE((uint)H7, output, 0 + 24);
			UInt32_To_BE((uint)H8, output, 0 + 28);

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
            for (int i = 0; i < this._buffer.Length; i++)
            {
                this._buffer[i] = 0;
            }

            H1 = 0x6a09e667;
            H2 = 0xbb67ae85;
            H3 = 0x3c6ef372;
            H4 = 0xa54ff53a;
            H5 = 0x510e527f;
            H6 = 0x9b05688c;
            H7 = 0x1f83d9ab;
            H8 = 0x5be0cd19;

            this._offset = 0;
            for (int i = 0; i < this.X.Length; i++)
            {
                this.X[i] = 0;
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

		private static uint BE_To_UInt32(byte[] bs, int off)
		{
			uint n = (uint)bs[off] << 24;
			n |= (uint)bs[++off] << 16;
			n |= (uint)bs[++off] << 8;
			n |= (uint)bs[++off];
			return n;
		}

		private static void UInt32_To_BE(uint n, byte[] bs, int off)
		{
			bs[off] = (byte)(n >> 24);
			bs[++off] = (byte)(n >> 16);
			bs[++off] = (byte)(n >> 8);
			bs[++off] = (byte)(n);
		}

		private void ProcessWord(byte[] input, int inOff)
		{
			X[this._offset] = BE_To_UInt32(input, inOff);

			if (++this._offset == 16)
			{
				ProcessBlock();
			}
		}

		private void ProcessLength(long bitLength)
		{
			if (this._offset > 14)
			{
				ProcessBlock();
			}

			X[14] = (uint)((ulong)bitLength >> 32);
			X[15] = (uint)((ulong)bitLength);
		}

		private void ProcessBlock()
		{
			//
			// expand 16 word block into 64 word blocks.
			//
			for (int ti = 16; ti <= 63; ti++)
			{
				X[ti] = Theta1(X[ti - 2]) + X[ti - 7] + Theta0(X[ti - 15]) + X[ti - 16];
			}

			//
			// set up working variables.
			//
			uint a = H1;
			uint b = H2;
			uint c = H3;
			uint d = H4;
			uint e = H5;
			uint f = H6;
			uint g = H7;
			uint h = H8;

			int t = 0;
			for (int i = 0; i < 8; ++i)
			{
				// t = 8 * i
				h += Sum1Ch(e, f, g) + K[t] + X[t];
				d += h;
				h += Sum0Maj(a, b, c);
				++t;

				// t = 8 * i + 1
				g += Sum1Ch(d, e, f) + K[t] + X[t];
				c += g;
				g += Sum0Maj(h, a, b);
				++t;

				// t = 8 * i + 2
				f += Sum1Ch(c, d, e) + K[t] + X[t];
				b += f;
				f += Sum0Maj(g, h, a);
				++t;

				// t = 8 * i + 3
				e += Sum1Ch(b, c, d) + K[t] + X[t];
				a += e;
				e += Sum0Maj(f, g, h);
				++t;

				// t = 8 * i + 4
				d += Sum1Ch(a, b, c) + K[t] + X[t];
				h += d;
				d += Sum0Maj(e, f, g);
				++t;

				// t = 8 * i + 5
				c += Sum1Ch(h, a, b) + K[t] + X[t];
				g += c;
				c += Sum0Maj(d, e, f);
				++t;

				// t = 8 * i + 6
				b += Sum1Ch(g, h, a) + K[t] + X[t];
				f += b;
				b += Sum0Maj(c, d, e);
				++t;

				// t = 8 * i + 7
				a += Sum1Ch(f, g, h) + K[t] + X[t];
				e += a;
				a += Sum0Maj(b, c, d);
				++t;
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

		private static uint Sum1Ch(uint x, uint y, uint z)
		{
			//			return Sum1(x) + Ch(x, y, z);
			return (((x >> 6) | (x << 26)) ^ ((x >> 11) | (x << 21)) ^ ((x >> 25) | (x << 7)))
				+ ((x & y) ^ ((~x) & z));
		}

		private static uint Sum0Maj(uint x, uint y, uint z)
		{
			//			return Sum0(x) + Maj(x, y, z);
			return (((x >> 2) | (x << 30)) ^ ((x >> 13) | (x << 19)) ^ ((x >> 22) | (x << 10)))
				+ ((x & y) ^ (x & z) ^ (y & z));
		}

		private static uint Theta0(uint x)
		{
			return ((x >> 7) | (x << 25)) ^ ((x >> 18) | (x << 14)) ^ (x >> 3);
		}

		private static uint Theta1(uint x)
		{
			return ((x >> 17) | (x << 15)) ^ ((x >> 19) | (x << 13)) ^ (x >> 10);
		}

        /// <summary>
        /// The SHA-256 Constants (represent the first 32 bits of the fractional parts of the cube roots of the first sixty-four prime numbers)
        /// </summary>
		private static readonly uint[] K = {
			0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
			0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
			0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
			0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
			0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
			0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
			0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
			0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
			0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
			0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
			0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
			0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
			0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
			0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
			0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
			0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
		};
	}
}
