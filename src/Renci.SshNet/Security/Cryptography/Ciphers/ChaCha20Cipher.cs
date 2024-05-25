using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements ChaCha20 cipher algorithm.
    /// </summary>
    internal sealed class ChaCha20Cipher : StreamCipher
    {
        private const uint ChachaConst0 = 0x61707865;
        private const uint ChachaConst1 = 0x3320646e;
        private const uint ChachaConst2 = 0x79622d32;
        private const uint ChachaConst3 = 0x6b206574;

        private uint _s00;
        private uint _s01;
        private uint _s02;
        private uint _s03;
        private uint _s04;
        private uint _s05;
        private uint _s06;
        private uint _s07;
        private uint _s08;
        private uint _s09;
        private uint _s10;
        private uint _s11;
        private uint _s12;
        private uint _s13;
        private uint _s14;
        private uint _s15;

        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public override byte MinimumSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChaCha20Cipher" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="counter">The init counter.</param>
        public ChaCha20Cipher(byte[] key, byte[] nonce, uint counter = 0)
            : base(key)
        {
            var keySize = key.Length * 8;

            if (keySize is not 256)
            {
                throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", keySize));
            }

            SetState(key, nonce.Take(12), counter);
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// Encrypted data.
        /// </returns>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            var output = new byte[length];
            _ = ProcessBytes(input, offset, length, output, 0);
            return output;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting.</param>
        /// <param name="length">The number of bytes to decrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            return Encrypt(input, offset, length);
        }

        private void AdvanceCounter()
        {
            unchecked
            {
                if (++_s12 == 0)
                {
                    ++_s13;
                }
            }
        }

        private int ProcessBytes(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if ((inputOffset + inputCount) > inputBuffer.Length)
            {
                throw new ArgumentException("input buffer too short");
            }

            if ((outputOffset + inputCount) > outputBuffer.Length)
            {
                throw new ArgumentException("output buffer too short");
            }

            var index = 0;
            var keyStream = new byte[64];

            for (var i = 0; i < inputCount; i++)
            {
                if (index == 0)
                {
                    GenerateKeyStream(keyStream);
                    AdvanceCounter();
                }

                outputBuffer[outputOffset + i] = (byte)(keyStream[index++] ^ inputBuffer[inputOffset + i]);
                index &= 63;
            }

            return inputCount;
        }

        private void GenerateKeyStream(byte[] output)
        {
            var s00 = _s00;
            var s01 = _s01;
            var s02 = _s02;
            var s03 = _s03;
            var s04 = _s04;
            var s05 = _s05;
            var s06 = _s06;
            var s07 = _s07;
            var s08 = _s08;
            var s09 = _s09;
            var s10 = _s10;
            var s11 = _s11;
            var s12 = _s12;
            var s13 = _s13;
            var s14 = _s14;
            var s15 = _s15;

            static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
            {
                a += b;
                d ^= a;
                d = d << 16 | d >> 16;
                c += d;
                b ^= c;
                b = b << 12 | b >> 20;
                a += b;
                d ^= a;
                d = d << 8 | d >> 24;
                c += d;
                b ^= c;
                b = b << 7 | b >> 25;
            }

            unchecked
            {
                for (var i = 20; i > 0; i -= 2)
                {
                    QuarterRound(ref s00, ref s04, ref s08, ref s12);
                    QuarterRound(ref s01, ref s05, ref s09, ref s13);
                    QuarterRound(ref s02, ref s06, ref s10, ref s14);
                    QuarterRound(ref s03, ref s07, ref s11, ref s15);
                    QuarterRound(ref s00, ref s05, ref s10, ref s15);
                    QuarterRound(ref s01, ref s06, ref s11, ref s12);
                    QuarterRound(ref s02, ref s07, ref s08, ref s13);
                    QuarterRound(ref s03, ref s04, ref s09, ref s14);
                }

                Pack.UInt32ToLittleEndian(_s00 + s00, output, 0);
                Pack.UInt32ToLittleEndian(_s01 + s01, output, 4);
                Pack.UInt32ToLittleEndian(_s02 + s02, output, 8);
                Pack.UInt32ToLittleEndian(_s03 + s03, output, 12);
                Pack.UInt32ToLittleEndian(_s04 + s04, output, 16);
                Pack.UInt32ToLittleEndian(_s05 + s05, output, 20);
                Pack.UInt32ToLittleEndian(_s06 + s06, output, 24);
                Pack.UInt32ToLittleEndian(_s07 + s07, output, 28);
                Pack.UInt32ToLittleEndian(_s08 + s08, output, 32);
                Pack.UInt32ToLittleEndian(_s09 + s09, output, 36);
                Pack.UInt32ToLittleEndian(_s10 + s10, output, 40);
                Pack.UInt32ToLittleEndian(_s11 + s11, output, 44);
                Pack.UInt32ToLittleEndian(_s12 + s12, output, 48);
                Pack.UInt32ToLittleEndian(_s13 + s13, output, 52);
                Pack.UInt32ToLittleEndian(_s14 + s14, output, 56);
                Pack.UInt32ToLittleEndian(_s15 + s15, output, 60);
            }
        }

        private void SetState(byte[] key, byte[] nonce, uint counter)
        {
            _s00 = ChachaConst0;
            _s01 = ChachaConst1;
            _s02 = ChachaConst2;
            _s03 = ChachaConst3;

            _s04 = Pack.LittleEndianToUInt32(key, 0);
            _s05 = Pack.LittleEndianToUInt32(key, 4);
            _s06 = Pack.LittleEndianToUInt32(key, 8);
            _s07 = Pack.LittleEndianToUInt32(key, 12);
            _s08 = Pack.LittleEndianToUInt32(key, 16);
            _s09 = Pack.LittleEndianToUInt32(key, 20);
            _s10 = Pack.LittleEndianToUInt32(key, 24);
            _s11 = Pack.LittleEndianToUInt32(key, 28);

            _s12 = counter;

            _s13 = Pack.LittleEndianToUInt32(nonce, 0);
            _s14 = Pack.LittleEndianToUInt32(nonce, 4);
            _s15 = Pack.LittleEndianToUInt32(nonce, 8);
        }
    }
}
