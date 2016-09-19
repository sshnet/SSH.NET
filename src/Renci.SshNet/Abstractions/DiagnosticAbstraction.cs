using System.Diagnostics;
#if FEATURE_DIAGNOSTICS_TRACESOURCE
using System.Threading;
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE

namespace Renci.SshNet.Abstractions
{
    internal static class DiagnosticAbstraction
    {
#if FEATURE_DIAGNOSTICS_TRACESOURCE

        private static readonly SourceSwitch SourceSwitch = new SourceSwitch("SshNetSwitch");

        public static bool IsEnabled(TraceEventType traceEventType)
        {
            return SourceSwitch.ShouldTrace(traceEventType);
        }

        private static readonly TraceSource Loggging =
#if DEBUG
            new TraceSource("SshNet.Logging", SourceLevels.All);
#else
            new TraceSource("SshNet.Logging");
#endif // DEBUG
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE

        [Conditional("DEBUG")]
        public static void Log(string text)
        {
#if FEATURE_DIAGNOSTICS_TRACESOURCE
            Loggging.TraceEvent(TraceEventType.Verbose, Thread.CurrentThread.ManagedThreadId, text);
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE
        }
    }
}
