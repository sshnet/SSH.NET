#if FEATURE_HMAC_SHA256

using System.Linq;

namespace Renci.SshNet.Security.Cryptography
{
    public class HMACSHA256 : System.Security.Cryptography.HMACSHA256
    {
        private readonly int _hashSize;

        public HMACSHA256(byte[] key)
            : base(key)
        {
            _hashSize = base.HashSize;
        }

        public HMACSHA256(byte[] key, int hashSize)
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

#endif // FEATURE_HMAC_SHA256
