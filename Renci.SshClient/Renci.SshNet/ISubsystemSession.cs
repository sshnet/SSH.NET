using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet
{
    /// <summary>
    /// Base interface for SSH subsystem implementations.
    /// </summary>
    internal interface ISubsystemSession
    {
        /// <summary>
        /// Gets a value indicating whether this session is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this session is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }
    }
}
