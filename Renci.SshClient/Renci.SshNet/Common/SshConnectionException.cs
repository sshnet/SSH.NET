using System;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when connection was terminated.
    /// </summary>
    public partial class SshConnectionException : SshException
    {
        /// <summary>
        /// Gets the disconnect reason if provided by the server or client. Otherwise None.
        /// </summary>
        public DisconnectReason DisconnectReason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        public SshConnectionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshConnectionException(string message)
            : base(message)
        {
            this.DisconnectReason = DisconnectReason.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="disconnectReasonCode">The disconnect reason code.</param>
        public SshConnectionException(string message, DisconnectReason disconnectReasonCode)
            : base(message)
        {
            this.DisconnectReason = disconnectReasonCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="disconnectReasonCode">The disconnect reason code.</param>
        /// <param name="inner">The inner.</param>
        public SshConnectionException(string message, DisconnectReason disconnectReasonCode, Exception inner)
            : base(message, inner)
        {
            this.DisconnectReason = disconnectReasonCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SshConnectionException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
