#nullable enable
namespace System.Security.Cryptography
{
    internal static class SHA256Extensions
    {
        extension(SHA256)
        {
#if !NET
            public static byte[] HashData(byte[] source)
            {
                using (var sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(source);
                }
            }
#endif
        }
    }
}
