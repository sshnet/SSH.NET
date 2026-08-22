namespace Renci.SshNet
{
    /// <summary>
    /// The ssh compatible standard POSIX/ANSI signals.
    /// </summary>
    public static class CommandSignals
    {
        /// <summary>
        /// Hangup (POSIX).
        /// </summary>
        public const string SIGHUP = "HUP";

        /// <summary>
        /// Interrupt (ANSI).
        /// </summary>
        public const string SIGINT = "INT";

        /// <summary>
        /// Quit (POSIX).
        /// </summary>
        public const string SIGQUIT = "QUIT";

        /// <summary>
        /// Illegal instruction (ANSI).
        /// </summary>
        public const string SIGILL = "ILL";

        /// <summary>
        /// Abort (ANSI).
        /// </summary>
        public const string SIGABRT = "ABRT";

        /// <summary>
        /// Floating-point exception (ANSI).
        /// </summary>
        public const string SIGFPE = "FPE";

        /// <summary>
        /// Kill, unblockable (POSIX).
        /// </summary>
        public const string SIGKILL = "KILL";

        /// <summary>
        /// User-defined signal 1 (POSIX).
        /// </summary>
        public const string SIGUSR1 = "USR1";

        /// <summary>
        /// Segmentation violation (ANSI).
        /// </summary>
        public const string SIGSEGV = "SEGV";

        /// <summary>
        /// User-defined signal 2 (POSIX).
        /// </summary>
        public const string SIGUSR2 = "USR2";

        /// <summary>
        /// Broken pipe (POSIX).
        /// </summary>
        public const string SIGPIPE = "PIPE";

        /// <summary>
        /// Alarm clock (POSIX).
        /// </summary>
        public const string SIGALRM = "ALRM";

        /// <summary>
        /// Termination (ANSI).
        /// </summary>
        public const string SIGTERM = "TERM";
    }
}
