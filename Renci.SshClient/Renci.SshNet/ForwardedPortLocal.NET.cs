using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading;
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

        partial void ExecuteThread(Action action);

        partial void InternalStart()
        {
            var addr = BoundHost.GetIPAddress();
            var ep = new IPEndPoint(addr, (int) BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {Blocking = true};
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            _listener.Bind(ep);
            _listener.Listen(1);

            // update bound port (in case original was passed as zero)
            BoundPort = (uint)((IPEndPoint)_listener.LocalEndPoint).Port;

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            _listenerTaskCompleted = new ManualResetEvent(false);

            ExecuteThread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            // accept new inbound connection
                            var asyncResult = _listener.BeginAccept(AcceptCallback, _listener);
                            // wait for the connection to be established
                            asyncResult.AsyncWaitHandle.WaitOne();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // BeginAccept will throw an ObjectDisposedException when the
                        // socket is closed
                    }
                    catch (Exception ex)
                    {
                        RaiseExceptionEvent(ex);
                    }
                    finally
                    {
                        // mark listener stopped
                        _listenerTaskCompleted.Set();
                    }
                });
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request
            var serverSocket = (Socket)ar.AsyncState;

            Socket clientSocket;

            try
            {
                clientSocket = serverSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                // when the socket is closed, an ObjectDisposedException is thrown
                // by Socket.EndAccept(IAsyncResult)
                return;
            }

            Interlocked.Increment(ref _pendingRequests);

            try
            {
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);

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
                socket.Close();
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
                Thread.Sleep(50);
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

            Session.Disconnected -= Session_Disconnected;
            Session.ErrorOccured -= Session_ErrorOccured;

            // close listener socket
            _listener.Close();
            // wait for listener loop to finish
            _listenerTaskCompleted.WaitOne();
        }

        private void Session_ErrorOccured(object sender, Common.ExceptionEventArgs e)
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
