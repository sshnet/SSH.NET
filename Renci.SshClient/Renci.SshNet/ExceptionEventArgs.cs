using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshNet.ForwardedPort.Exception"/> event.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
