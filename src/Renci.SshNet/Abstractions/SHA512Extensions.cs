#nullable enable
namespace System.Security.Cryptography
{
    internal static class SHA512Extensions
    {
        extension(SHA512)
        {
#if !NET
            public static byte[] HashData(byte[] source)
            {
                using (var sha512 = SHA512.Create())
                {
                    return sha512.ComputeHash(source);
                }
            }
#endif
        }
    }
}
