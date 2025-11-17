#nullable enable
#if !NET
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System
{
    internal static class ThrowExtensions
    {
        extension(ObjectDisposedException)
        {
            public static void ThrowIf(bool condition, object instance)
            {
                if (condition)
                {
                    Throw(instance);

                    static void Throw(object? instance)
                    {
                        throw new ObjectDisposedException(instance?.GetType().FullName);
                    }
                }
            }
        }

        extension(ArgumentNullException)
        {
            public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
                if (argument is null)
                {
                    ThrowArgumentNullException(paramName);
                }

                [DoesNotReturn]
                static void ThrowArgumentNullException(string? paramName)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
        }

        extension(ArgumentException)
        {
            public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
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
            }

            public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            {
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
            }
        }

        extension(ArgumentOutOfRangeException)
        {
            public static void ThrowIfNegative(long value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            {
                if (value < 0)
                {
                    Throw(value, paramName);

                    [DoesNotReturn]
                    static void Throw(long value, string? paramName)
                    {
                        throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
                    }
                }
            }
        }
    }
}
#endif
