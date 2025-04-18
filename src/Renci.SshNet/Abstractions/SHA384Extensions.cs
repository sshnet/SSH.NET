#nullable enable
namespace System.Security.Cryptography
{
    internal static class SHA384Extensions
    {
        extension(SHA384)
        {
#if !NET
            public static byte[] HashData(byte[] source)
            {
                using (var sha384 = SHA384.Create())
                {
                    return sha384.ComputeHash(source);
                }
            }
#endif
        }
    }
}
