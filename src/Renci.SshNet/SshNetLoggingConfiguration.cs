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

#if DEBUG
        /// <summary>
        /// Gets or sets the path to which to write session secrets which
        /// Wireshark can read and use to inspect encrypted traffic.
        /// </summary>
        /// <remarks>
        /// To configure in Wireshark, go to Edit -> Preferences -> Protocols
        /// -> SSH and set the same value for "Key log filename".
        /// </remarks>
        public static string? WiresharkKeyLogFilePath { get; set; }
#endif
    }
}
