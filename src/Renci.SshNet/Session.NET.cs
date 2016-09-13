using System;
using System.Net.Sockets;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet
{
    public partial class Session
    {
#if FEATURE_SOCKET_POLL
        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the socket is connected; otherwise, <c>false</c></param>
        /// <remarks>
        /// <para>
        /// As a first check we verify whether <see cref="Socket.Connected"/> is
        /// <c>true</c>. However, this only returns the state of the socket as of
        /// the last I/O operation.
        /// </para>
        /// <para>
        /// Therefore we use the combination of <see cref="Socket.Poll(int, SelectMode)"/> with mode <see cref="SelectMode.SelectRead"/>
        /// and <see cref="Socket.Available"/> to verify if the socket is still connected.
        /// </para>
        /// <para>
        /// The MSDN doc mention the following on the return value of <see cref="Socket.Poll(int, SelectMode)"/>
        /// with mode <see cref="SelectMode.SelectRead"/>:
        /// <list type="bullet">
        ///     <item>
        ///         <description><c>true</c> if data is available for reading;</description>
        ///     </item>
        ///     <item>
        ///         <description><c>true</c> if the connection has been closed, reset, or terminated; otherwise, returns <c>false</c>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// <c>Conclusion:</c> when the return value is <c>true</c> - but no data is available for reading - then
        /// the socket is no longer connected.
        /// </para>
        /// <para>
        /// When a <see cref="Socket"/> is used from multiple threads, there's a race condition
        /// between the invocation of <see cref="Socket.Poll(int, SelectMode)"/> and the moment
        /// when the value of <see cref="Socket.Available"/> is obtained. To workaround this issue
        /// we synchronize reads from the <see cref="Socket"/>.
        /// </para>
        /// </remarks>
#else
        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the socket is connected; otherwise, <c>false</c></param>
        /// <remarks>
        /// We verify whether <see cref="Socket.Connected"/> is <c>true</c>. However, this only returns the state
        /// of the socket as of the last I/O operation.
        /// </remarks>
#endif
        partial void IsSocketConnected(ref bool isConnected)
        {
            DiagnosticAbstraction.Log(string.Format("[{0}] {1} Checking socket", ToHex(SessionId), DateTime.Now.Ticks));

            lock (_socketDisposeLock)
            {
#if FEATURE_SOCKET_POLL
                if (!_socket.IsConnected())
                {
                    isConnected = false;
                    return;
                }

                lock (_socketReadLock)
                {
                    var connectionClosedOrDataAvailable = _socket.Poll(0, SelectMode.SelectRead);
                    isConnected = !(connectionClosedOrDataAvailable && _socket.Available == 0);
                }
#else
                isConnected = _socket.IsConnected();
#endif // FEATURE_SOCKET_POLL
            }

            DiagnosticAbstraction.Log(string.Format("[{0}] {1} Checked socket", ToHex(SessionId), DateTime.Now.Ticks));
        }
    }
}
