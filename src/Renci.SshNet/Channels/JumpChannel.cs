﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "direct-tcpip" SSH channel.
    /// </summary>
    internal class JumpChannel
    {
        private Socket listener;
        private ISession _session;
        private EventWaitHandle _channelOpen = new AutoResetEvent(false);

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
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\ForwardedPortLocalTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortLocal" language="C#" title="Local port forwarding" />
        /// </example>
        public JumpChannel(ISession session, string host, uint port)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            port.ValidatePort("port");

            Host = host;
            Port = port;

            _session = session;
        }

        public Socket Connect()
        {
            var ep = new IPEndPoint(IPAddress.Loopback, 0);
            listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            listener.Bind(ep);
            listener.Listen(1);

            IsStarted = true;

            // update bound port (in case original was passed as zero)
            ep.Port = ((IPEndPoint)listener.LocalEndPoint).Port;

            var e = new SocketAsyncEventArgs();
            e.Completed += AcceptCompleted;

            // only accept new connections while we are started
            if (!listener.AcceptAsync(e))
            {
                AcceptCompleted(null, e);
            }

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ep);

            //  Wait for channel to open
            _session.WaitOnHandle(_channelOpen);
            listener.Dispose();
            listener = null;

            return socket;
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortLocal"/> is reclaimed by garbage collection.
        /// </summary>
        ~JumpChannel()
        {
            Dispose(false);
        }

        #endregion


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
                // dispose broken client socket
                CloseClientSocket(clientSocket);
                return;
            }

            _channelOpen.Set();

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
                    channel.Open(Host, Port, null, clientSocket);
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