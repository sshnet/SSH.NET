using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Security;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents private key information from various sources.
    /// </summary>
    public interface IPrivateKeySource
    {
        /// <summary>
        /// Stores the host key.
        /// </summary>
        HostAlgorithm HostKey { get; }
    }
}
