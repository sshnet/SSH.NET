#nullable enable
namespace System.Security.Cryptography
{
    internal static class MD5Extensions
    {
        extension(MD5)
        {
#if !NET
            public static byte[] HashData(byte[] source)
            {
                using (var md5 = MD5.Create())
                {
                    return md5.ComputeHash(source);
                }
            }
#endif
        }
    }
}
