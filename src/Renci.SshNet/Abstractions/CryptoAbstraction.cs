using System;
#if !NET
using System.Runtime.CompilerServices;
#endif
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace Renci.SshNet.Abstractions
{
    internal static class CryptoAbstraction
    {
        internal static readonly RandomNumberGenerator Randomizer = RandomNumberGenerator.Create();

        internal static readonly SecureRandom SecureRandom = new SecureRandom(new CryptoApiRandomGenerator(Randomizer));

#if !NET
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
#endif
        public static bool FixedTimeEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
#if NET
            return CryptographicOperations.FixedTimeEquals(left, right);
#else
            // https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/CryptographicOperations.cs

            // NoOptimization because we want this method to be exactly as non-short-circuiting
            // as written.
            //
            // NoInlining because the NoOptimization would get lost if the method got inlined.

            if (left.Length != right.Length)
            {
                return false;
            }

            var length = left.Length;
            var accum = 0;

            for (var i = 0; i < length; i++)
            {
                accum |= left[i] - right[i];
            }

            return accum == 0;
#endif
        }
    }
}
