using System;
using System.Collections;

using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Digests;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto;
using Renci.SshNet.Security.Org.BouncyCastle.Utilities;
using System.Collections.Generic;

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

        private static readonly IDictionary algorithms = new Dictionary<object, object>();
        private static readonly IDictionary oids = new Dictionary<object, object>();

        static DigestUtilities()
        {
            // Signal to obfuscation tools not to change enum constants
            ((DigestAlgorithm)Enums.GetArbitraryValue(typeof(DigestAlgorithm))).ToString();

            algorithms["SHA256"] = "SHA-256";
            algorithms["2.16.840.1.101.3.4.2.1"] = "SHA-256";
        }

        public static ICollection Algorithms
        {
            get { return oids.Keys; }
        }

        public static IDigest GetDigest(
            string algorithm)
        {
            string upper = algorithm.ToUpper();
            string mechanism = (string) algorithms[upper];

            if (mechanism == null)
            {
                mechanism = upper;
            }

            try
            {
                DigestAlgorithm digestAlgorithm = (DigestAlgorithm)Enums.GetEnumValue(
                    typeof(DigestAlgorithm), mechanism);

                switch (digestAlgorithm)
                {
                    case DigestAlgorithm.SHA_256: return new Sha256Digest();
                }
            }
            catch (ArgumentException)
            {
            }

            throw new SecurityUtilityException("Digest " + mechanism + " not recognised.");
        }

        public static byte[] CalculateDigest(string algorithm, byte[] input)
        {
            IDigest digest = GetDigest(algorithm);
            digest.BlockUpdate(input, 0, input.Length);
            return DoFinal(digest);
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
