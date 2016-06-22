using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Common
{
    public class AsyncSocketListener : IDisposable
    {
        private readonly IPEndPoint _endPoint;
        private readonly ManualResetEvent _acceptCallbackDone;
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
        }

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
            if (_listener != null)
            {
                _listener.Dispose();
                _listener = null;
            }
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
            var listener = (Socket)state;
            while (_started)
            {
                _acceptCallbackDone.Reset();
                listener.BeginAccept(AcceptCallback, listener);
                _acceptCallbackDone.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            _acceptCallbackDone.Set();

            // Get the socket that handles the client request.
            var listener = (Socket)ar.AsyncState;
            try
            {
                var handler = listener.EndAccept(ar);
                SignalConnected(handler);
                var state = new SocketStateObject(handler);
                handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReadCallback, state);
            }
            catch (ObjectDisposedException)
            {
                // when the socket is closed, an ObjectDisposedException is thrown
                // by Socket.EndAccept(IAsyncResult)
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (SocketStateObject)ar.AsyncState;
            var handler = state.Socket;

            // Read data from the client socket.
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                var bytesReceived = new byte[bytesRead];
                Array.Copy(state.Buffer, bytesReceived, bytesRead);
                SignalBytesReceived(bytesReceived, handler);

                // prepare to receive more bytes
                try
                {
                    handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReadCallback, state);
                }
                catch (ObjectDisposedException)
                {
                    // when the socket is closed, an ObjectDisposedException is thrown
                }
            }
            else
            {
                SignalDisconnected(handler);
                handler.Shutdown(SocketShutdown.Send);
            }
        }

        private void SignalBytesReceived(byte[] bytesReceived, Socket client)
        {
            var subscribers = BytesReceived;
            if (subscribers != null)
                subscribers(bytesReceived, client);
        }

        private void SignalConnected(Socket client)
        {
            var subscribers = Connected;
            if (subscribers != null)
                subscribers(client);
        }

        private void SignalDisconnected(Socket client)
        {
            var subscribers = Disconnected;
            if (subscribers != null)
                subscribers(client);
        }

        private class SocketStateObject
        {
            public Socket Socket { get; private set; }

            public readonly byte[] Buffer = new byte[1024];

            public SocketStateObject(Socket handler)
            {
                Socket = handler;
            }
        }
    }
}
