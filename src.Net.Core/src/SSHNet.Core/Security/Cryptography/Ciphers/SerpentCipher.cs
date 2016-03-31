using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
	/// <summary>
	/// Implements Serpent cipher algorithm.
	/// </summary>
	public sealed class SerpentCipher : BlockCipher
	{
	    private const int ROUNDS = 32;

	    private const int PHI = unchecked((int) 0x9E3779B9); // (Sqrt(5) - 1) * 2**31

	    private readonly int[] _workingKey;

		private int _x0, _x1, _x2, _x3;    // registers

		/// <summary>
		/// Initializes a new instance of the <see cref="SerpentCipher"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="padding">The padding.</param>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
		/// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
		public SerpentCipher(byte[] key, CipherMode mode, CipherPadding padding)
			: base(key, 16, mode, padding)
		{
			var keySize = key.Length * 8;

			if (!(keySize == 128 || keySize == 192 || keySize == 256))
				throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", keySize));

			this._workingKey = this.MakeWorkingKey(key);
		}

		/// <summary>
		/// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input data to encrypt.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">The output to which to write encrypted data.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>
		/// The number of bytes encrypted.
		/// </returns>
		public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (inputCount != this.BlockSize)
				throw new ArgumentException("inputCount");

			this._x3 = BytesToWord(inputBuffer, inputOffset);
			this._x2 = BytesToWord(inputBuffer, inputOffset + 4);
			this._x1 = BytesToWord(inputBuffer, inputOffset + 8);
			this._x0 = BytesToWord(inputBuffer, inputOffset + 12);

			Sb0(this._workingKey[0] ^ this._x0, this._workingKey[1] ^ this._x1, this._workingKey[2] ^ this._x2, this._workingKey[3] ^ this._x3); LT();
			Sb1(this._workingKey[4] ^ this._x0, this._workingKey[5] ^ this._x1, this._workingKey[6] ^ this._x2, this._workingKey[7] ^ this._x3); LT();
			Sb2(this._workingKey[8] ^ this._x0, this._workingKey[9] ^ this._x1, this._workingKey[10] ^ this._x2, this._workingKey[11] ^ this._x3); LT();
			Sb3(this._workingKey[12] ^ this._x0, this._workingKey[13] ^ this._x1, this._workingKey[14] ^ this._x2, this._workingKey[15] ^ this._x3); LT();
			Sb4(this._workingKey[16] ^ this._x0, this._workingKey[17] ^ this._x1, this._workingKey[18] ^ this._x2, this._workingKey[19] ^ this._x3); LT();
			Sb5(this._workingKey[20] ^ this._x0, this._workingKey[21] ^ this._x1, this._workingKey[22] ^ this._x2, this._workingKey[23] ^ this._x3); LT();
			Sb6(this._workingKey[24] ^ this._x0, this._workingKey[25] ^ this._x1, this._workingKey[26] ^ this._x2, this._workingKey[27] ^ this._x3); LT();
			Sb7(this._workingKey[28] ^ this._x0, this._workingKey[29] ^ this._x1, this._workingKey[30] ^ this._x2, this._workingKey[31] ^ this._x3); LT();
			Sb0(this._workingKey[32] ^ this._x0, this._workingKey[33] ^ this._x1, this._workingKey[34] ^ this._x2, this._workingKey[35] ^ this._x3); LT();
			Sb1(this._workingKey[36] ^ this._x0, this._workingKey[37] ^ this._x1, this._workingKey[38] ^ this._x2, this._workingKey[39] ^ this._x3); LT();
			Sb2(this._workingKey[40] ^ this._x0, this._workingKey[41] ^ this._x1, this._workingKey[42] ^ this._x2, this._workingKey[43] ^ this._x3); LT();
			Sb3(this._workingKey[44] ^ this._x0, this._workingKey[45] ^ this._x1, this._workingKey[46] ^ this._x2, this._workingKey[47] ^ this._x3); LT();
			Sb4(this._workingKey[48] ^ this._x0, this._workingKey[49] ^ this._x1, this._workingKey[50] ^ this._x2, this._workingKey[51] ^ this._x3); LT();
			Sb5(this._workingKey[52] ^ this._x0, this._workingKey[53] ^ this._x1, this._workingKey[54] ^ this._x2, this._workingKey[55] ^ this._x3); LT();
			Sb6(this._workingKey[56] ^ this._x0, this._workingKey[57] ^ this._x1, this._workingKey[58] ^ this._x2, this._workingKey[59] ^ this._x3); LT();
			Sb7(this._workingKey[60] ^ this._x0, this._workingKey[61] ^ this._x1, this._workingKey[62] ^ this._x2, this._workingKey[63] ^ this._x3); LT();
			Sb0(this._workingKey[64] ^ this._x0, this._workingKey[65] ^ this._x1, this._workingKey[66] ^ this._x2, this._workingKey[67] ^ this._x3); LT();
			Sb1(this._workingKey[68] ^ this._x0, this._workingKey[69] ^ this._x1, this._workingKey[70] ^ this._x2, this._workingKey[71] ^ this._x3); LT();
			Sb2(this._workingKey[72] ^ this._x0, this._workingKey[73] ^ this._x1, this._workingKey[74] ^ this._x2, this._workingKey[75] ^ this._x3); LT();
			Sb3(this._workingKey[76] ^ this._x0, this._workingKey[77] ^ this._x1, this._workingKey[78] ^ this._x2, this._workingKey[79] ^ this._x3); LT();
			Sb4(this._workingKey[80] ^ this._x0, this._workingKey[81] ^ this._x1, this._workingKey[82] ^ this._x2, this._workingKey[83] ^ this._x3); LT();
			Sb5(this._workingKey[84] ^ this._x0, this._workingKey[85] ^ this._x1, this._workingKey[86] ^ this._x2, this._workingKey[87] ^ this._x3); LT();
			Sb6(this._workingKey[88] ^ this._x0, this._workingKey[89] ^ this._x1, this._workingKey[90] ^ this._x2, this._workingKey[91] ^ this._x3); LT();
			Sb7(this._workingKey[92] ^ this._x0, this._workingKey[93] ^ this._x1, this._workingKey[94] ^ this._x2, this._workingKey[95] ^ this._x3); LT();
			Sb0(this._workingKey[96] ^ this._x0, this._workingKey[97] ^ this._x1, this._workingKey[98] ^ this._x2, this._workingKey[99] ^ this._x3); LT();
			Sb1(this._workingKey[100] ^ this._x0, this._workingKey[101] ^ this._x1, this._workingKey[102] ^ this._x2, this._workingKey[103] ^ this._x3); LT();
			Sb2(this._workingKey[104] ^ this._x0, this._workingKey[105] ^ this._x1, this._workingKey[106] ^ this._x2, this._workingKey[107] ^ this._x3); LT();
			Sb3(this._workingKey[108] ^ this._x0, this._workingKey[109] ^ this._x1, this._workingKey[110] ^ this._x2, this._workingKey[111] ^ this._x3); LT();
			Sb4(this._workingKey[112] ^ this._x0, this._workingKey[113] ^ this._x1, this._workingKey[114] ^ this._x2, this._workingKey[115] ^ this._x3); LT();
			Sb5(this._workingKey[116] ^ this._x0, this._workingKey[117] ^ this._x1, this._workingKey[118] ^ this._x2, this._workingKey[119] ^ this._x3); LT();
			Sb6(this._workingKey[120] ^ this._x0, this._workingKey[121] ^ this._x1, this._workingKey[122] ^ this._x2, this._workingKey[123] ^ this._x3); LT();
			Sb7(this._workingKey[124] ^ this._x0, this._workingKey[125] ^ this._x1, this._workingKey[126] ^ this._x2, this._workingKey[127] ^ this._x3);

			WordToBytes(this._workingKey[131] ^ this._x3, outputBuffer, outputOffset);
			WordToBytes(this._workingKey[130] ^ this._x2, outputBuffer, outputOffset + 4);
			WordToBytes(this._workingKey[129] ^ this._x1, outputBuffer, outputOffset + 8);
			WordToBytes(this._workingKey[128] ^ this._x0, outputBuffer, outputOffset + 12);

			return this.BlockSize;
		}

		/// <summary>
		/// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input data to decrypt.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">The output to which to write decrypted data.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>
		/// The number of bytes decrypted.
		/// </returns>
		public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (inputCount != this.BlockSize)
				throw new ArgumentException("inputCount");

			this._x3 = this._workingKey[131] ^ BytesToWord(inputBuffer, inputOffset);
			this._x2 = this._workingKey[130] ^ BytesToWord(inputBuffer, inputOffset + 4);
			this._x1 = this._workingKey[129] ^ BytesToWord(inputBuffer, inputOffset + 8);
			this._x0 = this._workingKey[128] ^ BytesToWord(inputBuffer, inputOffset + 12);

			Ib7(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[124]; this._x1 ^= this._workingKey[125]; this._x2 ^= this._workingKey[126]; this._x3 ^= this._workingKey[127];
			InverseLT(); Ib6(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[120]; this._x1 ^= this._workingKey[121]; this._x2 ^= this._workingKey[122]; this._x3 ^= this._workingKey[123];
			InverseLT(); Ib5(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[116]; this._x1 ^= this._workingKey[117]; this._x2 ^= this._workingKey[118]; this._x3 ^= this._workingKey[119];
			InverseLT(); Ib4(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[112]; this._x1 ^= this._workingKey[113]; this._x2 ^= this._workingKey[114]; this._x3 ^= this._workingKey[115];
			InverseLT(); Ib3(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[108]; this._x1 ^= this._workingKey[109]; this._x2 ^= this._workingKey[110]; this._x3 ^= this._workingKey[111];
			InverseLT(); Ib2(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[104]; this._x1 ^= this._workingKey[105]; this._x2 ^= this._workingKey[106]; this._x3 ^= this._workingKey[107];
			InverseLT(); Ib1(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[100]; this._x1 ^= this._workingKey[101]; this._x2 ^= this._workingKey[102]; this._x3 ^= this._workingKey[103];
			InverseLT(); Ib0(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[96]; this._x1 ^= this._workingKey[97]; this._x2 ^= this._workingKey[98]; this._x3 ^= this._workingKey[99];
			InverseLT(); Ib7(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[92]; this._x1 ^= this._workingKey[93]; this._x2 ^= this._workingKey[94]; this._x3 ^= this._workingKey[95];
			InverseLT(); Ib6(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[88]; this._x1 ^= this._workingKey[89]; this._x2 ^= this._workingKey[90]; this._x3 ^= this._workingKey[91];
			InverseLT(); Ib5(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[84]; this._x1 ^= this._workingKey[85]; this._x2 ^= this._workingKey[86]; this._x3 ^= this._workingKey[87];
			InverseLT(); Ib4(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[80]; this._x1 ^= this._workingKey[81]; this._x2 ^= this._workingKey[82]; this._x3 ^= this._workingKey[83];
			InverseLT(); Ib3(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[76]; this._x1 ^= this._workingKey[77]; this._x2 ^= this._workingKey[78]; this._x3 ^= this._workingKey[79];
			InverseLT(); Ib2(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[72]; this._x1 ^= this._workingKey[73]; this._x2 ^= this._workingKey[74]; this._x3 ^= this._workingKey[75];
			InverseLT(); Ib1(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[68]; this._x1 ^= this._workingKey[69]; this._x2 ^= this._workingKey[70]; this._x3 ^= this._workingKey[71];
			InverseLT(); Ib0(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[64]; this._x1 ^= this._workingKey[65]; this._x2 ^= this._workingKey[66]; this._x3 ^= this._workingKey[67];
			InverseLT(); Ib7(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[60]; this._x1 ^= this._workingKey[61]; this._x2 ^= this._workingKey[62]; this._x3 ^= this._workingKey[63];
			InverseLT(); Ib6(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[56]; this._x1 ^= this._workingKey[57]; this._x2 ^= this._workingKey[58]; this._x3 ^= this._workingKey[59];
			InverseLT(); Ib5(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[52]; this._x1 ^= this._workingKey[53]; this._x2 ^= this._workingKey[54]; this._x3 ^= this._workingKey[55];
			InverseLT(); Ib4(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[48]; this._x1 ^= this._workingKey[49]; this._x2 ^= this._workingKey[50]; this._x3 ^= this._workingKey[51];
			InverseLT(); Ib3(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[44]; this._x1 ^= this._workingKey[45]; this._x2 ^= this._workingKey[46]; this._x3 ^= this._workingKey[47];
			InverseLT(); Ib2(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[40]; this._x1 ^= this._workingKey[41]; this._x2 ^= this._workingKey[42]; this._x3 ^= this._workingKey[43];
			InverseLT(); Ib1(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[36]; this._x1 ^= this._workingKey[37]; this._x2 ^= this._workingKey[38]; this._x3 ^= this._workingKey[39];
			InverseLT(); Ib0(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[32]; this._x1 ^= this._workingKey[33]; this._x2 ^= this._workingKey[34]; this._x3 ^= this._workingKey[35];
			InverseLT(); Ib7(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[28]; this._x1 ^= this._workingKey[29]; this._x2 ^= this._workingKey[30]; this._x3 ^= this._workingKey[31];
			InverseLT(); Ib6(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[24]; this._x1 ^= this._workingKey[25]; this._x2 ^= this._workingKey[26]; this._x3 ^= this._workingKey[27];
			InverseLT(); Ib5(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[20]; this._x1 ^= this._workingKey[21]; this._x2 ^= this._workingKey[22]; this._x3 ^= this._workingKey[23];
			InverseLT(); Ib4(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[16]; this._x1 ^= this._workingKey[17]; this._x2 ^= this._workingKey[18]; this._x3 ^= this._workingKey[19];
			InverseLT(); Ib3(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[12]; this._x1 ^= this._workingKey[13]; this._x2 ^= this._workingKey[14]; this._x3 ^= this._workingKey[15];
			InverseLT(); Ib2(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[8]; this._x1 ^= this._workingKey[9]; this._x2 ^= this._workingKey[10]; this._x3 ^= this._workingKey[11];
			InverseLT(); Ib1(this._x0, this._x1, this._x2, this._x3);
			this._x0 ^= this._workingKey[4]; this._x1 ^= this._workingKey[5]; this._x2 ^= this._workingKey[6]; this._x3 ^= this._workingKey[7];
			InverseLT(); Ib0(this._x0, this._x1, this._x2, this._x3);

			WordToBytes(this._x3 ^ this._workingKey[3], outputBuffer, outputOffset);
			WordToBytes(this._x2 ^ this._workingKey[2], outputBuffer, outputOffset + 4);
			WordToBytes(this._x1 ^ this._workingKey[1], outputBuffer, outputOffset + 8);
			WordToBytes(this._x0 ^ this._workingKey[0], outputBuffer, outputOffset + 12);

			return this.BlockSize;
		}

		/**
		* Expand a user-supplied key material into a session key.
		*
		* @param key  The user-key bytes (multiples of 4) to use.
		* @exception ArgumentException
		*/
		private int[] MakeWorkingKey(byte[] key)
		{
			//
			// pad key to 256 bits
			//
			int[] kPad = new int[16];
			int off;
			int length = 0;

			for (off = key.Length - 4; off > 0; off -= 4)
			{
				kPad[length++] = BytesToWord(key, off);
			}

			if (off == 0)
			{
				kPad[length++] = BytesToWord(key, 0);
				if (length < 8)
				{
					kPad[length] = 1;
				}
			}
			else
			{
				throw new ArgumentException("key must be a multiple of 4 bytes");
			}

			//
			// expand the padded key up to 33 x 128 bits of key material
			//
			int amount = (ROUNDS + 1) * 4;
			int[] w = new int[amount];

			//
			// compute w0 to w7 from w-8 to w-1
			//
			for (int i = 8; i < 16; i++)
			{
				kPad[i] = RotateLeft(kPad[i - 8] ^ kPad[i - 5] ^ kPad[i - 3] ^ kPad[i - 1] ^ PHI ^ (i - 8), 11);
			}

            Buffer.BlockCopy(kPad, 8, w, 0, 8);

			//
			// compute w8 to w136
			//
			for (int i = 8; i < amount; i++)
			{
				w[i] = RotateLeft(w[i - 8] ^ w[i - 5] ^ w[i - 3] ^ w[i - 1] ^ PHI ^ i, 11);
			}

			//
			// create the working keys by processing w with the Sbox and IP
			//
			Sb3(w[0], w[1], w[2], w[3]);
			w[0] = this._x0; w[1] = this._x1; w[2] = this._x2; w[3] = this._x3;
			Sb2(w[4], w[5], w[6], w[7]);
			w[4] = this._x0; w[5] = this._x1; w[6] = this._x2; w[7] = this._x3;
			Sb1(w[8], w[9], w[10], w[11]);
			w[8] = this._x0; w[9] = this._x1; w[10] = this._x2; w[11] = this._x3;
			Sb0(w[12], w[13], w[14], w[15]);
			w[12] = this._x0; w[13] = this._x1; w[14] = this._x2; w[15] = this._x3;
			Sb7(w[16], w[17], w[18], w[19]);
			w[16] = this._x0; w[17] = this._x1; w[18] = this._x2; w[19] = this._x3;
			Sb6(w[20], w[21], w[22], w[23]);
			w[20] = this._x0; w[21] = this._x1; w[22] = this._x2; w[23] = this._x3;
			Sb5(w[24], w[25], w[26], w[27]);
			w[24] = this._x0; w[25] = this._x1; w[26] = this._x2; w[27] = this._x3;
			Sb4(w[28], w[29], w[30], w[31]);
			w[28] = this._x0; w[29] = this._x1; w[30] = this._x2; w[31] = this._x3;
			Sb3(w[32], w[33], w[34], w[35]);
			w[32] = this._x0; w[33] = this._x1; w[34] = this._x2; w[35] = this._x3;
			Sb2(w[36], w[37], w[38], w[39]);
			w[36] = this._x0; w[37] = this._x1; w[38] = this._x2; w[39] = this._x3;
			Sb1(w[40], w[41], w[42], w[43]);
			w[40] = this._x0; w[41] = this._x1; w[42] = this._x2; w[43] = this._x3;
			Sb0(w[44], w[45], w[46], w[47]);
			w[44] = this._x0; w[45] = this._x1; w[46] = this._x2; w[47] = this._x3;
			Sb7(w[48], w[49], w[50], w[51]);
			w[48] = this._x0; w[49] = this._x1; w[50] = this._x2; w[51] = this._x3;
			Sb6(w[52], w[53], w[54], w[55]);
			w[52] = this._x0; w[53] = this._x1; w[54] = this._x2; w[55] = this._x3;
			Sb5(w[56], w[57], w[58], w[59]);
			w[56] = this._x0; w[57] = this._x1; w[58] = this._x2; w[59] = this._x3;
			Sb4(w[60], w[61], w[62], w[63]);
			w[60] = this._x0; w[61] = this._x1; w[62] = this._x2; w[63] = this._x3;
			Sb3(w[64], w[65], w[66], w[67]);
			w[64] = this._x0; w[65] = this._x1; w[66] = this._x2; w[67] = this._x3;
			Sb2(w[68], w[69], w[70], w[71]);
			w[68] = this._x0; w[69] = this._x1; w[70] = this._x2; w[71] = this._x3;
			Sb1(w[72], w[73], w[74], w[75]);
			w[72] = this._x0; w[73] = this._x1; w[74] = this._x2; w[75] = this._x3;
			Sb0(w[76], w[77], w[78], w[79]);
			w[76] = this._x0; w[77] = this._x1; w[78] = this._x2; w[79] = this._x3;
			Sb7(w[80], w[81], w[82], w[83]);
			w[80] = this._x0; w[81] = this._x1; w[82] = this._x2; w[83] = this._x3;
			Sb6(w[84], w[85], w[86], w[87]);
			w[84] = this._x0; w[85] = this._x1; w[86] = this._x2; w[87] = this._x3;
			Sb5(w[88], w[89], w[90], w[91]);
			w[88] = this._x0; w[89] = this._x1; w[90] = this._x2; w[91] = this._x3;
			Sb4(w[92], w[93], w[94], w[95]);
			w[92] = this._x0; w[93] = this._x1; w[94] = this._x2; w[95] = this._x3;
			Sb3(w[96], w[97], w[98], w[99]);
			w[96] = this._x0; w[97] = this._x1; w[98] = this._x2; w[99] = this._x3;
			Sb2(w[100], w[101], w[102], w[103]);
			w[100] = this._x0; w[101] = this._x1; w[102] = this._x2; w[103] = this._x3;
			Sb1(w[104], w[105], w[106], w[107]);
			w[104] = this._x0; w[105] = this._x1; w[106] = this._x2; w[107] = this._x3;
			Sb0(w[108], w[109], w[110], w[111]);
			w[108] = this._x0; w[109] = this._x1; w[110] = this._x2; w[111] = this._x3;
			Sb7(w[112], w[113], w[114], w[115]);
			w[112] = this._x0; w[113] = this._x1; w[114] = this._x2; w[115] = this._x3;
			Sb6(w[116], w[117], w[118], w[119]);
			w[116] = this._x0; w[117] = this._x1; w[118] = this._x2; w[119] = this._x3;
			Sb5(w[120], w[121], w[122], w[123]);
			w[120] = this._x0; w[121] = this._x1; w[122] = this._x2; w[123] = this._x3;
			Sb4(w[124], w[125], w[126], w[127]);
			w[124] = this._x0; w[125] = this._x1; w[126] = this._x2; w[127] = this._x3;
			Sb3(w[128], w[129], w[130], w[131]);
			w[128] = this._x0; w[129] = this._x1; w[130] = this._x2; w[131] = this._x3;

			return w;
		}

		private static int RotateLeft(int x, int bits)
		{
			return ((x << bits) | (int)((uint)x >> (32 - bits)));
		}

        private static int RotateRight(int x, int bits)
		{
			return ((int)((uint)x >> bits) | (x << (32 - bits)));
		}

        private static int BytesToWord(byte[] src, int srcOff)
		{
			return (((src[srcOff] & 0xff) << 24) | ((src[srcOff + 1] & 0xff) << 16) |
			((src[srcOff + 2] & 0xff) << 8) | ((src[srcOff + 3] & 0xff)));
		}

        private static void WordToBytes(int word, byte[] dst, int dstOff)
		{
			dst[dstOff + 3] = (byte)(word);
			dst[dstOff + 2] = (byte)((uint)word >> 8);
			dst[dstOff + 1] = (byte)((uint)word >> 16);
			dst[dstOff] = (byte)((uint)word >> 24);
		}

		/*
		* The sboxes below are based on the work of Brian Gladman and
		* Sam Simpson, whose original notice appears below.
		* <p>
		* For further details see:
		*      http://fp.gladman.plus.com/cryptography_technology/serpent/
		* </p>
		*/

		/* Partially optimised Serpent S Box bool functions derived  */
		/* using a recursive descent analyser but without a full search */
		/* of all subtrees. This set of S boxes is the result of work    */
		/* by Sam Simpson and Brian Gladman using the spare time on a    */
		/* cluster of high capacity servers to search for S boxes with    */
		/* this customised search engine. There are now an average of    */
		/* 15.375 terms    per S box.                                        */
		/*                                                              */
		/* Copyright:   Dr B. R Gladman (gladman@seven77.demon.co.uk)   */
		/*                and Sam Simpson (s.simpson@mia.co.uk)            */
		/*              17th December 1998                                */
		/*                                                              */
		/* We hereby give permission for information in this file to be */
		/* used freely subject only to acknowledgement of its origin.    */

        /// <summary>
        /// S0 - { 3, 8,15, 1,10, 6, 5,11,14,13, 4, 2, 7, 0, 9,12 } - 15 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb0(int a, int b, int c, int d)
		{
			int t1 = a ^ d;
			int t3 = c ^ t1;
			int t4 = b ^ t3;
			this._x3 = (a & d) ^ t4;
			int t7 = a ^ (b & t1);
			this._x2 = t4 ^ (c | t7);
			int t12 = this._x3 & (t3 ^ t7);
			this._x1 = (~t3) ^ t12;
			this._x0 = t12 ^ (~t7);
		}

        /// <summary>
        /// InvSO - {13, 3,11, 0,10, 6, 5,12, 1,14, 4, 7,15, 9, 8, 2 } - 15 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib0(int a, int b, int c, int d)
		{
			int t1 = ~a;
			int t2 = a ^ b;
			int t4 = d ^ (t1 | t2);
			int t5 = c ^ t4;
			this._x2 = t2 ^ t5;
			int t8 = t1 ^ (d & t2);
			this._x1 = t4 ^ (this._x2 & t8);
			this._x3 = (a & t4) ^ (t5 | this._x1);
			this._x0 = this._x3 ^ (t5 ^ t8);
		}

        /// <summary>
        /// S1 - {15,12, 2, 7, 9, 0, 5,10, 1,11,14, 8, 6,13, 3, 4 } - 14 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb1(int a, int b, int c, int d)
		{
			int t2 = b ^ (~a);
			int t5 = c ^ (a | t2);
			this._x2 = d ^ t5;
			int t7 = b ^ (d | t2);
			int t8 = t2 ^ this._x2;
			this._x3 = t8 ^ (t5 & t7);
			int t11 = t5 ^ t7;
			this._x1 = this._x3 ^ t11;
			this._x0 = t5 ^ (t8 & t11);
		}

        /// <summary>
        /// InvS1 - { 5, 8, 2,14,15, 6,12, 3,11, 4, 7, 9, 1,13,10, 0 } - 14 steps.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib1(int a, int b, int c, int d)
		{
			int t1 = b ^ d;
			int t3 = a ^ (b & t1);
			int t4 = t1 ^ t3;
			this._x3 = c ^ t4;
			int t7 = b ^ (t1 & t3);
			int t8 = this._x3 | t7;
			this._x1 = t3 ^ t8;
			int t10 = ~this._x1;
			int t11 = this._x3 ^ t7;
			this._x0 = t10 ^ t11;
			this._x2 = t4 ^ (t10 | t11);
		}

        /// <summary>
        /// S2 - { 8, 6, 7, 9, 3,12,10,15,13, 1,14, 4, 0,11, 5, 2 } - 16 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb2(int a, int b, int c, int d)
		{
			int t1 = ~a;
			int t2 = b ^ d;
			int t3 = c & t1;
			this._x0 = t2 ^ t3;
			int t5 = c ^ t1;
			int t6 = c ^ this._x0;
			int t7 = b & t6;
			this._x3 = t5 ^ t7;
			this._x2 = a ^ ((d | t7) & (this._x0 | t5));
			this._x1 = (t2 ^ this._x3) ^ (this._x2 ^ (d | t1));
		}

        /// <summary>
        /// InvS2 - {12, 9,15, 4,11,14, 1, 2, 0, 3, 6,13, 5, 8,10, 7 } - 16 steps.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib2(int a, int b, int c, int d)
		{
			int t1 = b ^ d;
			int t2 = ~t1;
			int t3 = a ^ c;
			int t4 = c ^ t1;
			int t5 = b & t4;
			this._x0 = t3 ^ t5;
			int t7 = a | t2;
			int t8 = d ^ t7;
			int t9 = t3 | t8;
			this._x3 = t1 ^ t9;
			int t11 = ~t4;
			int t12 = this._x0 | this._x3;
			this._x1 = t11 ^ t12;
			this._x2 = (d & t11) ^ (t3 ^ t12);
		}

        /// <summary>
        /// S3 - { 0,15,11, 8,12, 9, 6, 3,13, 1, 2, 4,10, 7, 5,14 } - 16 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb3(int a, int b, int c, int d)
		{
			int t1 = a ^ b;
			int t2 = a & c;
			int t3 = a | d;
			int t4 = c ^ d;
			int t5 = t1 & t3;
			int t6 = t2 | t5;
			this._x2 = t4 ^ t6;
			int t8 = b ^ t3;
			int t9 = t6 ^ t8;
			int t10 = t4 & t9;
			this._x0 = t1 ^ t10;
			int t12 = this._x2 & this._x0;
			this._x1 = t9 ^ t12;
			this._x3 = (b | d) ^ (t4 ^ t12);
		}

        /// <summary>
        /// InvS3 - { 0, 9,10, 7,11,14, 6,13, 3, 5,12, 2, 4, 8,15, 1 } - 15 terms
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib3(int a, int b, int c, int d)
		{
			int t1 = a | b;
			int t2 = b ^ c;
			int t3 = b & t2;
			int t4 = a ^ t3;
			int t5 = c ^ t4;
			int t6 = d | t4;
			this._x0 = t2 ^ t6;
			int t8 = t2 | t6;
			int t9 = d ^ t8;
			this._x2 = t5 ^ t9;
			int t11 = t1 ^ t9;
			int t12 = this._x0 & t11;
			this._x3 = t4 ^ t12;
			this._x1 = this._x3 ^ (this._x0 ^ t11);
		}

        /// <summary>
        /// S4 - { 1,15, 8, 3,12, 0,11, 6, 2, 5, 4,10, 9,14, 7,13 } - 15 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb4(int a, int b, int c, int d)
		{
			int t1 = a ^ d;
			int t2 = d & t1;
			int t3 = c ^ t2;
			int t4 = b | t3;
			this._x3 = t1 ^ t4;
			int t6 = ~b;
			int t7 = t1 | t6;
			this._x0 = t3 ^ t7;
			int t9 = a & this._x0;
			int t10 = t1 ^ t6;
			int t11 = t4 & t10;
			this._x2 = t9 ^ t11;
			this._x1 = (a ^ t3) ^ (t10 & this._x2);
		}

        /// <summary>
        /// InvS4 - { 5, 0, 8, 3,10, 9, 7,14, 2,12,11, 6, 4,15,13, 1 } - 15 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib4(int a, int b, int c, int d)
		{
			int t1 = c | d;
			int t2 = a & t1;
			int t3 = b ^ t2;
			int t4 = a & t3;
			int t5 = c ^ t4;
			this._x1 = d ^ t5;
			int t7 = ~a;
			int t8 = t5 & this._x1;
			this._x3 = t3 ^ t8;
			int t10 = this._x1 | t7;
			int t11 = d ^ t10;
			this._x0 = this._x3 ^ t11;
			this._x2 = (t3 & t11) ^ (this._x1 ^ t7);
		}

        /// <summary>
        /// S5 - {15, 5, 2,11, 4,10, 9,12, 0, 3,14, 8,13, 6, 7, 1 } - 16 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb5(int a, int b, int c, int d)
		{
			int t1 = ~a;
			int t2 = a ^ b;
			int t3 = a ^ d;
			int t4 = c ^ t1;
			int t5 = t2 | t3;
			this._x0 = t4 ^ t5;
			int t7 = d & this._x0;
			int t8 = t2 ^ this._x0;
			this._x1 = t7 ^ t8;
			int t10 = t1 | this._x0;
			int t11 = t2 | t7;
			int t12 = t3 ^ t10;
			this._x2 = t11 ^ t12;
			this._x3 = (b ^ t7) ^ (this._x1 & t12);
		}

        /// <summary>
        /// InvS5 - { 8,15, 2, 9, 4, 1,13,14,11, 6, 5, 3, 7,12,10, 0 } - 16 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib5(int a, int b, int c, int d)
		{
			int t1 = ~c;
			int t2 = b & t1;
			int t3 = d ^ t2;
			int t4 = a & t3;
			int t5 = b ^ t1;
			this._x3 = t4 ^ t5;
			int t7 = b | this._x3;
			int t8 = a & t7;
			this._x1 = t3 ^ t8;
			int t10 = a | d;
			int t11 = t1 ^ t7;
			this._x0 = t10 ^ t11;
			this._x2 = (b & t10) ^ (t4 | (a ^ c));
		}

        /// <summary>
        /// S6 - { 7, 2,12, 5, 8, 4, 6,11,14, 9, 1,15,13, 3,10, 0 } - 15 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb6(int a, int b, int c, int d)
		{
			int t1 = ~a;
			int t2 = a ^ d;
			int t3 = b ^ t2;
			int t4 = t1 | t2;
			int t5 = c ^ t4;
			this._x1 = b ^ t5;
			int t7 = t2 | this._x1;
			int t8 = d ^ t7;
			int t9 = t5 & t8;
			this._x2 = t3 ^ t9;
			int t11 = t5 ^ t8;
			this._x0 = this._x2 ^ t11;
			this._x3 = (~t5) ^ (t3 & t11);
		}

        /// <summary>
        /// InvS6 - {15,10, 1,13, 5, 3, 6, 0, 4, 9,14, 7, 2,12, 8,11 } - 15 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib6(int a, int b, int c, int d)
		{
			int t1 = ~a;
			int t2 = a ^ b;
			int t3 = c ^ t2;
			int t4 = c | t1;
			int t5 = d ^ t4;
			this._x1 = t3 ^ t5;
			int t7 = t3 & t5;
			int t8 = t2 ^ t7;
			int t9 = b | t8;
			this._x3 = t5 ^ t9;
			int t11 = b | this._x3;
			this._x0 = t8 ^ t11;
			this._x2 = (d & t1) ^ (t3 ^ t11);
		}

        /// <summary>
        /// S7 - { 1,13,15, 0,14, 8, 2,11, 7, 4,12,10, 9, 3, 5, 6 } - 16 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Sb7(int a, int b, int c, int d)
		{
			int t1 = b ^ c;
			int t2 = c & t1;
			int t3 = d ^ t2;
			int t4 = a ^ t3;
			int t5 = d | t1;
			int t6 = t4 & t5;
			this._x1 = b ^ t6;
			int t8 = t3 | this._x1;
			int t9 = a & t4;
			this._x3 = t1 ^ t9;
			int t11 = t4 ^ t8;
			int t12 = this._x3 & t11;
			this._x2 = t3 ^ t12;
			this._x0 = (~t11) ^ (this._x3 & this._x2);
		}

        /// <summary>
        /// InvS7 - { 3, 0, 6,13, 9,14,15, 8, 5,12,11, 7,10, 1, 4, 2 } - 17 terms.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        private void Ib7(int a, int b, int c, int d)
		{
			int t3 = c | (a & b);
			int t4 = d & (a | b);
			this._x3 = t3 ^ t4;
			int t6 = ~d;
			int t7 = b ^ t4;
			int t9 = t7 | (this._x3 ^ t6);
			this._x1 = a ^ t9;
			this._x0 = (c ^ t7) ^ (d | this._x1);
			this._x2 = (t3 ^ this._x1) ^ (this._x0 ^ (a & this._x3));
		}

        /// <summary>
        /// Apply the linear transformation to the register set.
        /// </summary>
		private void LT()
		{
			int x0 = RotateLeft(this._x0, 13);
			int x2 = RotateLeft(this._x2, 3);
			int x1 = this._x1 ^ x0 ^ x2;
			int x3 = this._x3 ^ x2 ^ x0 << 3;

			this._x1 = RotateLeft(x1, 1);
			this._x3 = RotateLeft(x3, 7);
			this._x0 = RotateLeft(x0 ^ this._x1 ^ this._x3, 5);
			this._x2 = RotateLeft(x2 ^ this._x3 ^ (this._x1 << 7), 22);
		}

        /// <summary>
        /// Apply the inverse of the linear transformation to the register set.
        /// </summary>
		private void InverseLT()
		{
			int x2 = RotateRight(this._x2, 22) ^ this._x3 ^ (this._x1 << 7);
			int x0 = RotateRight(this._x0, 5) ^ this._x1 ^ this._x3;
			int x3 = RotateRight(this._x3, 7);
			int x1 = RotateRight(this._x1, 1);
			this._x3 = x3 ^ x2 ^ x0 << 3;
			this._x1 = x1 ^ x0 ^ x2;
			this._x2 = RotateRight(x2, 3);
			this._x0 = RotateRight(x0, 13);
		}
	}
}
