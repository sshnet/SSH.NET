using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// The exception that is thrown when file or directory is not found.
    /// </summary>
    [Serializable]
    public class SshFileNotFoundException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshFileNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshFileNotFoundException(string message)
            : base(message)
        {

        }
    }
}
