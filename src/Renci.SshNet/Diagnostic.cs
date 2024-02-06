using System;
using System.Diagnostics;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides access to the <see cref="System.Diagnostics"/> internals of SSH.NET.
    /// </summary>
    public static class Diagnostic
    {
        /// <summary>
        /// The <see cref="TraceSource"/> instance used by SSH.NET.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Configuration on .NET Core must be done programmatically, e.g.
        /// <code>
        /// Diagnostics.Source.Switch = new SourceSwitch("sourceSwitch", nameof(SourceLevels.Verbose));
        /// Diagnostics.Source.Listeners.Remove("Default");
        /// Diagnostics.Source.Listeners.Add(new ConsoleTraceListener());
        /// Diagnostics.Source.Listeners.Add(new TextWriterTraceListener("SshNetTrace.log"));
        /// </code>
        /// </para>
        /// <para>
        /// On .NET Framework, it is possible to configure via App.config, e.g.
        /// <code>
        /// <![CDATA[
        /// <configuration>
        ///     <system.diagnostics>
        ///         <trace autoflush="true"/>
        ///         <sources>
        ///             <source name="SshNet.Logging" switchValue="Verbose">
        ///                 <listeners>
        ///                     <remove name="Default" />
        ///                     <add name="console"
        ///                          type="System.Diagnostics.ConsoleTraceListener" />
        ///                     <add name="logFile"
        ///                          type="System.Diagnostics.TextWriterTraceListener"
        ///                          initializeData="SshNetTrace.log" />
        ///                 </listeners>
        ///             </source>
        ///         </sources>
        ///     </system.diagnostics>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </para>
        /// </remarks>
        public static readonly TraceSource Source = new TraceSource("SshNet.Logging");

        /// <summary>
        /// Logs a message to <see cref="Source"/> with the specified event type.
        /// </summary>
        /// <param name="text">The message to log.</param>
        /// <param name="eventType">The trace event type.</param>
        public static void Log(string text, TraceEventType eventType)
        {
            Source.TraceEvent(eventType, Environment.CurrentManagedThreadId, text);
        }

        /// <inheritdoc cref="SourceSwitch.ShouldTrace(TraceEventType)" />
        public static bool IsEnabled(TraceEventType eventType)
        {
            return Source.Switch.ShouldTrace(eventType);
        }
    }
}
