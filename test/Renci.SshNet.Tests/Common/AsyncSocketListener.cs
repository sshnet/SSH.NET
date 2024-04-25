#pragma warning disable IDE0005 // Using directive is unnecessary; IntegrationTests use implicit usings
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#pragma warning restore IDE0005

namespace Renci.SshNet.Tests.Common
{
    public class AsyncSocketListener : IDisposable
    {
        private readonly IPEndPoint _endPoint;
        private readonly ManualResetEvent _acceptCallbackDone;
        private readonly List<Socket> _connectedClients;
        private readonly object _syncLock;
        private Socket _listener;
        private Thread _receiveThread;
        private bool _started;

        public delegate void BytesReceivedHandler(byte[] bytesReceived, Socket socket);
        public delegate void ConnectedHandler(Socket socket);

        public event BytesReceivedHandler BytesReceived;
        public event ConnectedHandler Connected;
        public event ConnectedHandler Disconnected;

        public AsyncSocketListener(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            _acceptCallbackDone = new ManualResetEvent(false);
            _connectedClients = new List<Socket>();
            _syncLock = new object();
            ShutdownRemoteCommunicationSocket = true;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Socket.Shutdown(SocketShutdown)"/> is invoked on the <see cref="Socket"/>
        /// that is used to handle the communication with the remote host, when the remote host has closed the connection.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to invoke <see cref="Socket.Shutdown(SocketShutdown)"/> on the <see cref="Socket"/> that is used
        /// to handle the communication with the remote host, when the remote host has closed the connection; otherwise,
        /// <see langword="false"/>. The default is <see langword="true"/>.
        /// </value>
        public bool ShutdownRemoteCommunicationSocket { get; set; }

        public void Start()
        {
            _listener = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(_endPoint);
            _listener.Listen(1);

            _started = true;

            _receiveThread = new Thread(StartListener);
            _receiveThread.Start(_listener);
        }

        public void Stop()
        {
            _started = false;

            lock (_syncLock)
            {
                foreach (var connectedClient in _connectedClients)
                {
                    try
                    {
                        connectedClient.Shutdown(SocketShutdown.Send);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("[{0}] Failure shutting down socket: {1}",
                                                typeof(AsyncSocketListener).FullName,
                                                ex);
                    }

                    DrainSocket(connectedClient);
                    connectedClient.Dispose();
                }

                _connectedClients.Clear();
            }

            _listener?.Dispose();

            if (_receiveThread != null)
            {
                _receiveThread.Join();
                _receiveThread = null;
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        private void StartListener(object state)
        {
            try
            {
                var listener = (Socket)state;
                while (_started)
                {
                    _ = _acceptCallbackDone.Reset();
                    _ = listener.BeginAccept(AcceptCallback, listener);
                    _ = _acceptCallbackDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                // On .NET framework when Thread throws an exception then unit tests
                // were executed without any problem.
                // On new .NET exceptions from Thread breaks unit tests session.
                Console.Error.WriteLine("[{0}] Failure in StartListener: {1}",
                    typeof(AsyncSocketListener).FullName,
                    ex);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue
            _ = _acceptCallbackDone.Set();

            // Get the socket that listens for inbound connections
            var listener = (Socket)ar.AsyncState;

            // Get the socket that handles the client request
            Socket handler;

            try
            {
                handler = listener.EndAccept(ar);
            }
            catch (SocketException ex)
            {
                // The listener is stopped through a Dispose() call, which in turn causes
                // Socket.EndAccept(...) to throw a SocketException or
                // ObjectDisposedException
                //
                // Since we consider such an exception normal when the listener is being
                // stopped, we only write a message to stderr if the listener is considered
                // to be up and running
                if (_started)
                {
                    Console.Error.WriteLine("[{0}] Failure accepting new connection: {1}",
                        typeof(AsyncSocketListener).FullName,
                        ex);
                }
                return;
            }
            catch (ObjectDisposedException ex)
            {
                // The listener is stopped through a Dispose() call, which in turn causes
                // Socket.EndAccept(IAsyncResult) to throw a SocketException or
                // ObjectDisposedException
                //
                // Since we consider such an exception normal when the listener is being
                // stopped, we only write a message to stderr if the listener is considered
                // to be up and running
                if (_started)
                {
                    Console.Error.WriteLine("[{0}] Failure accepting new connection: {1}",
                        typeof(AsyncSocketListener).FullName,
                        ex);
                }
                return;
            }

            // Signal new connection
            SignalConnected(handler);

            lock (_syncLock)
            {
                // Register client socket
                _connectedClients.Add(handler);
            }

            var state = new SocketStateObject(handler);

            try
            {
                _ =handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReadCallback, state);
            }
            catch (SocketException ex)
            {
                // The listener is stopped through a Dispose() call, which in turn causes
                // Socket.BeginReceive(...) to throw a SocketException or
                // ObjectDisposedException
                //
                // Since we consider such an exception normal when the listener is being
                // stopped, we only write a message to stderr if the listener is considered
                // to be up and running
                if (_started)
                {
                    Console.Error.WriteLine("[{0}] Failure receiving new data: {1}",
                        typeof(AsyncSocketListener).FullName,
                        ex);
                }
            }
            catch (ObjectDisposedException ex)
            {
                // The listener is stopped through a Dispose() call, which in turn causes
                // Socket.BeginReceive(...) to throw a SocketException or
                // ObjectDisposedException
                //
                // Since we consider such an exception normal when the listener is being
                // stopped, we only write a message to stderr if the listener is considered
                // to be up and running
                if (_started)
                {
                    Console.Error.WriteLine("[{0}] Failure receiving new data: {1}",
                                            typeof(AsyncSocketListener).FullName,
                                            ex);
                }
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object
            var state = (SocketStateObject)ar.AsyncState;
            var handler = state.Socket;

            int bytesRead;
            try
            {
                // Read data from the client socket.
                bytesRead = handler.EndReceive(ar, out var errorCode);
                if (errorCode != SocketError.Success)
                {
                    bytesRead = 0;
                }
            }
            catch (SocketException ex)
            {
                // The listener is stopped through a Dispose() call, which in turn causes
                // Socket.EndReceive(...) to throw a SocketException or
                // ObjectDisposedException
                //
                // Since we consider such an exception normal when the listener is being
                // stopped, we only write a message to stderr if the listener is considered
                // to be up and running
                if (_started)
                {
                    Console.Error.WriteLine("[{0}] Failure receiving new data: {1}",
                                            typeof(AsyncSocketListener).FullName,
                                            ex);
                }
                return;
            }
            catch (ObjectDisposedException ex)
            {
                // The listener is stopped through a Dispose() call, which in turn causes
                // Socket.EndReceive(...) to throw a SocketException or
                // ObjectDisposedException
                //
                // Since we consider such an exception normal when the listener is being
                // stopped, we only write a message to stderr if the listener is considered
                // to be up and running
                if (_started)
                {
                    Console.Error.WriteLine("[{0}] Failure receiving new data: {1}",
                                            typeof(AsyncSocketListener).FullName,
                                            ex);
                }
                return;
            }

            if (bytesRead > 0)
            {
                var bytesReceived = new byte[bytesRead];
                Array.Copy(state.Buffer, bytesReceived, bytesRead);
                SignalBytesReceived(bytesReceived, handler);

                try
                {
                    _ = handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReadCallback, state);
                }
                catch (ObjectDisposedException)
                {
                    SignalDisconnected(handler);
                }

                return;
            }

            SignalDisconnected(handler);

            if (ShutdownRemoteCommunicationSocket)
            {
                lock (_syncLock)
                {
                    if (!_started)
                    {
                        return;
                    }

                    try
                    {
                        handler.Shutdown(SocketShutdown.Send);
                        handler.Close();
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        // On .NET 7 we got Socket Exception with ConnectionReset from Shutdown method
                        // when the socket is disposed
                    }

                    _ = _connectedClients.Remove(handler);
                }
            }
        }

        private void SignalBytesReceived(byte[] bytesReceived, Socket client)
        {
            BytesReceived?.Invoke(bytesReceived, client);
        }

        private void SignalConnected(Socket client)
        {
            Connected?.Invoke(client);
        }

        private void SignalDisconnected(Socket client)
        {
            Disconnected?.Invoke(client);
        }

        private static void DrainSocket(Socket socket)
        {
            var buffer = new byte[128];

            try
            {
                while (true && socket.Connected)
                {
                    var bytesRead = socket.Receive(buffer);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.Error.WriteLine("[{0}] Failure draining socket ({1}): {2}",
                                        typeof(AsyncSocketListener).FullName,
                                        ex.SocketErrorCode.ToString("G"),
                                        ex);
            }
        }

        private class SocketStateObject
        {
            public Socket Socket { get; private set; }

            public readonly byte[] Buffer = new byte[2048];

            public SocketStateObject(Socket handler)
            {
                Socket = handler;
            }
        }
    }
}
