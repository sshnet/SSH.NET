using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// The exception that is thrown when pass phrase for key file is empty or null
    /// </summary>
    [Serializable]
    public class SshPassPhraseNullOrEmptyException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshPassPhraseNullOrEmptyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshPassPhraseNullOrEmptyException(string message)
            : base(message)
        {

        }
    }
}
