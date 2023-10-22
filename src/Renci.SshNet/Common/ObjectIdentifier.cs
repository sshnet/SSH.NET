using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Describes object identifier for DER encoding.
    /// </summary>
    public readonly struct ObjectIdentifier : IEquatable<ObjectIdentifier>
    {
        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        public ulong[] Identifiers { get; }

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

        /// <summary>
        /// Determines whether this instance and a specified object, which must also be a
        /// <see cref="ObjectIdentifier"/> object, have the same value.
        /// </summary>
        /// <param name="obj">The <see cref="ObjectIdentifier"/> to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an <see cref="ObjectIdentifier"/>
        /// and its <see cref="Identifiers"/> equal the <see cref="Identifiers"/> of this instance;
        /// otherwise, <see langword="false"/>. If <paramref name="obj"/> is <see langword="null"/>,
        /// the method returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is ObjectIdentifier identifier && Equals(identifier);
        }

        /// <summary>
        /// Determines whether this instance and the specified <see cref="ObjectIdentifier"/>
        /// have the same value.
        /// </summary>
        /// <param name="other">The <see cref="ObjectIdentifier"/> to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Identifiers"/> of <paramref name="other"/> and
        /// this are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(ObjectIdentifier other)
        {
            return EqualityComparer<ulong[]>.Default.Equals(Identifiers, other.Identifiers);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="ObjectIdentifier"/>.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
#if NET || NETSTANDARD2_1_OR_GREATER
            return HashCode.Combine(Identifiers);
#else
            return 1040100207 + EqualityComparer<ulong[]>.Default.GetHashCode(Identifiers);
#endif // NET || NETSTANDARD2_1_OR_GREATER
        }

        /// <summary>
        /// Determines whether <see cref="Identifiers"/> of the specified <see cref="ObjectIdentifier"/>
        /// instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ObjectIdentifier"/> to compare or <see langword="null"/>.</param>
        /// <param name="right">The second <see cref="ObjectIdentifier"/> to compare or <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Identifiers"/> of <paramref name="left"/> and
        /// <paramref name="right"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ObjectIdentifier left, ObjectIdentifier right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether <see cref="Identifiers"/> of the specified <see cref="ObjectIdentifier"/>
        /// instances are different.
        /// </summary>
        /// <param name="left">The first <see cref="ObjectIdentifier"/> to compare or <see langword="null"/>.</param>
        /// <param name="right">The second <see cref="ObjectIdentifier"/> to compare or <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Identifiers"/> of <paramref name="left"/> and
        /// <paramref name="right"/> are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(ObjectIdentifier left, ObjectIdentifier right)
        {
            return !(left == right);
        }
    }
}
