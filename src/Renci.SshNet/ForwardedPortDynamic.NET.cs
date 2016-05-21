using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    public partial class ForwardedPortDynamic
    {
        private Socket _listener;
        private int _pendingRequests;

#if FEATURE_SOCKET_EAP
        private ManualResetEvent _stoppingListener;
#endif // FEATURE_SOCKET_EAP

        partial void InternalStart()
        {
            var ip = IPAddress.Any;
            if (!string.IsNullOrEmpty(BoundHost))
            {
                ip = DnsAbstraction.GetHostAddresses(BoundHost)[0];
            }

            var ep = new IPEndPoint(ip, (int) BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // TODO: decide if we want to have blocking socket
#if FEATURE_SOCKET_SETSOCKETOPTION
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
#endif //FEATURE_SOCKET_SETSOCKETOPTION
            _listener.Bind(ep);
            _listener.Listen(5);

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            _listenerCompleted = new ManualResetEvent(false);

            ThreadAbstraction.ExecuteThread(() =>
                {
                    try
                    {
#if FEATURE_SOCKET_EAP
                        _stoppingListener = new ManualResetEvent(false);

                        StartAccept();

                        _stoppingListener.WaitOne();
#elif FEATURE_SOCKET_APM
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
#elif FEATURE_SOCKET_TAP
#error Accepting new socket connections is not implemented.
#else
#error Accepting new socket connections is not implemented.
#endif
                    }
                    catch (Exception ex)
                    {
                        RaiseExceptionEvent(ex);
                    }
                    finally
                    {
                        if (Session != null)
                        {
                            Session.ErrorOccured -= Session_ErrorOccured;
                            Session.Disconnected -= Session_Disconnected;
                        }

                        // mark listener stopped
                        _listenerCompleted.Set();
                    }
                });
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            StopListener();
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            StopListener();
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
            if (acceptAsyncEventArgs.SocketError != SocketError.Success)
            {
                StartAccept();
                acceptAsyncEventArgs.AcceptSocket.Dispose();
                return;
            }

            StartAccept();

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
                // when the socket is closed, an ObjectDisposedException is thrown
                // by Socket.EndAccept(IAsyncResult)
                return;
            }

            ProcessAccept(clientSocket);
        }
#endif

        private void ProcessAccept(Socket remoteSocket)
        {
            Interlocked.Increment(ref _pendingRequests);

#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | " + remoteSocket.RemoteEndPoint + " | ForwardedPortDynamic.ProcessAccept | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

            try
            {
#if FEATURE_SOCKET_SETSOCKETOPTION
                remoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                remoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
#endif //FEATURE_SOCKET_SETSOCKETOPTION

                using (var channel = Session.CreateChannelDirectTcpip())
                {
                    channel.Exception += Channel_Exception;

                    try
                    {
                        if (!HandleSocks(channel, remoteSocket, Session.ConnectionInfo.Timeout))
                        {
                            CloseSocket(remoteSocket);
                            return;
                        }

                        // start receiving from client socket (and sending to server)
                        channel.Bind();
                    }
#if DEBUG_GERT
                    catch (SocketException ex)
                    {
                        Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | " + ex.SocketErrorCode + " | " + DateTime.Now.ToString("hh:mm:ss.fff") + " | " + ex);
                    }
#endif // DEBUG_GERT
                    finally
                    {
                        channel.Close();
                    }
                }
            }
            catch (SocketException ex)
            {
                // ignore exception thrown by interrupting the blocking receive as part of closing
                // the forwarded port
                if (ex.SocketErrorCode != SocketError.Interrupted)
                {
#if DEBUG_GERT
                    RaiseExceptionEvent(new Exception("ID: " + Thread.CurrentThread.ManagedThreadId, ex));
#else
                    RaiseExceptionEvent(ex);
#endif // DEBUG_GERT
                }
                CloseSocket(remoteSocket);
            }
            catch (Exception exp)
            {
#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | " + exp + " | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT
                RaiseExceptionEvent(exp);
                CloseSocket(remoteSocket);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequests);
            }
        }

        private bool HandleSocks(IChannelDirectTcpip channel, Socket remoteSocket, TimeSpan timeout)
        {
            // create eventhandler which is to be invoked to interrupt a blocking receive
            // when we're closing the forwarded port
            EventHandler closeClientSocket = (_, args) => CloseSocket(remoteSocket);

            Closing += closeClientSocket;

            try
            {
#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | Before ReadByte for version | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

                var version = SocketAbstraction.ReadByte(remoteSocket, timeout);
                if (version == -1)
                {
                    return false;
                }

#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | After ReadByte for version | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

                if (version == 4)
                {
                    return HandleSocks4(remoteSocket, channel, timeout);
                }
                else if (version == 5)
                {
                    return HandleSocks5(remoteSocket, channel, timeout);
                }
                else
                {
                    throw new NotSupportedException(string.Format("SOCKS version {0} is not supported.", version));
                }
            }
            finally
            {
                // interrupt of blocking receive is now handled by channel (SOCKS4 and SOCKS5)
                // or no longer necessary
                Closing -= closeClientSocket;
            }

        }

        private static void CloseSocket(Socket socket)
        {
#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | ForwardedPortDynamic.CloseSocket | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
            }
        }

        partial void StopListener()
        {
            //  if the port is not started then there's nothing to stop
            if (!IsStarted)
                return;

#if FEATURE_SOCKET_EAP
            _stoppingListener.Set();
#endif // FEATURE_SOCKET_EAP

            // close listener socket
            _listener.Dispose();
            // wait for listener loop to finish
            _listenerCompleted.WaitOne();
        }

        /// <summary>
        /// Waits for pending requests to finish, and channels to close.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the forwarded port to stop.</param>
        partial void InternalStop(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero)
                return;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // break out of loop when one of the following conditions are met:
            // * the forwarded port is restarted
            // * all pending requests have been processed and corresponding channel are closed
            // * the specified timeout has elapsed
            while (!IsStarted)
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

        partial void InternalDispose(bool disposing)
        {
            if (disposing)
            {
                if (_listener != null)
                {
                    _listener.Dispose();
                    _listener = null;
                }

#if FEATURE_SOCKET_EAP
                if (_stoppingListener != null)
                {
                    _stoppingListener.Dispose();
                    _stoppingListener = null;
                }
#endif // FEATURE_SOCKET_EAP
            }
        }

        private bool HandleSocks4(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
        {
            var commandCode = SocketAbstraction.ReadByte(socket, timeout);
            if (commandCode == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            //  TODO:   See what need to be done depends on the code

            var portBuffer = new byte[2];
            if (SocketAbstraction.Read(socket, portBuffer, 0, portBuffer.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            var port = (uint)(portBuffer[0] * 256 + portBuffer[1]);

            var ipBuffer = new byte[4];
            if (SocketAbstraction.Read(socket, ipBuffer, 0, ipBuffer.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            var ipAddress = new IPAddress(ipBuffer);

            var username = ReadString(socket, timeout);
            if (username == null)
            {
                // SOCKS client closed connection
                return false;
            }

            var host = ipAddress.ToString();

            RaiseRequestReceived(host, port);

            channel.Open(host, port, this, socket);

            SocketAbstraction.SendByte(socket, 0x00);

            if (channel.IsOpen)
            {
                SocketAbstraction.SendByte(socket, 0x5a);
                SocketAbstraction.Send(socket, portBuffer, 0, portBuffer.Length);
                SocketAbstraction.Send(socket, ipBuffer, 0, ipBuffer.Length);
                return true;
            }

            // signal that request was rejected or failed
            SocketAbstraction.SendByte(socket, 0x5b);
            return false;
        }

        private bool HandleSocks5(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
        {
#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  Handling Socks5: " + socket.LocalEndPoint +  " | " + socket.RemoteEndPoint + " | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

            var authenticationMethodsCount = SocketAbstraction.ReadByte(socket, timeout);
            if (authenticationMethodsCount == -1)
            {
                // SOCKS client closed connection
                return false;
            }

#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  After ReadByte for authenticationMethodsCount | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

            var authenticationMethods = new byte[authenticationMethodsCount];
            if (SocketAbstraction.Read(socket, authenticationMethods, 0, authenticationMethods.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  After Read for authenticationMethods | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

            if (authenticationMethods.Min() == 0)
            {
                // no user authentication is one of the authentication methods supported
                // by the SOCKS client
                SocketAbstraction.Send(socket, new byte[] { 0x05, 0x00 }, 0, 2);

#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  After Send for authenticationMethods 0 | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT
            }
            else
            {
                // the SOCKS client requires authentication, which we currently do not support
                SocketAbstraction.Send(socket, new byte[] { 0x05, 0xFF }, 0, 2);

                // we continue business as usual but expect the client to close the connection
                // so one of the subsequent reads should return -1 signaling that the client
                // has effectively closed the connection
#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  After Send for authenticationMethods 2 | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT
            }

            var version = SocketAbstraction.ReadByte(socket, timeout);
            if (version == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            if (version != 5)
                throw new ProxyException("SOCKS5: Version 5 is expected.");

            var commandCode = SocketAbstraction.ReadByte(socket, timeout);
            if (commandCode == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            var reserved = SocketAbstraction.ReadByte(socket, timeout);
            if (reserved == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            if (reserved != 0)
            {
                throw new ProxyException("SOCKS5: 0 is expected for reserved byte.");
            }

            var addressType = SocketAbstraction.ReadByte(socket, timeout);
            if (addressType == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            IPAddress ipAddress;
            byte[] addressBuffer;
            switch (addressType)
            {
                case 0x01:
                    {
                        addressBuffer = new byte[4];
                        if (SocketAbstraction.Read(socket, addressBuffer, 0, 4, timeout) == 0)
                        {
                            // SOCKS client closed connection
                            return false;
                        }

                        ipAddress = new IPAddress(addressBuffer);
                    }
                    break;
                case 0x03:
                    {
                        var length = SocketAbstraction.ReadByte(socket, timeout);
                        addressBuffer = new byte[length];
                        if (SocketAbstraction.Read(socket, addressBuffer, 0, addressBuffer.Length, timeout) == 0)
                        {
                            // SOCKS client closed connection
                            return false;
                        }

                        ipAddress = IPAddress.Parse(SshData.Ascii.GetString(addressBuffer));

                        //var hostName = new Common.ASCIIEncoding().GetString(addressBuffer);

                        //ipAddress = Dns.GetHostEntry(hostName).AddressList[0];
                    }
                    break;
                case 0x04:
                    {
                        addressBuffer = new byte[16];
                        if (SocketAbstraction.Read(socket, addressBuffer, 0, 16, timeout) == 0)
                        {
                            // SOCKS client closed connection
                            return false;
                        }

                        ipAddress = new IPAddress(addressBuffer);
                    }
                    break;
                default:
                    throw new ProxyException(string.Format("SOCKS5: Address type '{0}' is not supported.", addressType));
            }

            var portBuffer = new byte[2];
            if (SocketAbstraction.Read(socket, portBuffer, 0, portBuffer.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            var port = (uint)(portBuffer[0] * 256 + portBuffer[1]);
            var host = ipAddress.ToString();

            RaiseRequestReceived(host, port);

#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  Before channel open | " + DateTime.Now.ToString("hh:mm:ss.fff"));

            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif // DEBUG_GERT

            channel.Open(host, port, this, socket);

#if DEBUG_GERT
            stopWatch.Stop();

            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " |  After channel open | " + DateTime.Now.ToString("hh:mm:ss.fff") + " => " + stopWatch.ElapsedMilliseconds);
#endif // DEBUG_GERT

            var replyBuffer = new byte[10];
            replyBuffer[0] = 0x05;

//            SocketAbstraction.SendByte(socket, 0x05);


            if (channel.IsOpen)
            {
                replyBuffer[1] = 0x00;
//                SocketAbstraction.SendByte(socket, 0x00);
            }
            else
            {
                replyBuffer[1] = 0x01;
                //SocketAbstraction.SendByte(socket, 0x01);

#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | Channel not open");
#endif // DEBUG_GERT
            }

            // reserved
            replyBuffer[2] = 0x00;

            // reserved
            //SocketAbstraction.SendByte(socket, 0x00);

            //if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            //{
            //    SocketAbstraction.SendByte(socket, 0x01);
            //}
            //else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            //{
            //    SocketAbstraction.SendByte(socket, 0x04);
            //}
            //else
            //{
            //    throw new NotSupportedException("Not supported address family.");
            //}

            // IPv4
            replyBuffer[3] = 0x01;

            SocketAbstraction.Send(socket, replyBuffer, 0, replyBuffer.Length);

            //var addressBytes = IPAddress.Any.GetAddressBytes();
            //SocketAbstraction.Send(socket, addressBytes, 0, addressBytes.Length);
            //SocketAbstraction.Send(socket, new byte[] {0x00, 0x00}, 0, 2);

            //var addressBytes = ipAddress.GetAddressBytes();
            //SocketAbstraction.Send(socket, addressBytes, 0, addressBytes.Length);
            //SocketAbstraction.Send(socket, portBuffer, 0, portBuffer.Length);

            return true;
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | Channel_Exception | " +
                              DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT
            RaiseExceptionEvent(e.Exception);
        }

        /// <summary>
        /// Reads a null terminated string from a socket.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="timeout">The timeout to apply to individual reads.</param>
        /// <returns>
        /// The <see cref="string"/> read, or <c>null</c> when the socket was closed.
        /// </returns>
        private static string ReadString(Socket socket, TimeSpan timeout)
        {
            var text = new StringBuilder();
            var buffer = new byte[1];
            while (true)
            {
                if (SocketAbstraction.Read(socket, buffer, 0, 1, timeout) == 0)
                {
                    // SOCKS client closed connection
                    return null;
                }

                var byteRead = buffer[0];
                if (byteRead == 0)
                {
                    // end of the string
                    break;
                }

                var c = (char) byteRead;
                text.Append(c);
            }
            return text.ToString();
        }
    }
}
