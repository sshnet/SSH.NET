#nullable enable
using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when a proxy connection cannot be established.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class ProxyException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyException"/> class.
        /// </summary>
        public ProxyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string)" path="/param"/>
        public ProxyException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
        public ProxyException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected ProxyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
