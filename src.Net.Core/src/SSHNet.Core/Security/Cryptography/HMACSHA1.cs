#if FEATURE_HMAC_SHA1

using System.Linq;

namespace Renci.SshNet.Security.Cryptography
{
    public class HMACSHA1 : System.Security.Cryptography.HMACSHA1
    {
        private readonly int _hashSize;

        public HMACSHA1(byte[] key)
            : base(key)
        {
            _hashSize = base.HashSize;
        }

        public HMACSHA1(byte[] key, int hashSize)
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
            return hash.Take(HashSize / 8).ToArray();
        }
    }
}

#endif // FEATURE_HMAC_SHA1
