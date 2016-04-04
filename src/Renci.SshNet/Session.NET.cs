using System.Net.Sockets;
using System.Diagnostics;

namespace Renci.SshNet
{
    public partial class Session
    {
#if FEATURE_DIAGNOSTICS_TRACESOURCE
        private readonly TraceSource _log =
#if DEBUG
            new TraceSource("SshNet.Logging", SourceLevels.All);
#else
            new TraceSource("SshNet.Logging");
#endif // DEBUG
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE

        /// <summary>
        /// Holds the lock object to ensure read access to the socket is synchronized.
        /// </summary>
        private readonly object _socketReadLock = new object();

        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the socket is connected; otherwise, <c>false</c></param>
        /// <remarks>
        /// <para>
        /// As a first check we verify whether <see cref="Socket.Connected"/> is
        /// <c>true</c>. However, this only returns the state of the socket as of
        /// the last I/O operation. Therefore we use the combination of Socket.Poll
        /// with mode SelectRead and Socket.Available to verify if the socket is
        /// still connected.
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
        /// when the value of <see cref="Socket.Available"/> is obtained. As a workaround, we signal
        /// when bytes are read from the <see cref="Socket"/>.
        /// </para>
        /// </remarks>
        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (_socket != null && _socket.Connected);
            if (isConnected)
            {
                // synchronize this to ensure thread B does not reset the wait handle before
                // thread A was able to check whether "bytes read from socket" signal was
                // actually received
                lock (_socketReadLock)
                {
                    _bytesReadFromSocket.Reset();
                    var connectionClosedOrDataAvailable = _socket.Poll(1000, SelectMode.SelectRead);
                    isConnected = !(connectionClosedOrDataAvailable && _socket.Available == 0);
                    if (!isConnected)
                    {
                        // the race condition is between the Socket.Poll call and
                        // Socket.Available, but the event handler - where we signal that
                        // bytes have been received from the socket - is sometimes invoked
                        // shortly after
                        isConnected = _bytesReadFromSocket.WaitOne(500);
                    }
                }
            }
        }

        /// <summary>
        /// Closes the socket and allows the socket to be reused after the current connection is closed.
        /// </summary>
        /// <exception cref="SocketException">An error occurred when trying to access the socket.</exception>
        partial void SocketDisconnect()
        {
            // TODO should disconnect instead ?!!
            _socket.Dispose();
        }

        [Conditional("DEBUG")]
        partial void Log(string text)
        {
#if FEATURE_DIAGNOSTICS_TRACESOURCE
            _log.TraceEvent(TraceEventType.Verbose, 1, text);
#endif // FEATURE_DIAGNOSTICS_TRACESOURCE
        }
    }
}
