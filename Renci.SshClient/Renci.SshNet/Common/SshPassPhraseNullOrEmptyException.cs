using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when pass phrase for key file is empty or null
    /// </summary>
    public partial class SshPassPhraseNullOrEmptyException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
        /// </summary>
        public SshPassPhraseNullOrEmptyException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshPassPhraseNullOrEmptyException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SshPassPhraseNullOrEmptyException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
