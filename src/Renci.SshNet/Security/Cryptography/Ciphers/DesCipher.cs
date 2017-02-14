using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements DES cipher algorithm.
    /// </summary>
    public class DesCipher : BlockCipher
    {
        private int[] _encryptionKey;

        private int[] _decryptionKey;

        #region Static tables

        private static readonly short[] Bytebit = {128, 64, 32, 16, 8, 4, 2, 1};

        private static readonly int[] Bigbyte =
        {
            0x800000, 0x400000, 0x200000, 0x100000,
            0x080000, 0x040000, 0x020000, 0x010000,
            0x008000, 0x004000, 0x002000, 0x001000,
            0x000800, 0x000400, 0x000200, 0x000100,
            0x000080, 0x000040, 0x000020, 0x000010,
            0x000008, 0x000004, 0x000002, 0x000001
        };

        /*
         * Use the key schedule specified in the Standard (ANSI X3.92-1981).
         */

        private static readonly byte[] Pc1 =
        {
            56, 48, 40, 32, 24, 16, 8, 0, 57, 49, 41, 33, 25, 17,
            9, 1, 58, 50, 42, 34, 26, 18, 10, 2, 59, 51, 43, 35,
            62, 54, 46, 38, 30, 22, 14, 6, 61, 53, 45, 37, 29, 21,
            13, 5, 60, 52, 44, 36, 28, 20, 12, 4, 27, 19, 11, 3
        };

        private static readonly byte[] Totrot =
        {
            1, 2, 4, 6, 8, 10, 12, 14,
            15, 17, 19, 21, 23, 25, 27, 28
        };

        private static readonly byte[] Pc2 =
        {
            13, 16, 10, 23, 0, 4, 2, 27, 14, 5, 20, 9,
            22, 18, 11, 3, 25, 7, 15, 6, 26, 19, 12, 1,
            40, 51, 30, 36, 46, 54, 29, 39, 50, 44, 32, 47,
            43, 48, 38, 55, 33, 52, 45, 41, 49, 35, 28, 31
        };

        private static readonly uint[] Sp1 =
        {
            0x01010400, 0x00000000, 0x00010000, 0x01010404,
            0x01010004, 0x00010404, 0x00000004, 0x00010000,
            0x00000400, 0x01010400, 0x01010404, 0x00000400,
            0x01000404, 0x01010004, 0x01000000, 0x00000004,
            0x00000404, 0x01000400, 0x01000400, 0x00010400,
            0x00010400, 0x01010000, 0x01010000, 0x01000404,
            0x00010004, 0x01000004, 0x01000004, 0x00010004,
            0x00000000, 0x00000404, 0x00010404, 0x01000000,
            0x00010000, 0x01010404, 0x00000004, 0x01010000,
            0x01010400, 0x01000000, 0x01000000, 0x00000400,
            0x01010004, 0x00010000, 0x00010400, 0x01000004,
            0x00000400, 0x00000004, 0x01000404, 0x00010404,
            0x01010404, 0x00010004, 0x01010000, 0x01000404,
            0x01000004, 0x00000404, 0x00010404, 0x01010400,
            0x00000404, 0x01000400, 0x01000400, 0x00000000,
            0x00010004, 0x00010400, 0x00000000, 0x01010004
        };

        private static readonly uint[] Sp2 =
        {
            0x80108020, 0x80008000, 0x00008000, 0x00108020,
            0x00100000, 0x00000020, 0x80100020, 0x80008020,
            0x80000020, 0x80108020, 0x80108000, 0x80000000,
            0x80008000, 0x00100000, 0x00000020, 0x80100020,
            0x00108000, 0x00100020, 0x80008020, 0x00000000,
            0x80000000, 0x00008000, 0x00108020, 0x80100000,
            0x00100020, 0x80000020, 0x00000000, 0x00108000,
            0x00008020, 0x80108000, 0x80100000, 0x00008020,
            0x00000000, 0x00108020, 0x80100020, 0x00100000,
            0x80008020, 0x80100000, 0x80108000, 0x00008000,
            0x80100000, 0x80008000, 0x00000020, 0x80108020,
            0x00108020, 0x00000020, 0x00008000, 0x80000000,
            0x00008020, 0x80108000, 0x00100000, 0x80000020,
            0x00100020, 0x80008020, 0x80000020, 0x00100020,
            0x00108000, 0x00000000, 0x80008000, 0x00008020,
            0x80000000, 0x80100020, 0x80108020, 0x00108000
        };

        private static readonly uint[] Sp3 =
        {
            0x00000208, 0x08020200, 0x00000000, 0x08020008,
            0x08000200, 0x00000000, 0x00020208, 0x08000200,
            0x00020008, 0x08000008, 0x08000008, 0x00020000,
            0x08020208, 0x00020008, 0x08020000, 0x00000208,
            0x08000000, 0x00000008, 0x08020200, 0x00000200,
            0x00020200, 0x08020000, 0x08020008, 0x00020208,
            0x08000208, 0x00020200, 0x00020000, 0x08000208,
            0x00000008, 0x08020208, 0x00000200, 0x08000000,
            0x08020200, 0x08000000, 0x00020008, 0x00000208,
            0x00020000, 0x08020200, 0x08000200, 0x00000000,
            0x00000200, 0x00020008, 0x08020208, 0x08000200,
            0x08000008, 0x00000200, 0x00000000, 0x08020008,
            0x08000208, 0x00020000, 0x08000000, 0x08020208,
            0x00000008, 0x00020208, 0x00020200, 0x08000008,
            0x08020000, 0x08000208, 0x00000208, 0x08020000,
            0x00020208, 0x00000008, 0x08020008, 0x00020200
        };

        private static readonly uint[] Sp4 =
        {
            0x00802001, 0x00002081, 0x00002081, 0x00000080,
            0x00802080, 0x00800081, 0x00800001, 0x00002001,
            0x00000000, 0x00802000, 0x00802000, 0x00802081,
            0x00000081, 0x00000000, 0x00800080, 0x00800001,
            0x00000001, 0x00002000, 0x00800000, 0x00802001,
            0x00000080, 0x00800000, 0x00002001, 0x00002080,
            0x00800081, 0x00000001, 0x00002080, 0x00800080,
            0x00002000, 0x00802080, 0x00802081, 0x00000081,
            0x00800080, 0x00800001, 0x00802000, 0x00802081,
            0x00000081, 0x00000000, 0x00000000, 0x00802000,
            0x00002080, 0x00800080, 0x00800081, 0x00000001,
            0x00802001, 0x00002081, 0x00002081, 0x00000080,
            0x00802081, 0x00000081, 0x00000001, 0x00002000,
            0x00800001, 0x00002001, 0x00802080, 0x00800081,
            0x00002001, 0x00002080, 0x00800000, 0x00802001,
            0x00000080, 0x00800000, 0x00002000, 0x00802080
        };

        private static readonly uint[] Sp5 =
        {
            0x00000100, 0x02080100, 0x02080000, 0x42000100,
            0x00080000, 0x00000100, 0x40000000, 0x02080000,
            0x40080100, 0x00080000, 0x02000100, 0x40080100,
            0x42000100, 0x42080000, 0x00080100, 0x40000000,
            0x02000000, 0x40080000, 0x40080000, 0x00000000,
            0x40000100, 0x42080100, 0x42080100, 0x02000100,
            0x42080000, 0x40000100, 0x00000000, 0x42000000,
            0x02080100, 0x02000000, 0x42000000, 0x00080100,
            0x00080000, 0x42000100, 0x00000100, 0x02000000,
            0x40000000, 0x02080000, 0x42000100, 0x40080100,
            0x02000100, 0x40000000, 0x42080000, 0x02080100,
            0x40080100, 0x00000100, 0x02000000, 0x42080000,
            0x42080100, 0x00080100, 0x42000000, 0x42080100,
            0x02080000, 0x00000000, 0x40080000, 0x42000000,
            0x00080100, 0x02000100, 0x40000100, 0x00080000,
            0x00000000, 0x40080000, 0x02080100, 0x40000100
        };

        private static readonly uint[] Sp6 =
        {
            0x20000010, 0x20400000, 0x00004000, 0x20404010,
            0x20400000, 0x00000010, 0x20404010, 0x00400000,
            0x20004000, 0x00404010, 0x00400000, 0x20000010,
            0x00400010, 0x20004000, 0x20000000, 0x00004010,
            0x00000000, 0x00400010, 0x20004010, 0x00004000,
            0x00404000, 0x20004010, 0x00000010, 0x20400010,
            0x20400010, 0x00000000, 0x00404010, 0x20404000,
            0x00004010, 0x00404000, 0x20404000, 0x20000000,
            0x20004000, 0x00000010, 0x20400010, 0x00404000,
            0x20404010, 0x00400000, 0x00004010, 0x20000010,
            0x00400000, 0x20004000, 0x20000000, 0x00004010,
            0x20000010, 0x20404010, 0x00404000, 0x20400000,
            0x00404010, 0x20404000, 0x00000000, 0x20400010,
            0x00000010, 0x00004000, 0x20400000, 0x00404010,
            0x00004000, 0x00400010, 0x20004010, 0x00000000,
            0x20404000, 0x20000000, 0x00400010, 0x20004010
        };

        private static readonly uint[] Sp7 =
        {
            0x00200000, 0x04200002, 0x04000802, 0x00000000,
            0x00000800, 0x04000802, 0x00200802, 0x04200800,
            0x04200802, 0x00200000, 0x00000000, 0x04000002,
            0x00000002, 0x04000000, 0x04200002, 0x00000802,
            0x04000800, 0x00200802, 0x00200002, 0x04000800,
            0x04000002, 0x04200000, 0x04200800, 0x00200002,
            0x04200000, 0x00000800, 0x00000802, 0x04200802,
            0x00200800, 0x00000002, 0x04000000, 0x00200800,
            0x04000000, 0x00200800, 0x00200000, 0x04000802,
            0x04000802, 0x04200002, 0x04200002, 0x00000002,
            0x00200002, 0x04000000, 0x04000800, 0x00200000,
            0x04200800, 0x00000802, 0x00200802, 0x04200800,
            0x00000802, 0x04000002, 0x04200802, 0x04200000,
            0x00200800, 0x00000000, 0x00000002, 0x04200802,
            0x00000000, 0x00200802, 0x04200000, 0x00000800,
            0x04000002, 0x04000800, 0x00000800, 0x00200002
        };

        private static readonly uint[] Sp8 =
        {
            0x10001040, 0x00001000, 0x00040000, 0x10041040,
            0x10000000, 0x10001040, 0x00000040, 0x10000000,
            0x00040040, 0x10040000, 0x10041040, 0x00041000,
            0x10041000, 0x00041040, 0x00001000, 0x00000040,
            0x10040000, 0x10000040, 0x10001000, 0x00001040,
            0x00041000, 0x00040040, 0x10040040, 0x10041000,
            0x00001040, 0x00000000, 0x00000000, 0x10040040,
            0x10000040, 0x10001000, 0x00041040, 0x00040000,
            0x00041040, 0x00040000, 0x10041000, 0x00001000,
            0x00000040, 0x10040040, 0x00001000, 0x00041040,
            0x10001000, 0x00000040, 0x10000040, 0x10040000,
            0x10040040, 0x10000000, 0x00040000, 0x10001040,
            0x00000000, 0x10041040, 0x00040040, 0x10000040,
            0x10040000, 0x10001000, 0x10001040, 0x00000000,
            0x10041040, 0x00041000, 0x00041000, 0x00001040,
            0x00001040, 0x00040040, 0x10000000, 0x10041000
        };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="padding">The padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public DesCipher(byte[] key, CipherMode mode, CipherPadding padding)
            : base(key, 8, mode, padding)
        {
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
            if ((inputOffset + BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            if (_encryptionKey == null)
            {
                _encryptionKey = GenerateWorkingKey(true, Key);
            }

            DesFunc(_encryptionKey, inputBuffer, inputOffset, outputBuffer, outputOffset);

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
            if ((inputOffset + BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            if (_decryptionKey == null)
            {
                _decryptionKey = GenerateWorkingKey(false, Key);
            }

            DesFunc(_decryptionKey, inputBuffer, inputOffset, outputBuffer, outputOffset);

            return BlockSize;
        }

        /// <summary>
        /// Generates the working key.
        /// </summary>
        /// <param name="encrypting">if set to <c>true</c> [encrypting].</param>
        /// <param name="key">The key.</param>
        /// <returns>Generated working key.</returns>
        protected int[] GenerateWorkingKey(bool encrypting, byte[] key)
        {
            ValidateKey();

            int[] newKey = new int[32];
            bool[] pc1m = new bool[56];
            bool[] pcr = new bool[56];

            for (int j = 0; j < 56; j++)
            {
                int l = Pc1[j];

                pc1m[j] = ((key[(uint)l >> 3] & Bytebit[l & 07]) != 0);
            }

            for (int i = 0; i < 16; i++)
            {
                int l, m;

                if (encrypting)
                {
                    m = i << 1;
                }
                else
                {
                    m = (15 - i) << 1;
                }

                var n = m + 1;
                newKey[m] = newKey[n] = 0;

                for (int j = 0; j < 28; j++)
                {
                    l = j + Totrot[i];
                    if (l < 28)
                    {
                        pcr[j] = pc1m[l];
                    }
                    else
                    {
                        pcr[j] = pc1m[l - 28];
                    }
                }

                for (int j = 28; j < 56; j++)
                {
                    l = j + Totrot[i];
                    if (l < 56)
                    {
                        pcr[j] = pc1m[l];
                    }
                    else
                    {
                        pcr[j] = pc1m[l - 28];
                    }
                }

                for (int j = 0; j < 24; j++)
                {
                    if (pcr[Pc2[j]])
                    {
                        newKey[m] |= Bigbyte[j];
                    }

                    if (pcr[Pc2[j + 24]])
                    {
                        newKey[n] |= Bigbyte[j];
                    }
                }
            }

            //
            // store the processed key
            //
            for (int i = 0; i != 32; i += 2)
            {
                var i1 = newKey[i];
                var i2 = newKey[i + 1];

                newKey[i] = (int) ((uint) ((i1 & 0x00fc0000) << 6) |
                                   (uint) ((i1 & 0x00000fc0) << 10) |
                                   ((uint) (i2 & 0x00fc0000) >> 10) |
                                   ((uint) (i2 & 0x00000fc0) >> 6));

                newKey[i + 1] = (int) ((uint) ((i1 & 0x0003f000) << 12) |
                                       (uint) ((i1 & 0x0000003f) << 16) |
                                       ((uint) (i2 & 0x0003f000) >> 4) |
                                       (uint) (i2 & 0x0000003f));
            }

            return newKey;
        }

        /// <summary>
        /// Validates the key.
        /// </summary>
        protected virtual void ValidateKey()
        {
            var keySize = Key.Length * 8;

            if (keySize != 64)
                throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", keySize));
        }

        /// <summary>
        /// Performs DES function.
        /// </summary>
        /// <param name="wKey">The w key.</param>
        /// <param name="input">The input.</param>
        /// <param name="inOff">The in off.</param>
        /// <param name="outBytes">The out bytes.</param>
        /// <param name="outOff">The out off.</param>
        protected static void DesFunc(int[] wKey, byte[] input, int inOff, byte[] outBytes, int outOff)
        {
            var left = Pack.BigEndianToUInt32(input, inOff);
            var right = Pack.BigEndianToUInt32(input, inOff + 4);

            var work = ((left >> 4) ^ right) & 0x0f0f0f0f;
            right ^= work;
            left ^= (work << 4);
            work = ((left >> 16) ^ right) & 0x0000ffff;
            right ^= work;
            left ^= (work << 16);
            work = ((right >> 2) ^ left) & 0x33333333;
            left ^= work;
            right ^= (work << 2);
            work = ((right >> 8) ^ left) & 0x00ff00ff;
            left ^= work;
            right ^= (work << 8);
            right = (right << 1) | (right >> 31);
            work = (left ^ right) & 0xaaaaaaaa;
            left ^= work;
            right ^= work;
            left = (left << 1) | (left >> 31);

            for (var round = 0; round < 8; round++)
            {
                work = (right << 28) | (right >> 4);
                work ^= (uint)wKey[round * 4 + 0];
                var fval = Sp7[work & 0x3f];
                fval |= Sp5[(work >> 8) & 0x3f];
                fval |= Sp3[(work >> 16) & 0x3f];
                fval |= Sp1[(work >> 24) & 0x3f];
                work = right ^ (uint) wKey[round * 4 + 1];
                fval |= Sp8[work & 0x3f];
                fval |= Sp6[(work >> 8) & 0x3f];
                fval |= Sp4[(work >> 16) & 0x3f];
                fval |= Sp2[(work >> 24) & 0x3f];
                left ^= fval;
                work = (left << 28) | (left >> 4);
                work ^= (uint)wKey[round * 4 + 2];
                fval = Sp7[work & 0x3f];
                fval |= Sp5[(work >> 8) & 0x3f];
                fval |= Sp3[(work >> 16) & 0x3f];
                fval |= Sp1[(work >> 24) & 0x3f];
                work = left ^ (uint)wKey[round * 4 + 3];
                fval |= Sp8[work & 0x3f];
                fval |= Sp6[(work >> 8) & 0x3f];
                fval |= Sp4[(work >> 16) & 0x3f];
                fval |= Sp2[(work >> 24) & 0x3f];
                right ^= fval;
            }

            right = (right << 31) | (right >> 1);
            work = (left ^ right) & 0xaaaaaaaa;
            left ^= work;
            right ^= work;
            left = (left << 31) | (left >> 1);
            work = ((left >> 8) ^ right) & 0x00ff00ff;
            right ^= work;
            left ^= (work << 8);
            work = ((left >> 2) ^ right) & 0x33333333;
            right ^= work;
            left ^= (work << 2);
            work = ((right >> 16) ^ left) & 0x0000ffff;
            left ^= work;
            right ^= (work << 16);
            work = ((right >> 4) ^ left) & 0x0f0f0f0f;
            left ^= work;
            right ^= (work << 4);

            Pack.UInt32ToBigEndian(right, outBytes, outOff);
            Pack.UInt32ToBigEndian(left, outBytes, outOff + 4);
        }
    }
}
