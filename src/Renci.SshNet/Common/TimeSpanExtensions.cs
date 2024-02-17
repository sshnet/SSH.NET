using System;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

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
        /// <param name="callerMemberName">The name of the calling member.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="timeSpan"/> does not represent a value between -1 and <see cref="int.MaxValue"/>, inclusive.
        /// </exception>
        public static int AsTimeout(this TimeSpan timeSpan,
#if NETCOREAPP3_0_OR_GREATER
                                    [CallerArgumentExpression(nameof(timeSpan))]
#endif
                                    string callerMemberName = "")
        {
            var timeoutInMilliseconds = timeSpan.TotalMilliseconds;
            return timeoutInMilliseconds is < -1d or > int.MaxValue
                       ? throw new ArgumentOutOfRangeException(callerMemberName, "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.")
                       : (int) timeoutInMilliseconds;
        }

        /// <summary>
        /// Ensures that the specified <paramref name="timeSpan"/> represents a valid timeout in milliseconds.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to ensure validity.</param>
        /// <param name="callerMemberName">The name of the calling member.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="timeSpan"/> does not represent a value between -1 and <see cref="int.MaxValue"/>, inclusive.
        /// </exception>
        public static void EnsureValidTimeout(this TimeSpan timeSpan,
#if NETCOREAPP3_0_OR_GREATER
                                    [CallerArgumentExpression(nameof(timeSpan))]
#endif
                                    string callerMemberName = "")
        {
            var timeoutInMilliseconds = timeSpan.TotalMilliseconds;
            if (timeoutInMilliseconds is < -1d or > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(callerMemberName, "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");
            }
        }
    }
}
