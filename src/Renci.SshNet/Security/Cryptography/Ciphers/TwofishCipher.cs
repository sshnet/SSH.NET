using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements Twofish cipher algorithm
    /// </summary>
    public sealed class TwofishCipher : BlockCipher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwofishCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="padding">The padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public TwofishCipher(byte[] key, CipherMode mode, CipherPadding padding)
            : base(key, 16, mode, padding)
        {
            var keySize = key.Length * 8;

            if (!(keySize == 128 || keySize == 192 || keySize == 256))
                throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", keySize));

            //  TODO:   Refactor this algorithm

            // calculate the MDS matrix
            var m1 = new int[2];
            var mX = new int[2];
            var mY = new int[2];

            for (var i = 0; i < MAX_KEY_BITS; i++)
            {
                var j = P[0 + i] & 0xff;
                m1[0] = j;
                mX[0] = Mx_X(j) & 0xff;
                mY[0] = Mx_Y(j) & 0xff;

                j = P[(1 * 256) + i] & 0xff;
                m1[1] = j;
                mX[1] = Mx_X(j) & 0xff;
                mY[1] = Mx_Y(j) & 0xff;

                gMDS0[i] = m1[P_00] | mX[P_00] << 8 | mY[P_00] << 16 | mY[P_00] << 24;

                gMDS1[i] = mY[P_10] | mY[P_10] << 8 | mX[P_10] << 16 | m1[P_10] << 24;

                gMDS2[i] = mX[P_20] | mY[P_20] << 8 | m1[P_20] << 16 | mY[P_20] << 24;

                gMDS3[i] = mX[P_30] | m1[P_30] << 8 | mY[P_30] << 16 | mX[P_30] << 24;
            }

            _k64Cnt = key.Length / 8; // pre-padded ?
            SetKey(key);
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
            var x0 = BytesTo32Bits(inputBuffer, inputOffset) ^ gSubKeys[INPUT_WHITEN];
            var x1 = BytesTo32Bits(inputBuffer, inputOffset + 4) ^ gSubKeys[INPUT_WHITEN + 1];
            var x2 = BytesTo32Bits(inputBuffer, inputOffset + 8) ^ gSubKeys[INPUT_WHITEN + 2];
            var x3 = BytesTo32Bits(inputBuffer, inputOffset + 12) ^ gSubKeys[INPUT_WHITEN + 3];

            var k = ROUND_SUBKEYS;
            for (var r = 0; r < ROUNDS; r += 2)
            {
                var t0 = Fe32_0(gSBox, x0);
                var t1 = Fe32_3(gSBox, x1);
                x2 ^= t0 + t1 + gSubKeys[k++];
                x2 = (int)((uint)x2 >> 1) | x2 << 31;
                x3 = (x3 << 1 | (int)((uint)x3 >> 31)) ^ (t0 + 2 * t1 + gSubKeys[k++]);

                t0 = Fe32_0(gSBox, x2);
                t1 = Fe32_3(gSBox, x3);
                x0 ^= t0 + t1 + gSubKeys[k++];
                x0 = (int)((uint)x0 >> 1) | x0 << 31;
                x1 = (x1 << 1 | (int)((uint)x1 >> 31)) ^ (t0 + 2 * t1 + gSubKeys[k++]);
            }

            Bits32ToBytes(x2 ^ gSubKeys[OUTPUT_WHITEN], outputBuffer, outputOffset);
            Bits32ToBytes(x3 ^ gSubKeys[OUTPUT_WHITEN + 1], outputBuffer, outputOffset + 4);
            Bits32ToBytes(x0 ^ gSubKeys[OUTPUT_WHITEN + 2], outputBuffer, outputOffset + 8);
            Bits32ToBytes(x1 ^ gSubKeys[OUTPUT_WHITEN + 3], outputBuffer, outputOffset + 12);

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
            var x2 = BytesTo32Bits(inputBuffer, inputOffset) ^ gSubKeys[OUTPUT_WHITEN];
            var x3 = BytesTo32Bits(inputBuffer, inputOffset + 4) ^ gSubKeys[OUTPUT_WHITEN + 1];
            var x0 = BytesTo32Bits(inputBuffer, inputOffset + 8) ^ gSubKeys[OUTPUT_WHITEN + 2];
            var x1 = BytesTo32Bits(inputBuffer, inputOffset + 12) ^ gSubKeys[OUTPUT_WHITEN + 3];

            var k = ROUND_SUBKEYS + 2 * ROUNDS - 1;
            for (var r = 0; r < ROUNDS; r += 2)
            {
                var t0 = Fe32_0(gSBox, x2);
                var t1 = Fe32_3(gSBox, x3);
                x1 ^= t0 + 2 * t1 + gSubKeys[k--];
                x0 = (x0 << 1 | (int)((uint)x0 >> 31)) ^ (t0 + t1 + gSubKeys[k--]);
                x1 = (int)((uint)x1 >> 1) | x1 << 31;

                t0 = Fe32_0(gSBox, x0);
                t1 = Fe32_3(gSBox, x1);
                x3 ^= t0 + 2 * t1 + gSubKeys[k--];
                x2 = (x2 << 1 | (int)((uint)x2 >> 31)) ^ (t0 + t1 + gSubKeys[k--]);
                x3 = (int)((uint)x3 >> 1) | x3 << 31;
            }

            Bits32ToBytes(x0 ^ gSubKeys[INPUT_WHITEN], outputBuffer, outputOffset);
            Bits32ToBytes(x1 ^ gSubKeys[INPUT_WHITEN + 1], outputBuffer, outputOffset + 4);
            Bits32ToBytes(x2 ^ gSubKeys[INPUT_WHITEN + 2], outputBuffer, outputOffset + 8);
            Bits32ToBytes(x3 ^ gSubKeys[INPUT_WHITEN + 3], outputBuffer, outputOffset + 12);

            return BlockSize;
        }

        #region Static Definition Tables

        private static readonly byte[] P =
            {
                // p0
                0xA9, 0x67, 0xB3, 0xE8, 0x04, 0xFD, 0xA3, 0x76, 0x9A, 0x92, 0x80, 0x78, 0xE4, 0xDD, 0xD1, 0x38,
                0x0D, 0xC6, 0x35, 0x98, 0x18, 0xF7, 0xEC, 0x6C, 0x43, 0x75, 0x37, 0x26, 0xFA, 0x13, 0x94, 0x48,
                0xF2, 0xD0, 0x8B, 0x30, 0x84, 0x54, 0xDF, 0x23, 0x19, 0x5B, 0x3D, 0x59, 0xF3, 0xAE, 0xA2, 0x82,
                0x63, 0x01, 0x83, 0x2E, 0xD9, 0x51, 0x9B, 0x7C, 0xA6, 0xEB, 0xA5, 0xBE, 0x16, 0x0C, 0xE3, 0x61,
                0xC0, 0x8C, 0x3A, 0xF5, 0x73, 0x2C, 0x25, 0x0B, 0xBB, 0x4E, 0x89, 0x6B, 0x53, 0x6A, 0xB4, 0xF1,
                0xE1, 0xE6, 0xBD, 0x45, 0xE2, 0xF4, 0xB6, 0x66, 0xCC, 0x95, 0x03, 0x56, 0xD4, 0x1C, 0x1E, 0xD7,
                0xFB, 0xC3, 0x8E, 0xB5, 0xE9, 0xCF, 0xBF, 0xBA, 0xEA, 0x77, 0x39, 0xAF, 0x33, 0xC9, 0x62, 0x71,
                0x81, 0x79, 0x09, 0xAD, 0x24, 0xCD, 0xF9, 0xD8, 0xE5, 0xC5, 0xB9, 0x4D, 0x44, 0x08, 0x86, 0xE7,
                0xA1, 0x1D, 0xAA, 0xED, 0x06, 0x70, 0xB2, 0xD2, 0x41, 0x7B, 0xA0, 0x11, 0x31, 0xC2, 0x27, 0x90,
                0x20, 0xF6, 0x60, 0xFF, 0x96, 0x5C, 0xB1, 0xAB, 0x9E, 0x9C, 0x52, 0x1B, 0x5F, 0x93, 0x0A, 0xEF,
                0x91, 0x85, 0x49, 0xEE, 0x2D, 0x4F, 0x8F, 0x3B, 0x47, 0x87, 0x6D, 0x46, 0xD6, 0x3E, 0x69, 0x64,
                0x2A, 0xCE, 0xCB, 0x2F, 0xFC, 0x97, 0x05, 0x7A, 0xAC, 0x7F, 0xD5, 0x1A, 0x4B, 0x0E, 0xA7, 0x5A,
                0x28, 0x14, 0x3F, 0x29, 0x88, 0x3C, 0x4C, 0x02, 0xB8, 0xDA, 0xB0, 0x17, 0x55, 0x1F, 0x8A, 0x7D,
                0x57, 0xC7, 0x8D, 0x74, 0xB7, 0xC4, 0x9F, 0x72, 0x7E, 0x15, 0x22, 0x12, 0x58, 0x07, 0x99, 0x34,
                0x6E, 0x50, 0xDE, 0x68, 0x65, 0xBC, 0xDB, 0xF8, 0xC8, 0xA8, 0x2B, 0x40, 0xDC, 0xFE, 0x32, 0xA4,
                0xCA, 0x10, 0x21, 0xF0, 0xD3, 0x5D, 0x0F, 0x00, 0x6F, 0x9D, 0x36, 0x42, 0x4A, 0x5E, 0xC1, 0xE0,
                // p1
                0x75, 0xF3, 0xC6, 0xF4, 0xDB, 0x7B, 0xFB, 0xC8, 0x4A, 0xD3, 0xE6, 0x6B, 0x45, 0x7D, 0xE8, 0x4B,
                0xD6, 0x32, 0xD8, 0xFD, 0x37, 0x71, 0xF1, 0xE1, 0x30, 0x0F, 0xF8, 0x1B, 0x87, 0xFA, 0x06, 0x3F,
                0x5E, 0xBA, 0xAE, 0x5B, 0x8A, 0x00, 0xBC, 0x9D, 0x6D, 0xC1, 0xB1, 0x0E, 0x80, 0x5D, 0xD2, 0xD5,
                0xA0, 0x84, 0x07, 0x14, 0xB5, 0x90, 0x2C, 0xA3, 0xB2, 0x73, 0x4C, 0x54, 0x92, 0x74, 0x36, 0x51,
                0x38, 0xB0, 0xBD, 0x5A, 0xFC, 0x60, 0x62, 0x96, 0x6C, 0x42, 0xF7, 0x10, 0x7C, 0x28, 0x27, 0x8C,
                0x13, 0x95, 0x9C, 0xC7, 0x24, 0x46, 0x3B, 0x70, 0xCA, 0xE3, 0x85, 0xCB, 0x11, 0xD0, 0x93, 0xB8,
                0xA6, 0x83, 0x20, 0xFF, 0x9F, 0x77, 0xC3, 0xCC, 0x03, 0x6F, 0x08, 0xBF, 0x40, 0xE7, 0x2B, 0xE2,
                0x79, 0x0C, 0xAA, 0x82, 0x41, 0x3A, 0xEA, 0xB9, 0xE4, 0x9A, 0xA4, 0x97, 0x7E, 0xDA, 0x7A, 0x17,
                0x66, 0x94, 0xA1, 0x1D, 0x3D, 0xF0, 0xDE, 0xB3, 0x0B, 0x72, 0xA7, 0x1C, 0xEF, 0xD1, 0x53, 0x3E,
                0x8F, 0x33, 0x26, 0x5F, 0xEC, 0x76, 0x2A, 0x49, 0x81, 0x88, 0xEE, 0x21, 0xC4, 0x1A, 0xEB, 0xD9,
                0xC5, 0x39, 0x99, 0xCD, 0xAD, 0x31, 0x8B, 0x01, 0x18, 0x23, 0xDD, 0x1F, 0x4E, 0x2D, 0xF9, 0x48,
                0x4F, 0xF2, 0x65, 0x8E, 0x78, 0x5C, 0x58, 0x19, 0x8D, 0xE5, 0x98, 0x57, 0x67, 0x7F, 0x05, 0x64,
                0xAF, 0x63, 0xB6, 0xFE, 0xF5, 0xB7, 0x3C, 0xA5, 0xCE, 0xE9, 0x68, 0x44, 0xE0, 0x4D, 0x43, 0x69,
                0x29, 0x2E, 0xAC, 0x15, 0x59, 0xA8, 0x0A, 0x9E, 0x6E, 0x47, 0xDF, 0x34, 0x35, 0x6A, 0xCF, 0xDC,
                0x22, 0xC9, 0xC0, 0x9B, 0x89, 0xD4, 0xED, 0xAB, 0x12, 0xA2, 0x0D, 0x52, 0xBB, 0x02, 0x2F, 0xA9,
                0xD7, 0x61, 0x1E, 0xB4, 0x50, 0x04, 0xF6, 0xC2, 0x16, 0x25, 0x86, 0x56, 0x55, 0x09, 0xBE, 0x91
            };

        #endregion

        /**
		* Define the fixed p0/p1 permutations used in keyed S-box lookup.
		* By changing the following constant definitions, the S-boxes will
		* automatically Get changed in the Twofish engine.
		*/
        private const int P_00 = 1;
        private const int P_01 = 0;
        private const int P_02 = 0;
        private const int P_03 = P_01 ^ 1;
        private const int P_04 = 1;

        private const int P_10 = 0;
        private const int P_11 = 0;
        private const int P_12 = 1;
        private const int P_13 = P_11 ^ 1;
        private const int P_14 = 0;

        private const int P_20 = 1;
        private const int P_21 = 1;
        private const int P_22 = 0;
        private const int P_23 = P_21 ^ 1;
        private const int P_24 = 0;

        private const int P_30 = 0;
        private const int P_31 = 1;
        private const int P_32 = 1;
        private const int P_33 = P_31 ^ 1;
        private const int P_34 = 1;

        /* Primitive polynomial for GF(256) */
        private const int GF256_FDBK = 0x169;
        private const int GF256_FDBK_2 = GF256_FDBK / 2;
        private const int GF256_FDBK_4 = GF256_FDBK / 4;

        private const int RS_GF_FDBK = 0x14D; // field generator

        //====================================
        // Useful constants
        //====================================

        private const int ROUNDS = 16;
        private const int MAX_ROUNDS = 16;  // bytes = 128 bits
        private const int MAX_KEY_BITS = 256;

        private const int INPUT_WHITEN = 0;
        private const int OUTPUT_WHITEN = INPUT_WHITEN + 16 / 4; // 4
        private const int ROUND_SUBKEYS = OUTPUT_WHITEN + 16 / 4;// 8

        private const int TOTAL_SUBKEYS = ROUND_SUBKEYS + 2 * MAX_ROUNDS;// 40

        private const int SK_STEP = 0x02020202;
        private const int SK_BUMP = 0x01010101;
        private const int SK_ROTL = 9;

        private readonly int[] gMDS0 = new int[MAX_KEY_BITS];
        private readonly int[] gMDS1 = new int[MAX_KEY_BITS];
        private readonly int[] gMDS2 = new int[MAX_KEY_BITS];
        private readonly int[] gMDS3 = new int[MAX_KEY_BITS];

        /**
        * gSubKeys[] and gSBox[] are eventually used in the
        * encryption and decryption methods.
        */
        private int[] gSubKeys;
        private int[] gSBox;

        private readonly int _k64Cnt;

        private void SetKey(byte[] key)
        {
            var k32e = new int[MAX_KEY_BITS / 64]; // 4
            var k32o = new int[MAX_KEY_BITS / 64]; // 4

            var sBoxKeys = new int[MAX_KEY_BITS / 64]; // 4
            gSubKeys = new int[TOTAL_SUBKEYS];

            if (_k64Cnt < 1)
            {
                throw new ArgumentException("Key size less than 64 bits");
            }

            if (_k64Cnt > 4)
            {
                throw new ArgumentException("Key size larger than 256 bits");
            }

            /*
            * k64Cnt is the number of 8 byte blocks (64 chunks)
            * that are in the input key.  The input key is a
            * maximum of 32 bytes ( 256 bits ), so the range
            * for k64Cnt is 1..4
            */
            for (var i = 0; i < _k64Cnt; i++)
            {
                var p = i * 8;

                k32e[i] = BytesTo32Bits(key, p);
                k32o[i] = BytesTo32Bits(key, p + 4);

                sBoxKeys[_k64Cnt - 1 - i] = RS_MDS_Encode(k32e[i], k32o[i]);
            }

            for (var i = 0; i < TOTAL_SUBKEYS / 2; i++)
            {
                var q = i * SK_STEP;
                var a = F32(q, k32e);
                var b = F32(q + SK_BUMP, k32o);
                b = b << 8 | (int)((uint)b >> 24);
                a += b;
                gSubKeys[i * 2] = a;
                a += b;
                gSubKeys[i * 2 + 1] = a << SK_ROTL | (int)((uint)a >> (32 - SK_ROTL));
            }

            /*
            * fully expand the table for speed
            */
            var k0 = sBoxKeys[0];
            var k1 = sBoxKeys[1];
            var k2 = sBoxKeys[2];
            var k3 = sBoxKeys[3];
            gSBox = new int[4 * MAX_KEY_BITS];
            for (var i = 0; i < MAX_KEY_BITS; i++)
            {
                int b1, b2, b3;
                var b0 = b1 = b2 = b3 = i;
                switch (_k64Cnt & 3)
                {
                    case 1:
                        gSBox[i * 2] = gMDS0[(P[P_01 * 256 + b0] & 0xff) ^ M_b0(k0)];
                        gSBox[i * 2 + 1] = gMDS1[(P[P_11 * 256 + b1] & 0xff) ^ M_b1(k0)];
                        gSBox[i * 2 + 0x200] = gMDS2[(P[P_21 * 256 + b2] & 0xff) ^ M_b2(k0)];
                        gSBox[i * 2 + 0x201] = gMDS3[(P[P_31 * 256 + b3] & 0xff) ^ M_b3(k0)];
                        break;
                    case 0: /* 256 bits of key */
                        b0 = (P[P_04 * 256 + b0] & 0xff) ^ M_b0(k3);
                        b1 = (P[P_14 * 256 + b1] & 0xff) ^ M_b1(k3);
                        b2 = (P[P_24 * 256 + b2] & 0xff) ^ M_b2(k3);
                        b3 = (P[P_34 * 256 + b3] & 0xff) ^ M_b3(k3);
                        goto case 3;
                    case 3:
                        b0 = (P[P_03 * 256 + b0] & 0xff) ^ M_b0(k2);
                        b1 = (P[P_13 * 256 + b1] & 0xff) ^ M_b1(k2);
                        b2 = (P[P_23 * 256 + b2] & 0xff) ^ M_b2(k2);
                        b3 = (P[P_33 * 256 + b3] & 0xff) ^ M_b3(k2);
                        goto case 2;
                    case 2:
                        gSBox[i * 2] = gMDS0[(P[P_01 * 256 + (P[P_02 * 256 + b0] & 0xff) ^ M_b0(k1)] & 0xff) ^ M_b0(k0)];
                        gSBox[i * 2 + 1] = gMDS1[(P[P_11 * 256 + (P[P_12 * 256 + b1] & 0xff) ^ M_b1(k1)] & 0xff) ^ M_b1(k0)];
                        gSBox[i * 2 + 0x200] = gMDS2[(P[P_21 * 256 + (P[P_22 * 256 + b2] & 0xff) ^ M_b2(k1)] & 0xff) ^ M_b2(k0)];
                        gSBox[i * 2 + 0x201] = gMDS3[(P[P_31 * 256 + (P[P_32 * 256 + b3] & 0xff) ^ M_b3(k1)] & 0xff) ^ M_b3(k0)];
                        break;
                }
            }

            /*
            * the function exits having setup the gSBox with the
            * input key material.
            */
        }

        /*
        * TODO:  This can be optimised and made cleaner by combining
        * the functionality in this function and applying it appropriately
        * to the creation of the subkeys during key setup.
        */
        private int F32(int x, int[] k32)
        {
            var b0 = M_b0(x);
            var b1 = M_b1(x);
            var b2 = M_b2(x);
            var b3 = M_b3(x);
            var k0 = k32[0];
            var k1 = k32[1];
            var k2 = k32[2];
            var k3 = k32[3];

            var result = 0;
            switch (_k64Cnt & 3)
            {
                case 1:
                    result = gMDS0[(P[P_01 * 256 + b0] & 0xff) ^ M_b0(k0)] ^
                             gMDS1[(P[P_11 * 256 + b1] & 0xff) ^ M_b1(k0)] ^
                             gMDS2[(P[P_21 * 256 + b2] & 0xff) ^ M_b2(k0)] ^
                             gMDS3[(P[P_31 * 256 + b3] & 0xff) ^ M_b3(k0)];
                    break;
                case 0: /* 256 bits of key */
                    b0 = (P[P_04 * 256 + b0] & 0xff) ^ M_b0(k3);
                    b1 = (P[P_14 * 256 + b1] & 0xff) ^ M_b1(k3);
                    b2 = (P[P_24 * 256 + b2] & 0xff) ^ M_b2(k3);
                    b3 = (P[P_34 * 256 + b3] & 0xff) ^ M_b3(k3);
                    goto case 3;
                case 3:
                    b0 = (P[P_03 * 256 + b0] & 0xff) ^ M_b0(k2);
                    b1 = (P[P_13 * 256 + b1] & 0xff) ^ M_b1(k2);
                    b2 = (P[P_23 * 256 + b2] & 0xff) ^ M_b2(k2);
                    b3 = (P[P_33 * 256 + b3] & 0xff) ^ M_b3(k2);
                    goto case 2;
                case 2:
                    result =
                    gMDS0[(P[P_01 * 256 + (P[P_02 * 256 + b0] & 0xff) ^ M_b0(k1)] & 0xff) ^ M_b0(k0)] ^
                    gMDS1[(P[P_11 * 256 + (P[P_12 * 256 + b1] & 0xff) ^ M_b1(k1)] & 0xff) ^ M_b1(k0)] ^
                    gMDS2[(P[P_21 * 256 + (P[P_22 * 256 + b2] & 0xff) ^ M_b2(k1)] & 0xff) ^ M_b2(k0)] ^
                    gMDS3[(P[P_31 * 256 + (P[P_32 * 256 + b3] & 0xff) ^ M_b3(k1)] & 0xff) ^ M_b3(k0)];
                    break;
            }
            return result;
        }

        /**
        * Use (12, 8) Reed-Solomon code over GF(256) to produce
        * a key S-box 32-bit entity from 2 key material 32-bit
        * entities.
        *
        * @param    k0 first 32-bit entity
        * @param    k1 second 32-bit entity
        * @return     Remainder polynomial Generated using RS code
        */
        private static int RS_MDS_Encode(int k0, int k1)
        {
            var r = k1;
            // shift 1 byte at a time
            r = RS_rem(r);
            r = RS_rem(r);
            r = RS_rem(r);
            r = RS_rem(r);
            r ^= k0;
            r = RS_rem(r);
            r = RS_rem(r);
            r = RS_rem(r);
            r = RS_rem(r);

            return r;
        }

        /**
        * Reed-Solomon code parameters: (12,8) reversible code:
        * <p>
        * <pre>
        * G(x) = x^4 + (a+1/a)x^3 + ax^2 + (a+1/a)x + 1
        * </pre>
        * where a = primitive root of field generator 0x14D
        * </p>
        */
        private static int RS_rem(int x)
        {
            var b = (int)(((uint)x >> 24) & 0xff);
            var g2 = ((b << 1) ^
                    ((b & 0x80) != 0 ? RS_GF_FDBK : 0)) & 0xff;
            var g3 = ((int)((uint)b >> 1) ^
                    ((b & 0x01) != 0 ? (int)((uint)RS_GF_FDBK >> 1) : 0)) ^ g2;
            return ((x << 8) ^ (g3 << 24) ^ (g2 << 16) ^ (g3 << 8) ^ b);
        }

        private static int LFSR1(int x)
        {
            return (x >> 1) ^
                    (((x & 0x01) != 0) ? GF256_FDBK_2 : 0);
        }

        private static int LFSR2(int x)
        {
            return (x >> 2) ^
                    (((x & 0x02) != 0) ? GF256_FDBK_2 : 0) ^
                    (((x & 0x01) != 0) ? GF256_FDBK_4 : 0);
        }

        private static int Mx_X(int x)
        {
            return x ^ LFSR2(x);
        } // 5B

        private static int Mx_Y(int x)
        {
            return x ^ LFSR1(x) ^ LFSR2(x);
        } // EF

        private static int M_b0(int x)
        {
            return x & 0xff;
        }

        private static int M_b1(int x)
        {
            return (int)((uint)x >> 8) & 0xff;
        }

        private static int M_b2(int x)
        {
            return (int)((uint)x >> 16) & 0xff;
        }

        private static int M_b3(int x)
        {
            return (int)((uint)x >> 24) & 0xff;
        }

        private static int Fe32_0(int[] gSBox1, int x)
        {
            return gSBox1[0x000 + 2 * (x & 0xff)] ^
                gSBox1[0x001 + 2 * ((int)((uint)x >> 8) & 0xff)] ^
                gSBox1[0x200 + 2 * ((int)((uint)x >> 16) & 0xff)] ^
                gSBox1[0x201 + 2 * ((int)((uint)x >> 24) & 0xff)];
        }

        private static int Fe32_3(int[] gSBox1, int x)
        {
            return gSBox1[0x000 + 2 * ((int)((uint)x >> 24) & 0xff)] ^
                gSBox1[0x001 + 2 * (x & 0xff)] ^
                gSBox1[0x200 + 2 * ((int)((uint)x >> 8) & 0xff)] ^
                gSBox1[0x201 + 2 * ((int)((uint)x >> 16) & 0xff)];
        }

        private static int BytesTo32Bits(byte[] b, int p)
        {
            return ((b[p] & 0xff)) |
                ((b[p + 1] & 0xff) << 8) |
                ((b[p + 2] & 0xff) << 16) |
                ((b[p + 3] & 0xff) << 24);
        }

        private static void Bits32ToBytes(int inData, byte[] b, int offset)
        {
            b[offset] = (byte)inData;
            b[offset + 1] = (byte)(inData >> 8);
            b[offset + 2] = (byte)(inData >> 16);
            b[offset + 3] = (byte)(inData >> 24);
        }
    }
}
