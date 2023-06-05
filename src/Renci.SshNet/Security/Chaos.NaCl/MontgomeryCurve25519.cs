using System;
using System.Collections.Generic;
using Renci.SshNet.Security.Chaos.NaCl.Internal;
using Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10;

namespace Renci.SshNet.Security.Chaos.NaCl
{
    // This class is mainly for compatibility with NaCl's Curve25519 implementation
    // If you don't need that compatibility, use Ed25519.KeyExchange
    internal static class MontgomeryCurve25519
    {
        internal static readonly int PublicKeySizeInBytes = 32;
        internal static readonly int PrivateKeySizeInBytes = 32;
    }
}
