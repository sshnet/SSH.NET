using System;
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
        private CountdownEvent _pendingRequestsCountdown;

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
            _listener.Listen(5);

            // update bound port (in case original was passed as zero)
            BoundPort = (uint)((IPEndPoint)_listener.LocalEndPoint).Port;

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            _listenerTaskCompleted = new ManualResetEvent(false);
            InitializePendingRequestsCountdown();

            ThreadAbstraction.ExecuteThread(() =>
                {
                    try
                    {
#if FEATURE_SOCKET_EAP
                        StartAccept(null);
#elif FEATURE_SOCKET_APM
                        _listener.BeginAccept(AcceptCallback, _listener);
#elif FEATURE_SOCKET_TAP
#error Accepting new socket connections is not implemented.
#else
#error Accepting new socket connections is not implemented.
#endif
                    }
                    catch (ObjectDisposedException)
                    {
                        // BeginAccept / AcceptAsync will throw an ObjectDisposedException when the
                        // server is closed before the listener has started accepting connections.
                        //
                        // As we start accepting connection on a separate thread, this is possible
                        // when the listener is stopped right after it was started.
                    }

                    // wait until listener is stopped
                    _listenerTaskCompleted.WaitOne();

                    var session = Session;
                    if (session != null)
                    {
                        session.Disconnected -= Session_Disconnected;
                        session.ErrorOccured -= Session_ErrorOccured;
                    }
                });
        }

#if FEATURE_SOCKET_EAP
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += AcceptCompleted;
            }
            else
            {
                // clear the socket as we're reusing the context object
                e.AcceptSocket = null;
            }

            if (!_listener.AcceptAsync(e))
            {
                AcceptCompleted(null, e);
            }
        }


        private void AcceptCompleted(object sender, SocketAsyncEventArgs acceptAsyncEventArgs)
        { 
            if (acceptAsyncEventArgs.SocketError == SocketError.OperationAborted)
            {
                // server was stopped
                return;
            }

            // capture client socket
            var clientSocket = acceptAsyncEventArgs.AcceptSocket;

            if (acceptAsyncEventArgs.SocketError != SocketError.Success)
            {
                // accept new connection
                StartAccept(acceptAsyncEventArgs);
                // close client socket
                CloseClientSocket(clientSocket);
                return;
            }

            // accept new connection
            StartAccept(acceptAsyncEventArgs);
            // process connection
            ProcessAccept(clientSocket);
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

        private static void CloseClientSocket(Socket clientSocket)
        {
            if (clientSocket.Connected)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Send);
                }
                catch (Exception)
                {
                    // ignore exception when client socket was already closed
                }
            }

            clientSocket.Dispose();
        }

        private void ProcessAccept(Socket clientSocket)
        {
            // capture the countdown event that we're adding a count to, as we need to make sure that we'll be signaling
            // that same instance; the instance field for the countdown event is re-initialized when the port is restarted
            // and at that time they may still be pending requests
            var pendingRequestsCountdown = _pendingRequestsCountdown;

            pendingRequestsCountdown.AddCount();

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
                CloseClientSocket(clientSocket);
            }
            finally
            {
                // take into account that countdown event has since been disposed (after waiting for a given timeout)
                try
                {
                    pendingRequestsCountdown.Signal();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="CountdownEvent"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the port is started for the first time, a <see cref="CountdownEvent"/> is created with an initial count
        /// of <c>1</c>.
        /// </para>
        /// <para>
        /// On subsequent (re)starts, we'll dispose the current <see cref="CountdownEvent"/> and create a new one with
        /// initial count of <c>1</c>.
        /// </para>
        /// </remarks>
        private void InitializePendingRequestsCountdown()
        {
            var original = Interlocked.Exchange(ref _pendingRequestsCountdown, new CountdownEvent(1));
            if (original != null)
            {
                original.Dispose();
            }
        }

        /// <summary>
        /// Waits for pending requests to finish.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the pending requests to finish.</param>
        partial void InternalStop(TimeSpan timeout)
        {
            _pendingRequestsCountdown.Signal();
            _pendingRequestsCountdown.Wait(timeout);
        }

        /// <summary>
        /// Interrupts the listener, and waits for the listener loop to finish.
        /// </summary>
        partial void StopListener()
        {
            // close listener socket
            var listener = _listener;
            if (listener != null)
            {
                listener.Dispose();
            }

            // allow listener thread to stop
            var listenerTaskCompleted = _listenerTaskCompleted;
            if (listenerTaskCompleted != null)
            {
                listenerTaskCompleted.Set();
            }
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

                var pendingRequestsCountdown = _pendingRequestsCountdown;
                if (pendingRequestsCountdown != null)
                {
                    _pendingRequestsCountdown = null;
                    pendingRequestsCountdown.Dispose();
                }
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            if (IsStarted)
            {
                StopListener();
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            if (IsStarted)
            {
                StopListener();
            }
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
            RaiseExceptionEvent(e.Exception);
        }
    }
}
