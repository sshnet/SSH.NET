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
        /// Gets or set the number of seconds to wait for an operation to complete.
        /// </summary>
        /// <value>
        /// The number of seconds to wait for an operation to complete, or -1 to wait indefinitely.
        /// </value>
        int OperationTimeout { get; }

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
        /// <param name="millisecondsTimeout">The number of millieseconds wait for <paramref name="waitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified timeout.</exception>
        void WaitOnHandle(WaitHandle waitHandle, int millisecondsTimeout);

        /// <summary>
        /// Blocks the current thread until the specified <see cref="WaitHandle"/> gets signaled, using a
        /// 32-bit signed integer to specify the time interval in milliseconds.
        /// </summary>
        /// <param name="waitHandle">The handle to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for <paramref name="waitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="waitHandle"/> received a signal within the specified timeout;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <remarks>
        /// The blocking wait is also interrupted when either the established channel is closed, the current
        /// session is disconnected or an unexpected <see cref="Exception"/> occurred while processing a channel
        /// or session event.
        /// </remarks>
        bool WaitOne(WaitHandle waitHandle, int millisecondsTimeout);

        /// <summary>
        /// Blocks the current thread until the specified <see cref="WaitHandle"/> gets signaled, using a
        /// 32-bit signed integer to specify the time interval in milliseconds.
        /// </summary>
        /// <param name="waitHandleA">The first handle to wait for.</param>
        /// <param name="waitHandleB">The second handle to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for a <see cref="WaitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <returns>
        /// <c>0</c> if <paramref name="waitHandleA"/> received a signal within the specified timeout and <c>1</c>
        /// if <paramref name="waitHandleB"/> received a signal within the specified timeout, or <see cref="WaitHandle.WaitTimeout"/>
        /// if no object satisfied the wait.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <remarks>
        /// <para>
        /// The blocking wait is also interrupted when either the established channel is closed, the current
        /// session is disconnected or an unexpected <see cref="Exception"/> occurred while processing a channel
        /// or session event.
        /// </para>
        /// <para>
        /// When both <paramref name="waitHandleA"/> and <paramref name="waitHandleB"/> are signaled during the call,
        /// then <c>0</c> is returned.
        /// </para>
        /// </remarks>
        int WaitAny(WaitHandle waitHandleA, WaitHandle waitHandleB, int millisecondsTimeout);

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a signal, using a 32-bit signed
        /// integer to specify the time interval.
        /// </summary>
        /// <param name="waitHandles">A <see cref="WaitHandle"/> array - constructed using <see cref="CreateWaitHandleArray(WaitHandle[])"/> - containing the objects to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for a <see cref="WaitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <returns>
        /// The array index of the first non-system object that satisfied the wait.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">No object satified the wait and a time interval equivalent to <paramref name="millisecondsTimeout"/> has passed.</exception>
        /// <remarks>
        /// For the return value, the index of the first non-system object is considered to be zero.
        /// </remarks>
        int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout);

        /// <summary>
        /// Creates a <see cref="WaitHandle"/> array that is composed of system objects and the specified
        /// elements.
        /// </summary>
        /// <param name="waitHandles">A <see cref="WaitHandle"/> array containing the objects to wait for.</param>
        /// <returns>
        /// A <see cref="WaitHandle"/> array that is composed of system objects and the specified elements.
        /// </returns>
        WaitHandle[] CreateWaitHandleArray(params WaitHandle[] waitHandles);

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
