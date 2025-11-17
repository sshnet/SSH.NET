namespace System.Security.Cryptography
{
    internal static class SHA1Extensions
    {
        extension(SHA1)
        {
#if !NET
            public static byte[] HashData(byte[] source)
            {
                using (var sha1 = SHA1.Create())
                {
                    return sha1.ComputeHash(source);
                }
            }
#endif
        }
    }
}
