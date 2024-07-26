using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;

#endif // NETFRAMEWORK
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when connection was terminated.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif // NETFRAMEWORK
    public class SshConnectionException : SshException
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
            DisconnectReason = DisconnectReason.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="disconnectReasonCode">The disconnect reason code.</param>
        public SshConnectionException(string message, DisconnectReason disconnectReasonCode)
            : base(message)
        {
            DisconnectReason = disconnectReasonCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public SshConnectionException(string message, Exception inner)
            : base(message, inner)
        {
            DisconnectReason = DisconnectReason.None;
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
            DisconnectReason = disconnectReasonCode;
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <see langword="null"/>.</exception>
        /// <exception cref="SerializationException">The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0). </exception>
        protected SshConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif // NETFRAMEWORK
    }
}
