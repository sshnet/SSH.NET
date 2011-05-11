using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// The exception that is thrown when authentication failed.
    /// </summary>
    [Serializable]
    public class SshAuthenticationException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshAuthenticationException(string message)
            : base(message)
        {

        }
    }
}
