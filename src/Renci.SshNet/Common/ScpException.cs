#nullable enable
using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when an SCP error occurs.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class ScpException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        public ScpException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string)" path="/param"/>
        public ScpException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
        public ScpException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected ScpException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
