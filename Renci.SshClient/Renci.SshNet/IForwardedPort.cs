using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Supports port forwarding functionality.
    /// </summary>
    internal interface IForwardedPort
    {
        /// <summary>
        /// The <see cref="Closing"/> event occurs as the forward port is being stopped.
        /// </summary>
        event EventHandler Closing;
    }
}
