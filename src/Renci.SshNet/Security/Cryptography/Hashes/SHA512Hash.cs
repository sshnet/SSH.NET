namespace Renci.SshNet.Security.Cryptography
{
    public class SHA512Hash : SHA2HashBase
    {
        private const int DIGEST_SIZE = 64;

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
        /// When overridden in a derived class, gets the input block size.
        /// </summary>
        /// <returns>The input block size.</returns>
        public override int InputBlockSize
        {
            get
            {
                return DIGEST_SIZE * 2;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the output block size.
        /// </summary>
        /// <returns>The output block size.</returns>
        public override int OutputBlockSize
        {
            get
            {
                return DIGEST_SIZE * 2;
            }
        }

        protected override byte[] HashFinal()
        {
            var output = new byte[DIGEST_SIZE];

            this.Finish();

            SHA2HashBase.UInt64_To_BE(H1, output, 0);
            SHA2HashBase.UInt64_To_BE(H2, output, 8);
            SHA2HashBase.UInt64_To_BE(H3, output, 16);
            SHA2HashBase.UInt64_To_BE(H4, output, 24);
            SHA2HashBase.UInt64_To_BE(H5, output, 32);
            SHA2HashBase.UInt64_To_BE(H6, output, 40);
            SHA2HashBase.UInt64_To_BE(H7, output, 48);
            SHA2HashBase.UInt64_To_BE(H8, output, 56);

            this.Initialize();

            return output;
        }

        public override void Initialize()
        {
            base.Initialize();

            /* SHA-512 initial hash value
             * The first 64 bits of the fractional parts of the square roots
             * of the first eight prime numbers
             */
            H1 = 0x6a09e667f3bcc908;
            H2 = 0xbb67ae8584caa73b;
            H3 = 0x3c6ef372fe94f82b;
            H4 = 0xa54ff53a5f1d36f1;
            H5 = 0x510e527fade682d1;
            H6 = 0x9b05688c2b3e6c1f;
            H7 = 0x1f83d9abfb41bd6b;
            H8 = 0x5be0cd19137e2179;
        }
    }
}
