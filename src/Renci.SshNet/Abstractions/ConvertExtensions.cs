#nullable enable
#if !NET
using System.Text;
#endif

namespace System
{
    internal static class ConvertExtensions
    {
        extension(Convert)
        {
#if !NET
            public static byte[] FromHexString(string s)
            {
                return Org.BouncyCastle.Utilities.Encoders.Hex.Decode(s);
            }

            public static string ToHexString(byte[] inArray)
            {
                ArgumentNullException.ThrowIfNull(inArray);

                var builder = new StringBuilder(inArray.Length * 2);

                foreach (var b in inArray)
                {
                    builder.Append(b.ToString("X2"));
                }

                return builder.ToString();
            }
#endif
        }
    }
}
