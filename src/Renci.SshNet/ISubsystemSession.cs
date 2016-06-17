using System;
using System.Threading;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base interface for SSH subsystem implementations.
    /// </summary>
    internal interface ISubsystemSession : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this session is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this session is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Connects the subsystem using a new SSH channel session.
        /// </summary>
        /// <exception cref="InvalidOperationException">The session is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the session was disposed.</exception>
        void Connect();

        /// <summary>
        /// Disconnects the subsystem channel.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Waits a specified time for a given <see cref="WaitHandle"/> to get signaled.
        /// </summary>
        /// <param name="waitHandle">The handle to wait for.</param>
        /// <param name="operationTimeout">The time to wait for <paramref name="waitHandle"/> to get signaled.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified <paramref name="operationTimeout"/>.</exception>
        void WaitOnHandle(WaitHandle waitHandle, TimeSpan operationTimeout);
    }
}
