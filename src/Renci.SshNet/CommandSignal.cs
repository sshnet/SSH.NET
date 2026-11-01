namespace Renci.SshNet
{
    /// <summary>
    /// The ssh compatible POSIX/ANSI signals with their libc compatible values.
    /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
    public enum CommandSignal
    {
        /// <summary>
        /// Hangup (POSIX).
        /// </summary>
        HUP = 1,

        /// <summary>
        /// Interrupt (ANSI).
        /// </summary>
        INT = 2,

        /// <summary>
        /// Quit (POSIX).
        /// </summary>
        QUIT = 3,

        /// <summary>
        /// Illegal instruction (ANSI).
        /// </summary>
        ILL = 4,

        /// <summary>
        /// Abort (ANSI).
        /// </summary>
        ABRT = 6,

        /// <summary>
        /// Floating-point exception (ANSI).
        /// </summary>
        FPE = 8,

        /// <summary>
        /// Kill, unblockable (POSIX).
        /// </summary>
        KILL = 9,

        /// <summary>
        /// User-defined signal 1 (POSIX).
        /// </summary>
        USR1 = 10,

        /// <summary>
        /// Segmentation violation (ANSI).
        /// </summary>
        SEGV = 11,

        /// <summary>
        /// User-defined signal 2 (POSIX).
        /// </summary>
        USR2 = 12,

        /// <summary>
        /// Broken pipe (POSIX).
        /// </summary>
        PIPE = 13,

        /// <summary>
        /// Alarm clock (POSIX).
        /// </summary>
        ALRM = 14,

        /// <summary>
        /// Termination (ANSI).
        /// </summary>
        TERM = 15,
    }
}
