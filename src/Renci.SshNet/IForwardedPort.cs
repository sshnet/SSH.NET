using System;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Supports port forwarding functionality.
    /// </summary>
    public interface IForwardedPort : IDisposable
    {
        /// <summary>
        /// The <see cref="Closing"/> event occurs as the forwarded port is being stopped.
        /// </summary>
        event EventHandler Closing;

        /// <summary>
        /// Occurs when an exception is thrown.
        /// </summary>
        event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Occurs when a port forwarding request is received.
        /// </summary>
        event EventHandler<PortForwardEventArgs> RequestReceived;

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if port forwarding is started; otherwise, <see langword="false"/>.
        /// </value>
        bool IsStarted { get; }

        /// <summary>
        /// Starts port forwarding.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops port forwarding.
        /// </summary>
        void Stop();
    }
}
