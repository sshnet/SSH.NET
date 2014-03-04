using System;
using System.Runtime.Serialization;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when there is something wrong with the server capabilities.
    /// </summary>
    [Serializable]
    public partial class NetConfServerException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshAuthenticationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected NetConfServerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
