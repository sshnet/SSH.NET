using System;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// The exception that is thrown when connection was terminated.
    /// </summary>
    [Serializable]
    public class SshConnectionException : SshException
    {
        /// <summary>
        /// Gets the disconnect reason if provided by the server or client. Otherwise None.
        /// </summary>
        public DisconnectReasons DisconnectReason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshConnectionException(string message)
            : base(message)
        {
            this.DisconnectReason = DisconnectReasons.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshConnectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="disconnectReasonCode">The disconnect reason code.</param>
        public SshConnectionException(string message, DisconnectReasons disconnectReasonCode)
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
        public SshConnectionException(string message, DisconnectReasons disconnectReasonCode, Exception inner)
            : base(message, inner)
        {
            this.DisconnectReason = disconnectReasonCode;
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
