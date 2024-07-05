using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Renci.SshNet.Common;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "direct-tcpip" SSH channel.
    /// </summary>
    internal sealed class JumpChannel : IDisposable
    {
        private readonly ISession _session;
        private readonly EventWaitHandle _channelOpen = new AutoResetEvent(initialState: false);

        private Socket _listener;

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
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
        /// </value>
        public bool IsStarted
        { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JumpChannel"/> class.
        /// </summary>
        /// <param name="session">The session used to create the channel.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        public JumpChannel(ISession session, string host, uint port)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            port.ValidatePort("port");

            Host = host;
            Port = port;

            _session = session;
        }

        public Socket Connect()
        {
            var ep = new IPEndPoint(IPAddress.Loopback, 0);
            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _listener.Bind(ep);
            _listener.Listen(1);

            IsStarted = true;

            // update bound port (in case original was passed as zero)
            ep.Port = ((IPEndPoint)_listener.LocalEndPoint).Port;

            using (var e = new SocketAsyncEventArgs())
            {
                e.Completed += AcceptCompleted;

                // only accept new connections while we are started
                if (!_listener.AcceptAsync(e))
                {
                    AcceptCompleted(sender: null, e);
                }
            }

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ep);

            // Wait for channel to open
            _session.WaitOnHandle(_channelOpen);
            _listener.Dispose();
            _listener = null;

            return socket;
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Don't dispose the _session here, as it's considered 'owned' by the object that instantiated this JumpChannel (usually SSHConnector)
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortLocal"/> is reclaimed by garbage collection.
        /// </summary>
        ~JumpChannel()
        {
            Dispose(disposing: false);
        }

        #endregion

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
                // dispose broken client socket
                CloseClientSocket(clientSocket);
                return;
            }

            _ = _channelOpen.Set();

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

            try
            {
                var originatorEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;

                using (var channel = _session.CreateChannelDirectTcpip())
                {
                    channel.Open(Host, Port, forwardedPort: null, clientSocket);
                    channel.Bind();
                }
            }
            catch
            {
                CloseClientSocket(clientSocket);
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
    }
}
