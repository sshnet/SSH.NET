using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
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
        /// <param name="message">The message.</param>
        public SshPermissionDeniedException(string message)
            : base(message)
        {

        }
    }
}
