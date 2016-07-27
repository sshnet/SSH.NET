using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements Serpent cipher algorithm.
    /// </summary>
    public sealed class SerpentCipher : BlockCipher
    {
        private const int Rounds = 32;
        private const int Phi = unchecked((int)0x9E3779B9); // (Sqrt(5) - 1) * 2**31

        private readonly int[] _workingKey;
        private int _x0, _x1, _x2, _x3;    // registers

        /// <summary>
        /// Initializes a new instance of the <see cref="SerpentCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="padding">The padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public SerpentCipher(byte[] key, CipherMode mode, CipherPadding padding)
            : base(key, 16, mode, padding)
        {
            var keySize = key.Length * 8;

            if (!(keySize == 128 || keySize == 192 || keySize == 256))
                throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", keySize));

            _workingKey = MakeWorkingKey(key);
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
            if (inputCount != BlockSize)
                throw new ArgumentException("inputCount");

            _x3 = BytesToWord(inputBuffer, inputOffset);
            _x2 = BytesToWord(inputBuffer, inputOffset + 4);
            _x1 = BytesToWord(inputBuffer, inputOffset + 8);
            _x0 = BytesToWord(inputBuffer, inputOffset + 12);

            Sb0(_workingKey[0] ^ _x0, _workingKey[1] ^ _x1, _workingKey[2] ^ _x2, _workingKey[3] ^ _x3); LT();
            Sb1(_workingKey[4] ^ _x0, _workingKey[5] ^ _x1, _workingKey[6] ^ _x2, _workingKey[7] ^ _x3); LT();
            Sb2(_workingKey[8] ^ _x0, _workingKey[9] ^ _x1, _workingKey[10] ^ _x2, _workingKey[11] ^ _x3); LT();
            Sb3(_workingKey[12] ^ _x0, _workingKey[13] ^ _x1, _workingKey[14] ^ _x2, _workingKey[15] ^ _x3); LT();
            Sb4(_workingKey[16] ^ _x0, _workingKey[17] ^ _x1, _workingKey[18] ^ _x2, _workingKey[19] ^ _x3); LT();
            Sb5(_workingKey[20] ^ _x0, _workingKey[21] ^ _x1, _workingKey[22] ^ _x2, _workingKey[23] ^ _x3); LT();
            Sb6(_workingKey[24] ^ _x0, _workingKey[25] ^ _x1, _workingKey[26] ^ _x2, _workingKey[27] ^ _x3); LT();
            Sb7(_workingKey[28] ^ _x0, _workingKey[29] ^ _x1, _workingKey[30] ^ _x2, _workingKey[31] ^ _x3); LT();
            Sb0(_workingKey[32] ^ _x0, _workingKey[33] ^ _x1, _workingKey[34] ^ _x2, _workingKey[35] ^ _x3); LT();
            Sb1(_workingKey[36] ^ _x0, _workingKey[37] ^ _x1, _workingKey[38] ^ _x2, _workingKey[39] ^ _x3); LT();
            Sb2(_workingKey[40] ^ _x0, _workingKey[41] ^ _x1, _workingKey[42] ^ _x2, _workingKey[43] ^ _x3); LT();
            Sb3(_workingKey[44] ^ _x0, _workingKey[45] ^ _x1, _workingKey[46] ^ _x2, _workingKey[47] ^ _x3); LT();
            Sb4(_workingKey[48] ^ _x0, _workingKey[49] ^ _x1, _workingKey[50] ^ _x2, _workingKey[51] ^ _x3); LT();
            Sb5(_workingKey[52] ^ _x0, _workingKey[53] ^ _x1, _workingKey[54] ^ _x2, _workingKey[55] ^ _x3); LT();
            Sb6(_workingKey[56] ^ _x0, _workingKey[57] ^ _x1, _workingKey[58] ^ _x2, _workingKey[59] ^ _x3); LT();
            Sb7(_workingKey[60] ^ _x0, _workingKey[61] ^ _x1, _workingKey[62] ^ _x2, _workingKey[63] ^ _x3); LT();
            Sb0(_workingKey[64] ^ _x0, _workingKey[65] ^ _x1, _workingKey[66] ^ _x2, _workingKey[67] ^ _x3); LT();
            Sb1(_workingKey[68] ^ _x0, _workingKey[69] ^ _x1, _workingKey[70] ^ _x2, _workingKey[71] ^ _x3); LT();
            Sb2(_workingKey[72] ^ _x0, _workingKey[73] ^ _x1, _workingKey[74] ^ _x2, _workingKey[75] ^ _x3); LT();
            Sb3(_workingKey[76] ^ _x0, _workingKey[77] ^ _x1, _workingKey[78] ^ _x2, _workingKey[79] ^ _x3); LT();
            Sb4(_workingKey[80] ^ _x0, _workingKey[81] ^ _x1, _workingKey[82] ^ _x2, _workingKey[83] ^ _x3); LT();
            Sb5(_workingKey[84] ^ _x0, _workingKey[85] ^ _x1, _workingKey[86] ^ _x2, _workingKey[87] ^ _x3); LT();
            Sb6(_workingKey[88] ^ _x0, _workingKey[89] ^ _x1, _workingKey[90] ^ _x2, _workingKey[91] ^ _x3); LT();
            Sb7(_workingKey[92] ^ _x0, _workingKey[93] ^ _x1, _workingKey[94] ^ _x2, _workingKey[95] ^ _x3); LT();
            Sb0(_workingKey[96] ^ _x0, _workingKey[97] ^ _x1, _workingKey[98] ^ _x2, _workingKey[99] ^ _x3); LT();
            Sb1(_workingKey[100] ^ _x0, _workingKey[101] ^ _x1, _workingKey[102] ^ _x2, _workingKey[103] ^ _x3); LT();
            Sb2(_workingKey[104] ^ _x0, _workingKey[105] ^ _x1, _workingKey[106] ^ _x2, _workingKey[107] ^ _x3); LT();
            Sb3(_workingKey[108] ^ _x0, _workingKey[109] ^ _x1, _workingKey[110] ^ _x2, _workingKey[111] ^ _x3); LT();
            Sb4(_workingKey[112] ^ _x0, _workingKey[113] ^ _x1, _workingKey[114] ^ _x2, _workingKey[115] ^ _x3); LT();
            Sb5(_workingKey[116] ^ _x0, _workingKey[117] ^ _x1, _workingKey[118] ^ _x2, _workingKey[119] ^ _x3); LT();
            Sb6(_workingKey[120] ^ _x0, _workingKey[121] ^ _x1, _workingKey[122] ^ _x2, _workingKey[123] ^ _x3); LT();
            Sb7(_workingKey[124] ^ _x0, _workingKey[125] ^ _x1, _workingKey[126] ^ _x2, _workingKey[127] ^ _x3);

            WordToBytes(_workingKey[131] ^ _x3, outputBuffer, outputOffset);
            WordToBytes(_workingKey[130] ^ _x2, outputBuffer, outputOffset + 4);
            WordToBytes(_workingKey[129] ^ _x1, outputBuffer, outputOffset + 8);
            WordToBytes(_workingKey[128] ^ _x0, outputBuffer, outputOffset + 12);

            return BlockSize;
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
            if (inputCount != BlockSize)
                throw new ArgumentException("inputCount");

            _x3 = _workingKey[131] ^ BytesToWord(inputBuffer, inputOffset);
            _x2 = _workingKey[130] ^ BytesToWord(inputBuffer, inputOffset + 4);
            _x1 = _workingKey[129] ^ BytesToWord(inputBuffer, inputOffset + 8);
            _x0 = _workingKey[128] ^ BytesToWord(inputBuffer, inputOffset + 12);

            Ib7(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[124]; _x1 ^= _workingKey[125]; _x2 ^= _workingKey[126]; _x3 ^= _workingKey[127];
            InverseLT(); Ib6(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[120]; _x1 ^= _workingKey[121]; _x2 ^= _workingKey[122]; _x3 ^= _workingKey[123];
            InverseLT(); Ib5(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[116]; _x1 ^= _workingKey[117]; _x2 ^= _workingKey[118]; _x3 ^= _workingKey[119];
            InverseLT(); Ib4(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[112]; _x1 ^= _workingKey[113]; _x2 ^= _workingKey[114]; _x3 ^= _workingKey[115];
            InverseLT(); Ib3(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[108]; _x1 ^= _workingKey[109]; _x2 ^= _workingKey[110]; _x3 ^= _workingKey[111];
            InverseLT(); Ib2(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[104]; _x1 ^= _workingKey[105]; _x2 ^= _workingKey[106]; _x3 ^= _workingKey[107];
            InverseLT(); Ib1(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[100]; _x1 ^= _workingKey[101]; _x2 ^= _workingKey[102]; _x3 ^= _workingKey[103];
            InverseLT(); Ib0(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[96]; _x1 ^= _workingKey[97]; _x2 ^= _workingKey[98]; _x3 ^= _workingKey[99];
            InverseLT(); Ib7(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[92]; _x1 ^= _workingKey[93]; _x2 ^= _workingKey[94]; _x3 ^= _workingKey[95];
            InverseLT(); Ib6(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[88]; _x1 ^= _workingKey[89]; _x2 ^= _workingKey[90]; _x3 ^= _workingKey[91];
            InverseLT(); Ib5(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[84]; _x1 ^= _workingKey[85]; _x2 ^= _workingKey[86]; _x3 ^= _workingKey[87];
            InverseLT(); Ib4(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[80]; _x1 ^= _workingKey[81]; _x2 ^= _workingKey[82]; _x3 ^= _workingKey[83];
            InverseLT(); Ib3(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[76]; _x1 ^= _workingKey[77]; _x2 ^= _workingKey[78]; _x3 ^= _workingKey[79];
            InverseLT(); Ib2(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[72]; _x1 ^= _workingKey[73]; _x2 ^= _workingKey[74]; _x3 ^= _workingKey[75];
            InverseLT(); Ib1(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[68]; _x1 ^= _workingKey[69]; _x2 ^= _workingKey[70]; _x3 ^= _workingKey[71];
            InverseLT(); Ib0(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[64]; _x1 ^= _workingKey[65]; _x2 ^= _workingKey[66]; _x3 ^= _workingKey[67];
            InverseLT(); Ib7(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[60]; _x1 ^= _workingKey[61]; _x2 ^= _workingKey[62]; _x3 ^= _workingKey[63];
            InverseLT(); Ib6(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[56]; _x1 ^= _workingKey[57]; _x2 ^= _workingKey[58]; _x3 ^= _workingKey[59];
            InverseLT(); Ib5(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[52]; _x1 ^= _workingKey[53]; _x2 ^= _workingKey[54]; _x3 ^= _workingKey[55];
            InverseLT(); Ib4(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[48]; _x1 ^= _workingKey[49]; _x2 ^= _workingKey[50]; _x3 ^= _workingKey[51];
            InverseLT(); Ib3(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[44]; _x1 ^= _workingKey[45]; _x2 ^= _workingKey[46]; _x3 ^= _workingKey[47];
            InverseLT(); Ib2(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[40]; _x1 ^= _workingKey[41]; _x2 ^= _workingKey[42]; _x3 ^= _workingKey[43];
            InverseLT(); Ib1(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[36]; _x1 ^= _workingKey[37]; _x2 ^= _workingKey[38]; _x3 ^= _workingKey[39];
            InverseLT(); Ib0(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[32]; _x1 ^= _workingKey[33]; _x2 ^= _workingKey[34]; _x3 ^= _workingKey[35];
            InverseLT(); Ib7(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[28]; _x1 ^= _workingKey[29]; _x2 ^= _workingKey[30]; _x3 ^= _workingKey[31];
            InverseLT(); Ib6(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[24]; _x1 ^= _workingKey[25]; _x2 ^= _workingKey[26]; _x3 ^= _workingKey[27];
            InverseLT(); Ib5(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[20]; _x1 ^= _workingKey[21]; _x2 ^= _workingKey[22]; _x3 ^= _workingKey[23];
            InverseLT(); Ib4(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[16]; _x1 ^= _workingKey[17]; _x2 ^= _workingKey[18]; _x3 ^= _workingKey[19];
            InverseLT(); Ib3(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[12]; _x1 ^= _workingKey[13]; _x2 ^= _workingKey[14]; _x3 ^= _workingKey[15];
            InverseLT(); Ib2(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[8]; _x1 ^= _workingKey[9]; _x2 ^= _workingKey[10]; _x3 ^= _workingKey[11];
            InverseLT(); Ib1(_x0, _x1, _x2, _x3);
            _x0 ^= _workingKey[4]; _x1 ^= _workingKey[5]; _x2 ^= _workingKey[6]; _x3 ^= _workingKey[7];
            InverseLT(); Ib0(_x0, _x1, _x2, _x3);

            WordToBytes(_x3 ^ _workingKey[3], outputBuffer, outputOffset);
            WordToBytes(_x2 ^ _workingKey[2], outputBuffer, outputOffset + 4);
            WordToBytes(_x1 ^ _workingKey[1], outputBuffer, outputOffset + 8);
            WordToBytes(_x0 ^ _workingKey[0], outputBuffer, outputOffset + 12);

            return BlockSize;
        }


        /// <summary>
        /// Expand a user-supplied key material into a session key.
        /// </summary>
        /// <param name="key">The user-key bytes to use.</param>
        /// <returns>
        /// A session key.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="key"/> is not multiple of 4 bytes.</exception>
        private int[] MakeWorkingKey(byte[] key)
        {
            //
            // pad key to 256 bits
            //
            var kPad = new int[16];
            int off;
            var length = 0;

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
            const int amount = (Rounds + 1) * 4;
            var w = new int[amount];

            //
            // compute w0 to w7 from w-8 to w-1
            //
            for (var i = 8; i < 16; i++)
            {
                kPad[i] = RotateLeft(kPad[i - 8] ^ kPad[i - 5] ^ kPad[i - 3] ^ kPad[i - 1] ^ Phi ^ (i - 8), 11);
            }

            Buffer.BlockCopy(kPad, 8, w, 0, 8);

            //
            // compute w8 to w136
            //
            for (var i = 8; i < amount; i++)
            {
                w[i] = RotateLeft(w[i - 8] ^ w[i - 5] ^ w[i - 3] ^ w[i - 1] ^ Phi ^ i, 11);
            }

            //
            // create the working keys by processing w with the Sbox and IP
            //
            Sb3(w[0], w[1], w[2], w[3]);
            w[0] = _x0; w[1] = _x1; w[2] = _x2; w[3] = _x3;
            Sb2(w[4], w[5], w[6], w[7]);
            w[4] = _x0; w[5] = _x1; w[6] = _x2; w[7] = _x3;
            Sb1(w[8], w[9], w[10], w[11]);
            w[8] = _x0; w[9] = _x1; w[10] = _x2; w[11] = _x3;
            Sb0(w[12], w[13], w[14], w[15]);
            w[12] = _x0; w[13] = _x1; w[14] = _x2; w[15] = _x3;
            Sb7(w[16], w[17], w[18], w[19]);
            w[16] = _x0; w[17] = _x1; w[18] = _x2; w[19] = _x3;
            Sb6(w[20], w[21], w[22], w[23]);
            w[20] = _x0; w[21] = _x1; w[22] = _x2; w[23] = _x3;
            Sb5(w[24], w[25], w[26], w[27]);
            w[24] = _x0; w[25] = _x1; w[26] = _x2; w[27] = _x3;
            Sb4(w[28], w[29], w[30], w[31]);
            w[28] = _x0; w[29] = _x1; w[30] = _x2; w[31] = _x3;
            Sb3(w[32], w[33], w[34], w[35]);
            w[32] = _x0; w[33] = _x1; w[34] = _x2; w[35] = _x3;
            Sb2(w[36], w[37], w[38], w[39]);
            w[36] = _x0; w[37] = _x1; w[38] = _x2; w[39] = _x3;
            Sb1(w[40], w[41], w[42], w[43]);
            w[40] = _x0; w[41] = _x1; w[42] = _x2; w[43] = _x3;
            Sb0(w[44], w[45], w[46], w[47]);
            w[44] = _x0; w[45] = _x1; w[46] = _x2; w[47] = _x3;
            Sb7(w[48], w[49], w[50], w[51]);
            w[48] = _x0; w[49] = _x1; w[50] = _x2; w[51] = _x3;
            Sb6(w[52], w[53], w[54], w[55]);
            w[52] = _x0; w[53] = _x1; w[54] = _x2; w[55] = _x3;
            Sb5(w[56], w[57], w[58], w[59]);
            w[56] = _x0; w[57] = _x1; w[58] = _x2; w[59] = _x3;
            Sb4(w[60], w[61], w[62], w[63]);
            w[60] = _x0; w[61] = _x1; w[62] = _x2; w[63] = _x3;
            Sb3(w[64], w[65], w[66], w[67]);
            w[64] = _x0; w[65] = _x1; w[66] = _x2; w[67] = _x3;
            Sb2(w[68], w[69], w[70], w[71]);
            w[68] = _x0; w[69] = _x1; w[70] = _x2; w[71] = _x3;
            Sb1(w[72], w[73], w[74], w[75]);
            w[72] = _x0; w[73] = _x1; w[74] = _x2; w[75] = _x3;
            Sb0(w[76], w[77], w[78], w[79]);
            w[76] = _x0; w[77] = _x1; w[78] = _x2; w[79] = _x3;
            Sb7(w[80], w[81], w[82], w[83]);
            w[80] = _x0; w[81] = _x1; w[82] = _x2; w[83] = _x3;
            Sb6(w[84], w[85], w[86], w[87]);
            w[84] = _x0; w[85] = _x1; w[86] = _x2; w[87] = _x3;
            Sb5(w[88], w[89], w[90], w[91]);
            w[88] = _x0; w[89] = _x1; w[90] = _x2; w[91] = _x3;
            Sb4(w[92], w[93], w[94], w[95]);
            w[92] = _x0; w[93] = _x1; w[94] = _x2; w[95] = _x3;
            Sb3(w[96], w[97], w[98], w[99]);
            w[96] = _x0; w[97] = _x1; w[98] = _x2; w[99] = _x3;
            Sb2(w[100], w[101], w[102], w[103]);
            w[100] = _x0; w[101] = _x1; w[102] = _x2; w[103] = _x3;
            Sb1(w[104], w[105], w[106], w[107]);
            w[104] = _x0; w[105] = _x1; w[106] = _x2; w[107] = _x3;
            Sb0(w[108], w[109], w[110], w[111]);
            w[108] = _x0; w[109] = _x1; w[110] = _x2; w[111] = _x3;
            Sb7(w[112], w[113], w[114], w[115]);
            w[112] = _x0; w[113] = _x1; w[114] = _x2; w[115] = _x3;
            Sb6(w[116], w[117], w[118], w[119]);
            w[116] = _x0; w[117] = _x1; w[118] = _x2; w[119] = _x3;
            Sb5(w[120], w[121], w[122], w[123]);
            w[120] = _x0; w[121] = _x1; w[122] = _x2; w[123] = _x3;
            Sb4(w[124], w[125], w[126], w[127]);
            w[124] = _x0; w[125] = _x1; w[126] = _x2; w[127] = _x3;
            Sb3(w[128], w[129], w[130], w[131]);
            w[128] = _x0; w[129] = _x1; w[130] = _x2; w[131] = _x3;

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
        /* 15.375 terms    per S box.          */
        /*                    */
        /* Copyright:   Dr B. R Gladman (gladman@seven77.demon.co.uk)   */
        /*    and Sam Simpson (s.simpson@mia.co.uk)      */
        /*        17th December 1998        */
        /*                    */
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
            _x3 = (a & d) ^ t4;
            int t7 = a ^ (b & t1);
            _x2 = t4 ^ (c | t7);
            int t12 = _x3 & (t3 ^ t7);
            _x1 = (~t3) ^ t12;
            _x0 = t12 ^ (~t7);
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
            _x2 = t2 ^ t5;
            int t8 = t1 ^ (d & t2);
            _x1 = t4 ^ (_x2 & t8);
            _x3 = (a & t4) ^ (t5 | _x1);
            _x0 = _x3 ^ (t5 ^ t8);
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
            _x2 = d ^ t5;
            int t7 = b ^ (d | t2);
            int t8 = t2 ^ _x2;
            _x3 = t8 ^ (t5 & t7);
            int t11 = t5 ^ t7;
            _x1 = _x3 ^ t11;
            _x0 = t5 ^ (t8 & t11);
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
            _x3 = c ^ t4;
            int t7 = b ^ (t1 & t3);
            int t8 = _x3 | t7;
            _x1 = t3 ^ t8;
            int t10 = ~_x1;
            int t11 = _x3 ^ t7;
            _x0 = t10 ^ t11;
            _x2 = t4 ^ (t10 | t11);
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
            _x0 = t2 ^ t3;
            int t5 = c ^ t1;
            int t6 = c ^ _x0;
            int t7 = b & t6;
            _x3 = t5 ^ t7;
            _x2 = a ^ ((d | t7) & (_x0 | t5));
            _x1 = (t2 ^ _x3) ^ (_x2 ^ (d | t1));
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
            _x0 = t3 ^ t5;
            int t7 = a | t2;
            int t8 = d ^ t7;
            int t9 = t3 | t8;
            _x3 = t1 ^ t9;
            int t11 = ~t4;
            int t12 = _x0 | _x3;
            _x1 = t11 ^ t12;
            _x2 = (d & t11) ^ (t3 ^ t12);
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
            _x2 = t4 ^ t6;
            int t8 = b ^ t3;
            int t9 = t6 ^ t8;
            int t10 = t4 & t9;
            _x0 = t1 ^ t10;
            int t12 = _x2 & _x0;
            _x1 = t9 ^ t12;
            _x3 = (b | d) ^ (t4 ^ t12);
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
            _x0 = t2 ^ t6;
            int t8 = t2 | t6;
            int t9 = d ^ t8;
            _x2 = t5 ^ t9;
            int t11 = t1 ^ t9;
            int t12 = _x0 & t11;
            _x3 = t4 ^ t12;
            _x1 = _x3 ^ (_x0 ^ t11);
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
            _x3 = t1 ^ t4;
            int t6 = ~b;
            int t7 = t1 | t6;
            _x0 = t3 ^ t7;
            int t9 = a & _x0;
            int t10 = t1 ^ t6;
            int t11 = t4 & t10;
            _x2 = t9 ^ t11;
            _x1 = (a ^ t3) ^ (t10 & _x2);
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
            _x1 = d ^ t5;
            int t7 = ~a;
            int t8 = t5 & _x1;
            _x3 = t3 ^ t8;
            int t10 = _x1 | t7;
            int t11 = d ^ t10;
            _x0 = _x3 ^ t11;
            _x2 = (t3 & t11) ^ (_x1 ^ t7);
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
            _x0 = t4 ^ t5;
            int t7 = d & _x0;
            int t8 = t2 ^ _x0;
            _x1 = t7 ^ t8;
            int t10 = t1 | _x0;
            int t11 = t2 | t7;
            int t12 = t3 ^ t10;
            _x2 = t11 ^ t12;
            _x3 = (b ^ t7) ^ (_x1 & t12);
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
            _x3 = t4 ^ t5;
            int t7 = b | _x3;
            int t8 = a & t7;
            _x1 = t3 ^ t8;
            int t10 = a | d;
            int t11 = t1 ^ t7;
            _x0 = t10 ^ t11;
            _x2 = (b & t10) ^ (t4 | (a ^ c));
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
            _x1 = b ^ t5;
            int t7 = t2 | _x1;
            int t8 = d ^ t7;
            int t9 = t5 & t8;
            _x2 = t3 ^ t9;
            int t11 = t5 ^ t8;
            _x0 = _x2 ^ t11;
            _x3 = (~t5) ^ (t3 & t11);
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
            _x1 = t3 ^ t5;
            int t7 = t3 & t5;
            int t8 = t2 ^ t7;
            int t9 = b | t8;
            _x3 = t5 ^ t9;
            int t11 = b | _x3;
            _x0 = t8 ^ t11;
            _x2 = (d & t1) ^ (t3 ^ t11);
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
            _x1 = b ^ t6;
            int t8 = t3 | _x1;
            int t9 = a & t4;
            _x3 = t1 ^ t9;
            int t11 = t4 ^ t8;
            int t12 = _x3 & t11;
            _x2 = t3 ^ t12;
            _x0 = (~t11) ^ (_x3 & _x2);
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
            _x3 = t3 ^ t4;
            int t6 = ~d;
            int t7 = b ^ t4;
            int t9 = t7 | (_x3 ^ t6);
            _x1 = a ^ t9;
            _x0 = (c ^ t7) ^ (d | _x1);
            _x2 = (t3 ^ _x1) ^ (_x0 ^ (a & _x3));
        }

        /// <summary>
        /// Apply the linear transformation to the register set.
        /// </summary>
        private void LT()
        {
            int x0 = RotateLeft(_x0, 13);
            int x2 = RotateLeft(_x2, 3);
            int x1 = _x1 ^ x0 ^ x2;
            int x3 = _x3 ^ x2 ^ x0 << 3;

            _x1 = RotateLeft(x1, 1);
            _x3 = RotateLeft(x3, 7);
            _x0 = RotateLeft(x0 ^ _x1 ^ _x3, 5);
            _x2 = RotateLeft(x2 ^ _x3 ^ (_x1 << 7), 22);
        }

        /// <summary>
        /// Apply the inverse of the linear transformation to the register set.
        /// </summary>
        private void InverseLT()
        {
            int x2 = RotateRight(_x2, 22) ^ _x3 ^ (_x1 << 7);
            int x0 = RotateRight(_x0, 5) ^ _x1 ^ _x3;
            int x3 = RotateRight(_x3, 7);
            int x1 = RotateRight(_x1, 1);
            _x3 = x3 ^ x2 ^ x0 << 3;
            _x1 = x1 ^ x0 ^ x2;
            _x2 = RotateRight(x2, 3);
            _x0 = RotateRight(x0, 13);
        }
    }
}
