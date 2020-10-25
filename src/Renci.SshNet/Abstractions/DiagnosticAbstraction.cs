using System.Diagnostics;

namespace Renci.SshNet.Abstractions
{
    /// <summary>
    /// Diagnostics for Renci library
    /// </summary>
    public static class DiagnosticAbstraction
    {
#if FEATURE_DIAGNOSTICS_TRACESOURCE

        private static readonly SourceSwitch SourceSwitch = new SourceSwitch("SshNetSwitch");

        /// <summary>
        /// Whether the specified event type is enabled for tracing or not
        /// </summary>
        /// <param name="traceEventType">The trace event type</param>
        /// <returns>true if enabled for tracing, false otherwise</returns>
        public static bool IsEnabled(TraceEventType traceEventType)
        {
            return SourceSwitch.ShouldTrace(traceEventType);
        }

        /// <summary>
        /// The trace source for Renci
        /// </summary>
        public static readonly TraceSource Logging =
#if DEBUG
            new TraceSource(name: "SshNet.Logging", defaultLevel: SourceLevels.All);
#else
            new TraceSource("SshNet.Logging");
#endif // DEBUG
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE

        /// <summary>
        /// Log the provided text
        /// </summary>
        /// <param name="text">The text string to log</param>
        /// <param name="eventType">The trace event type</param>
        /// <param name="id">A numeric identifier for the event.</param>
        public static void Log(string text, TraceEventType eventType = TraceEventType.Verbose, int id = 0)
        {
#if FEATURE_DIAGNOSTICS_TRACESOURCE
            Logging.TraceEvent(eventType, id, text);
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE
        }
    }
}
