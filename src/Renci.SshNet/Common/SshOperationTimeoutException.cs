using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif // NETFRAMEWORK

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when operation is timed out.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif // NETFRAMEWORK
    public class SshOperationTimeoutException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
        /// </summary>
        public SshOperationTimeoutException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshOperationTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SshOperationTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <see langword="null"/>.</exception>
        /// <exception cref="SerializationException">The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0). </exception>
        protected SshOperationTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif // NETFRAMEWORK
    }
}
