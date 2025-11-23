#nullable enable
using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when an SSH exception occurs.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class SshException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshException"/> class.
        /// </summary>
        public SshException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
        public SshException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
        public SshException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected SshException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
