﻿using System.Diagnostics;

namespace Renci.SshNet.Abstractions
{
    internal static class DiagnosticAbstraction
    {
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

        [Conditional("DEBUG")]
        public static void Log(string text)
        {
            Loggging.TraceEvent(TraceEventType.Verbose,
#if NET6_0_OR_GREATER
                                System.Environment.CurrentManagedThreadId,
#else
                                System.Threading.Thread.CurrentThread.ManagedThreadId,
#endif // NET6_0_OR_GREATER
                                text);
        }
    }
}
