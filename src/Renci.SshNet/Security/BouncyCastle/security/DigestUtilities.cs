using Renci.SshNet.Security.Org.BouncyCastle.Crypto;

namespace Renci.SshNet.Security.Org.BouncyCastle.Security
{
    /// <remarks>
    ///  Utility class for creating IDigest objects from their names/Oids
    /// </remarks>
    internal sealed class DigestUtilities
    {
        private enum DigestAlgorithm {
            SHA_256
        };

        private DigestUtilities()
        {
        }

        public static byte[] DoFinal(
            IDigest digest)
        {
            byte[] b = new byte[digest.GetDigestSize()];
            digest.DoFinal(b, 0);
            return b;
        }

        public static byte[] DoFinal(
            IDigest	digest,
            byte[]	input)
        {
            digest.BlockUpdate(input, 0, input.Length);
            return DoFinal(digest);
        }
    }
}
