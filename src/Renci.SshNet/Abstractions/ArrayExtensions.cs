#nullable enable
#if !NET

namespace System
{
    internal static class ArrayExtensions
    {
        extension(Array)
        {
            internal static int MaxLength
            {
                get { return 0X7FFFFFC7; }
            }
        }
    }
}
#endif
