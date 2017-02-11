using System;
#if FEATURE_BINARY_SERIALIZATION
using System.Runtime.Serialization;
#endif // FEATURE_BINARY_SERIALIZATION

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when a proxy connection cannot be established.
    /// </summary>
#if FEATURE_BINARY_SERIALIZATION
    [Serializable]
#endif // FEATURE_BINARY_SERIALIZATION
    public class ProxyException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        public ProxyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ProxyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ProxyException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

#if FEATURE_BINARY_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <c>null</c>.</exception>
        /// <exception cref="SerializationException">The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0).</exception>
        protected ProxyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif // FEATURE_BINARY_SERIALIZATION
    }
}
