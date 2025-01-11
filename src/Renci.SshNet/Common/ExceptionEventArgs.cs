using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for the ErrorOccured events.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">An System.Exception that represents the error that occurred.</param>
        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="Exception"/> that represents the error that occurred.
        /// </summary>
        /// <value>
        /// The <see cref="Exception"/> that represents the error that occurred.
        /// </value>
        public Exception Exception { get; }
    }
}
