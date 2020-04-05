using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    public partial class ForwardedPortLocal
    {
        private Socket _listener;
        private CountdownEvent _pendingChannelCountdown;

        partial void InternalStart()
        {
            var addr = DnsAbstraction.GetHostAddresses(BoundHost)[0];
            var ep = new IPEndPoint(addr, (int) BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};
            _listener.Bind(ep);
            _listener.Listen(5);

            // update bound port (in case original was passed as zero)
            BoundPort = (uint)((IPEndPoint)_listener.LocalEndPoint).Port;

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            InitializePendingChannelCountdown();

            // consider port started when we're listening for inbound connections
            _status = ForwardedPortStatus.Started;

            StartAccept(null);
        }

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

            // only accept new connections while we are started
            if (IsStarted)
            {
                try
                {
                    if (!_listener.AcceptAsync(e))
                    {
                        AcceptCompleted(null, e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    if (_status == ForwardedPortStatus.Stopped || _status == ForwardedPortStatus.Stopped)
                    {
                        // ignore ObjectDisposedException while stopping or stopped
                        return;
                    }

                    throw;
                }
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted || e.SocketError == SocketError.NotSocket)
            {
                // server was stopped
                return;
            }

            // capture client socket
            var clientSocket = e.AcceptSocket;

            if (e.SocketError != SocketError.Success)
            {
                // accept new connection
                StartAccept(e);
                // dispose broken client socket
                CloseClientSocket(clientSocket);
                return;
            }

            // accept new connection
            StartAccept(e);
            // process connection
            ProcessAccept(clientSocket);
        }

        private void ProcessAccept(Socket clientSocket)
        {
            // close the client socket if we're no longer accepting new connections
            if (!IsStarted)
            {
                CloseClientSocket(clientSocket);
                return;
            }

            // capture the countdown event that we're adding a count to, as we need to make sure that we'll be signaling
            // that same instance; the instance field for the countdown event is re-initialized when the port is restarted
            // and at that time there may still be pending requests
            var pendingChannelCountdown = _pendingChannelCountdown;

            pendingChannelCountdown.AddCount();

            try
            {
                var originatorEndPoint = (IPEndPoint) clientSocket.RemoteEndPoint;

                RaiseRequestReceived(originatorEndPoint.Address.ToString(),
                    (uint)originatorEndPoint.Port);

                using (var channel = Session.CreateChannelDirectTcpip())
                {
                    channel.Exception += Channel_Exception;
                    channel.Open(Host, Port, this, clientSocket);
                    channel.Bind();
                }
            }
            catch (Exception exp)
            {
                RaiseExceptionEvent(exp);
                CloseClientSocket(clientSocket);
            }
            finally
            {
                // take into account that CountdownEvent has since been disposed; when stopping the port we
                // wait for a given time for the channels to close, but once that timeout period has elapsed
                // the CountdownEvent will be disposed
                try
                {
                    pendingChannelCountdown.Signal();
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
        private void InitializePendingChannelCountdown()
        {
            var original = Interlocked.Exchange(ref _pendingChannelCountdown, new CountdownEvent(1));
            if (original != null)
            {
                original.Dispose();
            }
        }

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

        /// <summary>
        /// Interrupts the listener, and unsubscribes from <see cref="Session"/> events.
        /// </summary>
        partial void StopListener()
        {
            // close listener socket
            var listener = _listener;
            if (listener != null)
            {
                listener.Dispose();
            }

            // unsubscribe from session events
            var session = Session;
            if (session != null)
            {
                session.ErrorOccured -= Session_ErrorOccured;
                session.Disconnected -= Session_Disconnected;
            }
        }

        /// <summary>
        /// Waits for pending channels to close.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the pending channels to close.</param>
        partial void InternalStop(TimeSpan timeout)
        {
            _pendingChannelCountdown.Signal();
            if (!_pendingChannelCountdown.Wait(timeout))
            {
                // TODO: log as warning
                DiagnosticAbstraction.Log("Timeout waiting for pending channels in local forwarded port to close.");
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

                var pendingRequestsCountdown = _pendingChannelCountdown;
                if (pendingRequestsCountdown != null)
                {
                    _pendingChannelCountdown = null;
                    pendingRequestsCountdown.Dispose();
                }
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            var session = Session;
            if (session != null)
            {
                StopPort(session.ConnectionInfo.Timeout);
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            var session = Session;
            if (session != null)
            {
                StopPort(session.ConnectionInfo.Timeout);
            }
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
            RaiseExceptionEvent(e.Exception);
        }
    }
}
