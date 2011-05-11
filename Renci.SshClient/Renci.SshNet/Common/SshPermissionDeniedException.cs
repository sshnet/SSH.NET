using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when operation permission is denied.
    /// </summary>
    [Serializable]
    public class SshPermissionDeniedException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshPermissionDeniedException"/> class.
        /// </summary>
        public SshPermissionDeniedException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshPermissionDeniedException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SshPermissionDeniedException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        ///   
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected SshPermissionDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
