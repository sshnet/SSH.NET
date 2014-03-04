using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
	/// <summary>
	/// SHA1 algorithm implementation
	/// </summary>
	public sealed class SHA1Hash : HashAlgorithm
	{
		private const int DIGEST_SIZE = 20;

		private const uint Y1 = 0x5a827999;

		private const uint Y2 = 0x6ed9eba1;

		private const uint Y3 = 0x8f1bbcdc;

		private const uint Y4 = 0xca62c1d6;

		private uint H1, H2, H3, H4, H5;
		
		private readonly uint[] _hashValue = new uint[80];
		
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
		/// Initializes a new instance of the <see cref="SHA1Hash"/> class.
		/// </summary>
		public SHA1Hash()
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

			this._hashValue[14] = (uint)((ulong)bitLength >> 32);
            this._hashValue[15] = (uint)((ulong)bitLength);


			this.ProcessBlock();

            UInt32ToBigEndian(H1, output, 0);
            UInt32ToBigEndian(H2, output, 4);
            UInt32ToBigEndian(H3, output, 8);
            UInt32ToBigEndian(H4, output, 12);
            UInt32ToBigEndian(H5, output, 16);

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

            H1 = 0x67452301;
            H2 = 0xefcdab89;
            H3 = 0x98badcfe;
            H4 = 0x10325476;
            H5 = 0xc3d2e1f0;

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
            this._hashValue[this._offset] = BigEndianToUInt32(input, inOff);

			if (++this._offset == 16)
			{
				this.ProcessBlock();
			}
		}

		private static uint F(uint u, uint v, uint w)
		{
			return (u & v) | (~u & w);
		}

		private static uint H(uint u, uint v, uint w)
		{
			return u ^ v ^ w;
		}

		private static uint G(uint u, uint v, uint w)
		{
			return (u & v) | (u & w) | (v & w);
		}

		private void ProcessBlock()
		{
			//
			// expand 16 word block into 80 word block.
			//
			for (int i = 16; i < 80; i++)
			{
				uint t = _hashValue[i - 3] ^ _hashValue[i - 8] ^ _hashValue[i - 14] ^ _hashValue[i - 16];
				_hashValue[i] = t << 1 | t >> 31;
			}

			//
			// set up working variables.
			//
			uint A = H1;
			uint B = H2;
			uint C = H3;
			uint D = H4;
			uint E = H5;

			//
			// round 1
			//
			int idx = 0;

            // E = rotateLeft(A, 5) + F(B, C, D) + E + X[idx++] + Y1
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + F(B, C, D) + _hashValue[idx++] + Y1;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + F(A, B, C) + _hashValue[idx++] + Y1;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + F(E, A, B) + _hashValue[idx++] + Y1;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + F(D, E, A) + _hashValue[idx++] + Y1;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + F(C, D, E) + _hashValue[idx++] + Y1;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + F(B, C, D) + E + X[idx++] + Y1
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + F(B, C, D) + _hashValue[idx++] + Y1;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + F(A, B, C) + _hashValue[idx++] + Y1;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + F(E, A, B) + _hashValue[idx++] + Y1;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + F(D, E, A) + _hashValue[idx++] + Y1;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + F(C, D, E) + _hashValue[idx++] + Y1;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + F(B, C, D) + E + X[idx++] + Y1
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + F(B, C, D) + _hashValue[idx++] + Y1;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + F(A, B, C) + _hashValue[idx++] + Y1;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + F(E, A, B) + _hashValue[idx++] + Y1;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + F(D, E, A) + _hashValue[idx++] + Y1;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + F(C, D, E) + _hashValue[idx++] + Y1;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + F(B, C, D) + E + X[idx++] + Y1
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + F(B, C, D) + _hashValue[idx++] + Y1;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + F(A, B, C) + _hashValue[idx++] + Y1;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + F(E, A, B) + _hashValue[idx++] + Y1;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + F(D, E, A) + _hashValue[idx++] + Y1;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + F(C, D, E) + _hashValue[idx++] + Y1;
            C = C << 30 | (C >> 2);
			//
			// round 2
			//
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y2
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y2;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y2;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y2;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y2;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y2;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y2
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y2;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y2;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y2;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y2;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y2;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y2
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y2;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y2;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y2;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y2;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y2;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y2
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y2;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y2;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y2;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y2;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y2;
            C = C << 30 | (C >> 2);

			//
			// round 3
            // E = rotateLeft(A, 5) + G(B, C, D) + E + X[idx++] + Y3
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + G(B, C, D) + _hashValue[idx++] + Y3;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + G(A, B, C) + _hashValue[idx++] + Y3;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + G(E, A, B) + _hashValue[idx++] + Y3;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + G(D, E, A) + _hashValue[idx++] + Y3;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + G(C, D, E) + _hashValue[idx++] + Y3;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + G(B, C, D) + E + X[idx++] + Y3
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + G(B, C, D) + _hashValue[idx++] + Y3;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + G(A, B, C) + _hashValue[idx++] + Y3;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + G(E, A, B) + _hashValue[idx++] + Y3;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + G(D, E, A) + _hashValue[idx++] + Y3;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + G(C, D, E) + _hashValue[idx++] + Y3;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + G(B, C, D) + E + X[idx++] + Y3
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + G(B, C, D) + _hashValue[idx++] + Y3;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + G(A, B, C) + _hashValue[idx++] + Y3;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + G(E, A, B) + _hashValue[idx++] + Y3;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + G(D, E, A) + _hashValue[idx++] + Y3;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + G(C, D, E) + _hashValue[idx++] + Y3;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + G(B, C, D) + E + X[idx++] + Y3
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + G(B, C, D) + _hashValue[idx++] + Y3;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + G(A, B, C) + _hashValue[idx++] + Y3;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + G(E, A, B) + _hashValue[idx++] + Y3;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + G(D, E, A) + _hashValue[idx++] + Y3;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + G(C, D, E) + _hashValue[idx++] + Y3;
            C = C << 30 | (C >> 2);

            //
			// round 4
			//
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y4
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y4;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y4;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y4;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y4;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y4;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y4
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y4;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y4;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y4;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y4;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y4;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y4
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y4;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y4;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y4;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y4;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y4;
            C = C << 30 | (C >> 2);
            // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y4
            // B = rotateLeft(B, 30)
            E += (A << 5 | (A >> 27)) + H(B, C, D) + _hashValue[idx++] + Y4;
            B = B << 30 | (B >> 2);

            D += (E << 5 | (E >> 27)) + H(A, B, C) + _hashValue[idx++] + Y4;
            A = A << 30 | (A >> 2);

            C += (D << 5 | (D >> 27)) + H(E, A, B) + _hashValue[idx++] + Y4;
            E = E << 30 | (E >> 2);

            B += (C << 5 | (C >> 27)) + H(D, E, A) + _hashValue[idx++] + Y4;
            D = D << 30 | (D >> 2);

            A += (B << 5 | (B >> 27)) + H(C, D, E) + _hashValue[idx++] + Y4;
            C = C << 30 | (C >> 2);

			H1 += A;
			H2 += B;
			H3 += C;
			H4 += D;
			H5 += E;

			//
			// reset start of the buffer.
			//
			this._offset = 0;
            for (int i = 0; i < this._hashValue.Length; i++)
            {
                this._hashValue[i] = 0;
            }
		}

        private static uint BigEndianToUInt32(byte[] bs, int off)
		{
			uint n = (uint)bs[off] << 24;
			n |= (uint)bs[++off] << 16;
			n |= (uint)bs[++off] << 8;
			n |= (uint)bs[++off];
			return n;
		}

        private static void UInt32ToBigEndian(uint n, byte[] bs, int off)
		{
			bs[off] = (byte)(n >> 24);
			bs[++off] = (byte)(n >> 16);
			bs[++off] = (byte)(n >> 8);
			bs[++off] = (byte)(n);
		}
	}
}
