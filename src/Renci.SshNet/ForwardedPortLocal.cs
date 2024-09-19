using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding.
    /// </summary>
    public partial class ForwardedPortLocal : ForwardedPort
    {
        private ForwardedPortStatus _status;
        private bool _isDisposed;
        private Socket _listener;
        private CountdownEvent _pendingChannelCountdown;

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public string BoundHost { get; private set; }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; private set; }

        /// <summary>
        /// Gets the forwarded host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the forwarded port.
        /// </summary>
        public uint Port { get; private set; }

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if port forwarding is started; otherwise, <see langword="false"/>.
        /// </value>
        public override bool IsStarted
        {
            get { return _status == ForwardedPortStatus.Started; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortLocal(uint boundPort, string host, uint port)
            : this(string.Empty, boundPort, host, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundHost">The bound host.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="boundHost"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortLocal(string boundHost, string host, uint port)
            : this(boundHost, 0, host, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundHost">The bound host.</param>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="boundHost"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortLocal(string boundHost, uint boundPort, string host, uint port)
        {
            ThrowHelper.ThrowIfNull(boundHost);
            ThrowHelper.ThrowIfNull(host);

            boundPort.ValidatePort();
            port.ValidatePort();

            BoundHost = boundHost;
            BoundPort = boundPort;
            Host = host;
            Port = port;
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            if (!ForwardedPortStatus.ToStarting(ref _status))
            {
                return;
            }

            try
            {
                InternalStart();
            }
            catch (Exception)
            {
                _status = ForwardedPortStatus.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Stops local port forwarding, and waits for the specified timeout until all pending
        /// requests are processed.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for pending requests to finish processing.</param>
        protected override void StopPort(TimeSpan timeout)
        {
            timeout.EnsureValidTimeout();

            if (!ForwardedPortStatus.ToStopping(ref _status))
            {
                return;
            }

            // signal existing channels that the port is closing
            base.StopPort(timeout);

            // prevent new requests from getting processed
            StopListener();

            // wait for open channels to close
            InternalStop(timeout);

            // mark port stopped
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Ensures the current instance is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance is disposed.</exception>
        protected override void CheckDisposed()
        {
            ThrowHelper.ThrowObjectDisposedIf(_isDisposed, this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            base.Dispose(disposing);
            InternalDispose(disposing);
            _isDisposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        ~ForwardedPortLocal()
        {
            Dispose(disposing: false);
        }

        private void InternalStart()
        {
            var addr = Dns.GetHostAddresses(BoundHost)[0];
            var ep = new IPEndPoint(addr, (int)BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _listener.Bind(ep);
            _listener.Listen(5);

            // update bound port (in case original was passed as zero)
            BoundPort = (uint)((IPEndPoint)_listener.LocalEndPoint).Port;

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            InitializePendingChannelCountdown();

            // consider port started when we're listening for inbound connections
            _status = ForwardedPortStatus.Started;

            StartAccept(e: null);
        }

        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e is null)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                e = new SocketAsyncEventArgs();
#pragma warning restore CA2000 // Dispose objects before losing scope
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
                        AcceptCompleted(sender: null, e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    if (_status == ForwardedPortStatus.Stopping || _status == ForwardedPortStatus.Stopped)
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
            if (e.SocketError is SocketError.OperationAborted or SocketError.NotSocket)
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
                var originatorEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;

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
                    _ = pendingChannelCountdown.Signal();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore any ObjectDisposedException
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
            original?.Dispose();
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
        private void StopListener()
        {
            // close listener socket
            _listener?.Dispose();

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
        private void InternalStop(TimeSpan timeout)
        {
            _ = _pendingChannelCountdown.Signal();

            if (!_pendingChannelCountdown.Wait(timeout))
            {
                // TODO: log as warning
                DiagnosticAbstraction.Log("Timeout waiting for pending channels in local forwarded port to close.");
            }
        }

        private void InternalDispose(bool disposing)
        {
            if (disposing)
            {
                var listener = _listener;
                if (listener is not null)
                {
                    _listener = null;
                    listener.Dispose();
                }

                var pendingRequestsCountdown = _pendingChannelCountdown;
                if (pendingRequestsCountdown is not null)
                {
                    _pendingChannelCountdown = null;
                    pendingRequestsCountdown.Dispose();
                }
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            var session = Session;
            if (session is not null)
            {
                StopPort(session.ConnectionInfo.Timeout);
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            var session = Session;
            if (session is not null)
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
