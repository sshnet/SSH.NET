#nullable enable
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Allows configuring the logging for internal logs of SSH.NET.
    /// </summary>
    public static class SshNetLoggingConfiguration
    {
        internal static ILoggerFactory LoggerFactory { get; private set; } = NullLoggerFactory.Instance;

        /// <summary>
        /// Initializes the logging for SSH.NET.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public static void InitializeLogging(ILoggerFactory loggerFactory)
        {
            ThrowHelper.ThrowIfNull(loggerFactory);
            LoggerFactory = loggerFactory;
        }
    }
}
