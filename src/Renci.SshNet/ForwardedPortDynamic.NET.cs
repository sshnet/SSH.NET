using System;
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
        private CountdownEvent _pendingRequestsCountdown;

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
                    catch (Exception ex)
                    {
                        RaiseExceptionEvent(ex);
                    }

                    // wait until listener is stopped
                    _listenerCompleted.WaitOne();

                    var session = Session;
                    if (session != null)
                    {
                        session.ErrorOccured -= Session_ErrorOccured;
                        session.Disconnected -= Session_Disconnected;
                    }
                });
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            if (IsStarted)
            {
                StopListener();
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            if (IsStarted)
            {
                StopListener();
            }
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
                // dispose broken client socket
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

        private void ProcessAccept(Socket remoteSocket)
        {
            // capture the countdown event that we're adding a count to, as we need to make sure that we'll be signaling
            // that same instance; the instance field for the countdown event is re-initialized when the port is restarted
            // and at that time they may still be pending requests
            var pendingRequestsCountdown = _pendingRequestsCountdown;

            pendingRequestsCountdown.AddCount();

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
                            CloseClientSocket(remoteSocket);
                            return;
                        }

                        // start receiving from client socket (and sending to server)
                        channel.Bind();
                    }
#if DEBUG_GERT
                    catch (SocketException ex)
                    {
                        Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | " + ex.SocketErrorCode + " | " + DateTime.Now.ToString("hh:mm:ss.fff") + " | " + ex);
                        throw;
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
                CloseClientSocket(remoteSocket);
            }
            catch (Exception exp)
            {
#if DEBUG_GERT
                Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | " + exp + " | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT
                RaiseExceptionEvent(exp);
                CloseClientSocket(remoteSocket);
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

        private bool HandleSocks(IChannelDirectTcpip channel, Socket remoteSocket, TimeSpan timeout)
        {
            // create eventhandler which is to be invoked to interrupt a blocking receive
            // when we're closing the forwarded port
            EventHandler closeClientSocket = (_, args) => CloseClientSocket(remoteSocket);

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

        private static void CloseClientSocket(Socket clientSocket)
        {
#if DEBUG_GERT
            Console.WriteLine("ID: " + Thread.CurrentThread.ManagedThreadId + " | ForwardedPortDynamic.CloseSocket | " + DateTime.Now.ToString("hh:mm:ss.fff"));
#endif // DEBUG_GERT

            if (clientSocket.Connected)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // ignore exception when client socket was already closed
                }

            }

            clientSocket.Dispose();
        }

        partial void StopListener()
        {
            // close listener socket
            var listener = _listener;
            if (listener != null)
            {
                listener.Dispose();
            }

            // allow listener thread to stop
            var listenerCompleted = _listenerCompleted;
            if (listenerCompleted != null)
            {
                listenerCompleted.Set();
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

        private bool HandleSocks4(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
        {
            var commandCode = SocketAbstraction.ReadByte(socket, timeout);
            if (commandCode == -1)
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
                        if (length == -1)
                        {
                            // SOCKS client closed connection
                            return false;
                        }
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

            SocketAbstraction.SendByte(socket, 0x05);


            if (channel.IsOpen)
            {
                SocketAbstraction.SendByte(socket, 0x00);
            }
            else
            {
                SocketAbstraction.SendByte(socket, 0x01);
            }

            // reserved
            SocketAbstraction.SendByte(socket, 0x00);

            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                SocketAbstraction.SendByte(socket, 0x01);
            }
            else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                SocketAbstraction.SendByte(socket, 0x04);
            }
            else
            {
                throw new NotSupportedException("Not supported address family.");
            }

            var addressBytes = ipAddress.GetAddressBytes();
            SocketAbstraction.Send(socket, addressBytes, 0, addressBytes.Length);
            SocketAbstraction.Send(socket, portBuffer, 0, portBuffer.Length);

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
