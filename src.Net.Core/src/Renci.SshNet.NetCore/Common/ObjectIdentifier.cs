using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Describes object identifier for DER encoding
    /// </summary>
    public struct ObjectIdentifier
    {
        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        public ulong[] Identifiers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifier"/> class.
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        public ObjectIdentifier(params ulong[] identifiers)
            : this()
        {
            if (identifiers.Length < 2)
                throw new ArgumentException("identifiers");

            Identifiers = identifiers;
        }
    }
}
