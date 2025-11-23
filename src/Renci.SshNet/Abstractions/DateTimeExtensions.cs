#nullable enable
namespace System
{
    internal static class DateTimeExtensions
    {
        extension(DateTime)
        {
#if !NET
            public static DateTime UnixEpoch
            {
                get
                {
                    return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                }
            }
#endif
        }
    }
}
