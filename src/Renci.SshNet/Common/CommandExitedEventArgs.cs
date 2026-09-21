#nullable enable
using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Class for command exit related events.
    /// </summary>
    public class CommandExitedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExitedEventArgs"/> class.
        /// </summary>
        /// <param name="exitStatus">The exit status.</param>
        /// <param name="exitSignal">The exit signal.</param>
        public CommandExitedEventArgs(int? exitStatus, string? exitSignal)
        {
            ExitStatus = exitStatus;
            ExitSignal = exitSignal;
        }

        /// <summary>
        /// Gets the number representing the exit status of the command, if applicable,
        /// otherwise <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The value is not <see langword="null"/> when an exit status code has been returned
        /// from the server. If the command terminated due to a signal, <see cref="ExitSignal"/>
        /// may be not <see langword="null"/> instead.
        /// </remarks>
        /// <seealso cref="ExitSignal"/>
        public int? ExitStatus { get; }

        /// <summary>
        /// Gets the name of the signal due to which the command
        /// terminated violently, if applicable, otherwise <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The value (if it exists) is supplied by the server and is usually one of the
        /// following, as described in https://datatracker.ietf.org/doc/html/rfc4254#section-6.10:
        /// ABRT, ALRM, FPE, HUP, ILL, INT, KILL, PIPE, QUIT, SEGV, TER, USR1, USR2.
        /// </remarks>
        public string? ExitSignal { get; }
    }
}
