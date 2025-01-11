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
#if NET7_0_OR_GREATER
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
#if NET6_0_OR_GREATER
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
#if NET8_0_OR_GREATER
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
#if NET7_0_OR_GREATER
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
    }
}
