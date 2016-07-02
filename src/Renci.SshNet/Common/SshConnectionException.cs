using System;
#if FEATURE_BINARY_SERIALIZATION
using System.Runtime.Serialization;
#endif // FEATURE_BINARY_SERIALIZATION
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when connection was terminated.
    /// </summary>
#if FEATURE_BINARY_SERIALIZATION
    [Serializable]
#endif // FEATURE_BINARY_SERIALIZATION
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
        /// <param name="disconnectReasonCode">The disconnect reason code.</param>
        /// <param name="inner">The inner.</param>
        public SshConnectionException(string message, DisconnectReason disconnectReasonCode, Exception inner)
            : base(message, inner)
        {
            DisconnectReason = disconnectReasonCode;
        }

#if FEATURE_BINARY_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <c>null</c>.</exception>
        /// <exception cref="SerializationException">The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0). </exception>
        protected SshConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif // FEATURE_BINARY_SERIALIZATION
    }
}
