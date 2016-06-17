using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Supports port forwarding functionality.
    /// </summary>
    public interface IForwardedPort
    {
        /// <summary>
        /// The <see cref="Closing"/> event occurs as the forwarded port is being stopped.
        /// </summary>
        event EventHandler Closing;
    }
}
