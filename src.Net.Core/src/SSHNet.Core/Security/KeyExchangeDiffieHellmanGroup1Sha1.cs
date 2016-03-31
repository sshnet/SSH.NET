using Renci.SshNet.Common;
using System.Globalization;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group1-sha1" algorithm implementation.
    /// </summary>
    public class KeyExchangeDiffieHellmanGroup1Sha1 : KeyExchangeDiffieHellmanGroupSha1
    {
        private const string SecondOkleyGroup = @"00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE65381FFFFFFFFFFFFFFFF";

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "diffie-hellman-group1-sha1"; }
        }

        /// <summary>
        /// Gets the group prime.
        /// </summary>
        /// <value>
        /// The group prime.
        /// </value>
        public override BigInteger GroupPrime
        {
            get
            {
                BigInteger prime;
                BigInteger.TryParse(SecondOkleyGroup, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out prime);
                return prime;
            }
        }
    }
}
