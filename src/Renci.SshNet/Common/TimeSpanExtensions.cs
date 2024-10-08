#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides extension methods for <see cref="TimeSpan"/>.
    /// </summary>
    internal static class TimeSpanExtensions
    {
        /// <summary>
        /// Returns the specified <paramref name="timeSpan"/> as a valid timeout in milliseconds.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to ensure validity.</param>
        /// <param name="paramName">The name of the calling member.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="timeSpan"/> does not represent a value between -1 and <see cref="int.MaxValue"/>, inclusive.
        /// </exception>
        public static int AsTimeout(this TimeSpan timeSpan, [CallerArgumentExpression(nameof(timeSpan))] string? paramName = null)
        {
            var timeoutInMilliseconds = timeSpan.TotalMilliseconds;
            return timeoutInMilliseconds is < -1d or > int.MaxValue
                       ? throw new ArgumentOutOfRangeException(paramName, "The timeout must represent a value between -1 and Int32.MaxValue milliseconds, inclusive.")
                       : (int)timeoutInMilliseconds;
        }

        /// <summary>
        /// Ensures that the specified <paramref name="timeSpan"/> represents a valid timeout in milliseconds.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to ensure validity.</param>
        /// <param name="paramName">The name of the calling member.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="timeSpan"/> does not represent a value between -1 and <see cref="int.MaxValue"/>, inclusive.
        /// </exception>
        public static void EnsureValidTimeout(this TimeSpan timeSpan, [CallerArgumentExpression(nameof(timeSpan))] string? paramName = null)
        {
            _ = timeSpan.AsTimeout(paramName);
        }
    }
}
