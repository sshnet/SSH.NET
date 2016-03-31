#if FEATURE_HMAC_MD5

using System.Linq;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the MD5 hash function.
    /// </summary>
    public class HMACMD5 : System.Security.Cryptography.HMACMD5
    {
        private readonly int _hashSize;

        public HMACMD5(byte[] key)
            : base(key)
        {
            _hashSize = base.HashSize;
        }

        public HMACMD5(byte[] key, int hashSize)
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

#endif // FEATURE_HMAC_MD5
