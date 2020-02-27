using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if FEATURE_BINARY_SERIALIZATION
using System.Runtime.Serialization;
#endif // FEATURE_BINARY_SERIALIZATION

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when SSH exception occurs.
    /// </summary>
#if FEATURE_BINARY_SERIALIZATION
    [Serializable]
#endif // FEATURE_BINARY_SERIALIZATION
    public class RevokedKeyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RevokedKeyException"/> class.
        /// </summary>
        public RevokedKeyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RevokedKeyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public RevokedKeyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RevokedKeyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public RevokedKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }

#if FEATURE_BINARY_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="RevokedKeyException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <c>null</c>.</exception>
        /// <exception cref="SerializationException">The class name is <c>null</c> or <see cref="Exception.HResult"/> is zero (0). </exception>
        protected RevokedKeyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif // FEATURE_BINARY_SERIALIZATION
    }
}
