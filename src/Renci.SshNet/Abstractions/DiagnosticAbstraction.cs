using System.Diagnostics;

namespace Renci.SshNet.Abstractions
{
    internal static class DiagnosticAbstraction
    {
#if FEATURE_DIAGNOSTICS_TRACESOURCE
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
            Loggging.TraceEvent(TraceEventType.Verbose, 1, text);
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE
        }
    }
}
