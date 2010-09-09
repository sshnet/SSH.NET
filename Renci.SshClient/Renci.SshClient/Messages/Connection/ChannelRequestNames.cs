namespace Renci.SshClient.Messages.Connection
{
    internal enum ChannelRequestNames
    {
        /// <summary>
        /// pty-req
        /// </summary>
        PseudoTerminal,
        /// <summary>
        /// x11-req
        /// </summary>
        X11Forwarding,
        /// <summary>
        /// env
        /// </summary>
        EnvironmentVariable,
        /// <summary>
        /// shell
        /// </summary>
        Shell,
        /// <summary>
        /// exec
        /// </summary>
        Exec,
        /// <summary>
        /// subsystem
        /// </summary>
        Subsystem,
        /// <summary>
        /// window-change
        /// </summary>
        WindowChange,
        /// <summary>
        /// xon-xoff
        /// </summary>
        XonXoff,
        /// <summary>
        /// signal
        /// </summary>
        Signal,
        /// <summary>
        /// exit-status
        /// </summary>
        ExitStatus,
        /// <summary>
        /// exit-signal
        /// </summary>
        ExitSignal
    }
}
