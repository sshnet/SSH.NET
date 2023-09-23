using System;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Describes object identifier for DER encoding.
    /// </summary>
    public struct ObjectIdentifier
    {
        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        public ulong[] Identifiers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifier"/> struct.
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="identifiers"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="identifiers"/> has less than two elements.</exception>
        public ObjectIdentifier(params ulong[] identifiers)
        {
            if (identifiers is null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            if (identifiers.Length < 2)
            {
                throw new ArgumentException("Must contain at least two elements.", nameof(identifiers));
            }

            Identifiers = identifiers;
        }

        internal static ObjectIdentifier FromHashAlgorithmName(HashAlgorithmName hashAlgorithmName)
        {
            var oid = CryptoConfig.MapNameToOID(hashAlgorithmName.Name);

            if (oid is null)
            {
                throw new ArgumentException($"Could not map `{hashAlgorithmName}` to OID.", nameof(hashAlgorithmName));
            }

            var identifiers = oid.Split('.').Select(ulong.Parse).ToArray();

            return new ObjectIdentifier(identifiers);
        }
    }
}
