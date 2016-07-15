using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal
    {
        private Socket _listener;
        private int _pendingRequests;

        partial void InternalStart()
        {
            var addr = DnsAbstraction.GetHostAddresses(BoundHost)[0];
            var ep = new IPEndPoint(addr, (int) BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // TODO: decide if we want to have blocking socket
#if FEATURE_SOCKET_SETSOCKETOPTION
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
#endif // FEATURE_SOCKET_SETSOCKETOPTION
            _listener.Bind(ep);
            _listener.Listen(1);

            // update bound port (in case original was passed as zero)
            BoundPort = (uint)((IPEndPoint)_listener.LocalEndPoint).Port;

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            _listenerTaskCompleted = new ManualResetEvent(false);

            ThreadAbstraction.ExecuteThread(() =>
                {
                    try
                    {
#if FEATURE_SOCKET_EAP
                        StartAccept();
#elif FEATURE_SOCKET_APM
                        _listener.BeginAccept(AcceptCallback, _listener);
#elif FEATURE_SOCKET_TAP
#error Accepting new socket connections is not implemented.
#else
#error Accepting new socket connections is not implemented.
#endif

                        // wait until listener is stopped
                        _listenerTaskCompleted.WaitOne();
                    }
                    catch (ObjectDisposedException)
                    {
                        // BeginAccept / AcceptAsync will throw an ObjectDisposedException when the
                        // server is closed before the listener has started accepting connections.
                        //
                        // As we start accepting connection on a separate thread, this is possible
                        // when the listener is stopped right after it was started.

                        // mark listener stopped
                        _listenerTaskCompleted.Set();
                    }
                    catch (Exception ex)
                    {
                        RaiseExceptionEvent(ex);

                        // mark listener stopped
                        _listenerTaskCompleted.Set();
                    }
                    finally
                    {
                        if (Session != null)
                        {
                            Session.Disconnected -= Session_Disconnected;
                            Session.ErrorOccured -= Session_ErrorOccured;
                        }
                    }
                });
        }

#if FEATURE_SOCKET_EAP
        private void StartAccept()
        {
            var args = new SocketAsyncEventArgs();
            args.Completed += AcceptCompleted;

            if (!_listener.AcceptAsync(args))
            {
                AcceptCompleted(null, args);
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs acceptAsyncEventArgs)
        {
            if (acceptAsyncEventArgs.SocketError == SocketError.OperationAborted)
            {
                // server was stopped
                return;
            }

            if (acceptAsyncEventArgs.SocketError != SocketError.Success)
            {
                // accept new connection
                StartAccept();
                // dispose broken socket
                acceptAsyncEventArgs.AcceptSocket.Dispose();
                return;
            }

            // accept new connection
            StartAccept();
            // process connection
            ProcessAccept(acceptAsyncEventArgs.AcceptSocket);
        }
#elif FEATURE_SOCKET_APM
        private void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request
            var serverSocket = (Socket) ar.AsyncState;

            Socket clientSocket;

            try
            {
                clientSocket = serverSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                // when the server socket is closed, an ObjectDisposedException is thrown
                // by Socket.EndAccept(IAsyncResult)
                return;
            }

            // accept new connection
            _listener.BeginAccept(AcceptCallback, _listener);
            // process connection
            ProcessAccept(clientSocket);
        }
#endif

        private void ProcessAccept(Socket clientSocket)
        {
            Interlocked.Increment(ref _pendingRequests);

            try
            {
#if FEATURE_SOCKET_SETSOCKETOPTION
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
#endif //FEATURE_SOCKET_SETSOCKETOPTION

                var originatorEndPoint = (IPEndPoint) clientSocket.RemoteEndPoint;

                RaiseRequestReceived(originatorEndPoint.Address.ToString(),
                    (uint)originatorEndPoint.Port);

                using (var channel = Session.CreateChannelDirectTcpip())
                {
                    channel.Exception += Channel_Exception;
                    channel.Open(Host, Port, this, clientSocket);
                    channel.Bind();
                    channel.Close();
                }
            }
            catch (Exception exp)
            {
                RaiseExceptionEvent(exp);
                CloseSocket(clientSocket);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequests);
            }
        }

        private static void CloseSocket(Socket socket)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
            }
        }

        partial void InternalStop(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero)
                return;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (true)
            {
                // break out of loop when all pending requests have been processed
                if (Interlocked.CompareExchange(ref _pendingRequests, 0, 0) == 0)
                    break;
                // break out of loop when specified timeout has elapsed
                if (stopWatch.Elapsed >= timeout && timeout != SshNet.Session.InfiniteTimeSpan)
                    break;
                // give channels time to process pending requests
                ThreadAbstraction.Sleep(50);
            }

            stopWatch.Stop();
        }

        /// <summary>
        /// Interrupts the listener, and waits for the listener loop to finish.
        /// </summary>
        /// <remarks>
        /// When the forwarded port is stopped, then any further action is skipped.
        /// </remarks>
        partial void StopListener()
        {
            if (!IsStarted)
                return;

            // close listener socket
            _listener.Dispose();
            // allow listener thread to stop
            _listenerTaskCompleted.Set();
        }

        partial void InternalDispose(bool disposing)
        {
            if (disposing)
            {
                var listener = _listener;
                if (listener != null)
                {
                    _listener = null;
                    listener.Dispose();
                }
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            StopListener();
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            StopListener();
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
            RaiseExceptionEvent(e.Exception);
        }
    }
}
