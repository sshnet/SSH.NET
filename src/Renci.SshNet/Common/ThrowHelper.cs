#nullable enable
#if !NET
using System;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet.Common
{
    internal static class ThrowHelper
    {
        // A rough copy of
        // https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/IO/Stream.cs#L960C13-L974C10
        // for lower targets.
        public static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            if ((uint)count > buffer.Length - offset)
            {
                Throw();

                [DoesNotReturn]
                static void Throw()
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(count),
                        "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
                }
            }
        }
    }
}
#endif
