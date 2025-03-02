#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Renci.SshNet.Common
{
    internal static class ThrowHelper
    {
        public static void ThrowObjectDisposedIf(bool condition, object instance)
        {
#if NET
            ObjectDisposedException.ThrowIf(condition, instance);
#else
            if (condition)
            {
                Throw(instance);

                static void Throw(object? instance)
                {
                    throw new ObjectDisposedException(instance?.GetType().FullName);
                }
            }
#endif
        }

        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if NET
            ArgumentNullException.ThrowIfNull(argument, paramName);
#else
            if (argument is null)
            {
                Throw(paramName);

                [DoesNotReturn]
                static void Throw(string? paramName)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
#endif
        }

        public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if NET
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
#else
            if (string.IsNullOrWhiteSpace(argument))
            {
                Throw(argument, paramName);

                [DoesNotReturn]
                static void Throw(string? argument, string? paramName)
                {
                    ThrowIfNull(argument, paramName);
                    throw new ArgumentException("The value cannot be an empty string or composed entirely of whitespace.", paramName);
                }
            }
#endif
        }

        public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if NET
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
#else
            if (string.IsNullOrEmpty(argument))
            {
                Throw(argument, paramName);

                [DoesNotReturn]
                static void Throw(string? argument, string? paramName)
                {
                    ThrowIfNull(argument, paramName);
                    throw new ArgumentException("The value cannot be an empty string.", paramName);
                }
            }
#endif
        }

#if !NET
        // A rough copy of
        // https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/IO/Stream.cs#L960C13-L974C10
        // for lower targets.
        public static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            ThrowIfNull(buffer);
            ThrowIfNegative(offset);

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
#endif

        public static void ThrowIfNegative(long value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
#if NET
            ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
#else
            if (value < 0)
            {
                Throw(value, paramName);

                [DoesNotReturn]
                static void Throw(long value, string? paramName)
                {
                    throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
                }
            }
#endif
        }
    }
}
