#if FEATURE_HMAC_SHA384

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    public class HMACSHA384 : System.Security.Cryptography.HMACSHA384
    {
        private readonly int _hashSize;

        public HMACSHA384(byte[] key)
            : base(key)
        {
            _hashSize = base.HashSize;
        }

        public HMACSHA384(byte[] key, int hashSize)
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

#endif // FEATURE_HMAC_SHA384
