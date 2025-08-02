using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base interface for SSH subsystem implementations.
    /// </summary>
    internal interface ISubsystemSession : IDisposable
    {
        /// <summary>
        /// Gets the logger factory for this subsystem session.
        /// </summary>
        /// <value>
        /// The logger factory for this connection. Will never return <see langword="null"/>.
        /// </value>
        public ILoggerFactory SessionLoggerFactory { get; }

        /// <summary>
        /// Gets or sets the number of milliseconds to wait for an operation to complete.
        /// </summary>
        /// <value>
        /// The number of milliseconds to wait for an operation to complete, or <c>-1</c> to wait indefinitely.
        /// </value>
        int OperationTimeout { get; set; }

        /// <summary>
        /// Gets a value indicating whether this session is open.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this session is open; otherwise, <see langword="false"/>.
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
        /// Waits a specified time for a given <see cref="WaitHandle"/> to be signaled.
        /// </summary>
        /// <param name="waitHandle">The handle to wait for.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for <paramref name="waitHandle"/> to be signaled, or <c>-1</c> to wait indefinitely.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified timeout.</exception>
        void WaitOnHandle(WaitHandle waitHandle, int millisecondsTimeout);

        /// <summary>
        /// Asynchronously waits for a given <see cref="WaitHandle"/> to be signaled.
        /// </summary>
        /// <param name="waitHandle">The handle to wait for.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for <paramref name="waitHandle"/> to be signaled, or <c>-1</c> to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified timeout.</exception>
        /// <returns>A <see cref="Task"/> representing the wait.</returns>
        Task WaitOnHandleAsync(WaitHandle waitHandle, int millisecondsTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously waits for a given <see cref="TaskCompletionSource{T}"/> to complete.
        /// </summary>
        /// <typeparam name="T">The type of the result which is being awaited.</typeparam>
        /// <param name="tcs">The handle to wait for.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for <paramref name="tcs"/> to complete, or <c>-1</c> to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified timeout.</exception>
        /// <returns>A <see cref="Task"/> representing the wait.</returns>
        Task<T> WaitOnHandleAsync<T>(TaskCompletionSource<T> tcs, int millisecondsTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a signal, using a 32-bit signed
        /// integer to specify the time interval.
        /// </summary>
        /// <param name="waitHandles">A <see cref="WaitHandle"/> array - constructed using <see cref="CreateWaitHandleArray"/> - containing the objects to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for a <see cref="WaitHandle"/> to get signaled, or <c>-1</c> to wait indefinitely.</param>
        /// <returns>
        /// The array index of the first non-system object that satisfied the wait.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">No object satisfied the wait and a time interval equivalent to <paramref name="millisecondsTimeout"/> has passed.</exception>
        /// <remarks>
        /// For the return value, the index of the first non-system object is considered to be zero.
        /// </remarks>
        int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout);

        /// <summary>
        /// Creates a <see cref="WaitHandle"/> array that is composed of system objects and the specified
        /// elements.
        /// </summary>
        /// <param name="waitHandle1">The first <see cref="WaitHandle"/> to wait for.</param>
        /// <param name="waitHandle2">The second <see cref="WaitHandle"/> to wait for.</param>
        /// <returns>
        /// A <see cref="WaitHandle"/> array that is composed of system objects and the specified elements.
        /// </returns>
        WaitHandle[] CreateWaitHandleArray(WaitHandle waitHandle1, WaitHandle waitHandle2);
    }
}
