using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents possible authentication methods results
    /// </summary>
    public enum AuthenticationResult
    {
        /// <summary>
        /// Authentication was successful.
        /// </summary>
        Success,
        /// <summary>
        /// Authentication completed with partial success.
        /// </summary>
        PartialSuccess,
        /// <summary>
        /// Authentication failed.
        /// </summary>
        Failure
    }
}
