#if FEATURE_HMAC_SHA512

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    public class HMACSHA512 : System.Security.Cryptography.HMACSHA512
    {
        private readonly int _hashSize;

        public HMACSHA512(byte[] key)
            : base(key)
        {
            _hashSize = base.HashSize;
        }

        public HMACSHA512(byte[] key, int hashSize)
            : base(key)
        {
            _hashSize = hashSize;
        }

        public override int HashSize
        {
            get { return _hashSize; }
        }

        protected override byte[] HashFinal()
        {
            var hash = base.HashFinal();
            return hash.Take(HashSize / 8);
        }
    }
}

#endif // FEATURE_HMAC_SHA512
