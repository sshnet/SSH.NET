using System;
using System.Diagnostics;
using System.IO;
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

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {Blocking = true};
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
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
#elif FEATURE_SOCKET_TAP
#error Accepting new socket connections is not implemented.
#else
#error Accepting new socket connections is not implemented.
#endif
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

            try
            {
                remoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                remoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);

                using (var channel = Session.CreateChannelDirectTcpip())
                {
                    channel.Exception += Channel_Exception;

                    var version = new byte[1];

                    // create eventhandler which is to be invoked to interrupt a blocking receive
                    // when we're closing the forwarded port
                    EventHandler closeClientSocket = (_, args) => CloseSocket(remoteSocket);

                    try
                    {
                        Closing += closeClientSocket;

                        var bytesRead = remoteSocket.Receive(version);
                        if (bytesRead == 0)
                        {
                            CloseSocket(remoteSocket);
                            return;
                        }

                        if (version[0] == 4)
                        {
                            HandleSocks4(remoteSocket, channel);
                        }
                        else if (version[0] == 5)
                        {
                            HandleSocks5(remoteSocket, channel);
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("SOCKS version {0} is not supported.",
                                version[0]));
                        }

                        // interrupt of blocking receive is now handled by channel (SOCKS4 and SOCKS5)
                        // or no longer necessary
                        Closing -= closeClientSocket;

                        // start receiving from client socket (and sending to server)
                        channel.Bind();
                    }
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
                    RaiseExceptionEvent(ex);
                }
                CloseSocket(remoteSocket);
            }
            catch (Exception exp)
            {
                RaiseExceptionEvent(exp);
                CloseSocket(remoteSocket);
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

        private void HandleSocks4(Socket socket, IChannelDirectTcpip channel)
        {
            using (var stream = new NetworkStream(socket))
            {
                var commandCode = stream.ReadByte();
                //  TODO:   See what need to be done depends on the code

                var portBuffer = new byte[2];
                stream.Read(portBuffer, 0, portBuffer.Length);
                var port = (uint)(portBuffer[0] * 256 + portBuffer[1]);

                var ipBuffer = new byte[4];
                stream.Read(ipBuffer, 0, ipBuffer.Length);
                var ipAddress = new IPAddress(ipBuffer);

                var username = ReadString(stream);

                var host = ipAddress.ToString();

                RaiseRequestReceived(host, port);

                channel.Open(host, port, this, socket);

                using (var writeStream = new MemoryStream())
                {
                    writeStream.WriteByte(0x00);

                    if (channel.IsOpen)
                    {
                        writeStream.WriteByte(0x5a);
                    }
                    else
                    {
                        writeStream.WriteByte(0x5b);
                    }

                    writeStream.Write(portBuffer, 0, portBuffer.Length);
                    writeStream.Write(ipBuffer, 0, ipBuffer.Length);

                    // write buffer to stream
                    var writeBuffer = writeStream.ToArray();
                    stream.Write(writeBuffer, 0, writeBuffer.Length);
                    stream.Flush();
                }
            }
        }

        private void HandleSocks5(Socket socket, IChannelDirectTcpip channel)
        {
            using (var stream = new NetworkStream(socket))
            {
                var authenticationMethodsCount = stream.ReadByte();

                var authenticationMethods = new byte[authenticationMethodsCount];
                stream.Read(authenticationMethods, 0, authenticationMethods.Length);

                if (authenticationMethods.Min() == 0)
                {
                    stream.Write(new byte[] { 0x05, 0x00 }, 0, 2);
                }
                else
                {
                    stream.Write(new byte[] { 0x05, 0xFF }, 0, 2);
                }

                var version = stream.ReadByte();

                if (version != 5)
                    throw new ProxyException("SOCKS5: Version 5 is expected.");

                var commandCode = stream.ReadByte();

                if (stream.ReadByte() != 0)
                {
                    throw new ProxyException("SOCKS5: 0 is expected.");
                }

                var addressType = stream.ReadByte();

                IPAddress ipAddress;
                byte[] addressBuffer;
                switch (addressType)
                {
                    case 0x01:
                        {
                            addressBuffer = new byte[4];
                            stream.Read(addressBuffer, 0, 4);

                            ipAddress = new IPAddress(addressBuffer);
                        }
                        break;
                    case 0x03:
                        {
                            var length = stream.ReadByte();
                            addressBuffer = new byte[length];
                            stream.Read(addressBuffer, 0, addressBuffer.Length);

                            ipAddress = IPAddress.Parse(SshData.Ascii.GetString(addressBuffer));

                            //var hostName = new Common.ASCIIEncoding().GetString(addressBuffer);

                            //ipAddress = Dns.GetHostEntry(hostName).AddressList[0];
                        }
                        break;
                    case 0x04:
                        {
                            addressBuffer = new byte[16];
                            stream.Read(addressBuffer, 0, 16);

                            ipAddress = new IPAddress(addressBuffer);
                        }
                        break;
                    default:
                        throw new ProxyException(string.Format("SOCKS5: Address type '{0}' is not supported.", addressType));
                }

                var portBuffer = new byte[2];
                stream.Read(portBuffer, 0, portBuffer.Length);
                var port = (uint)(portBuffer[0] * 256 + portBuffer[1]);
                var host = ipAddress.ToString();

                RaiseRequestReceived(host, port);

                channel.Open(host, port, this, socket);

                using (var writeStream = new MemoryStream())
                {
                    writeStream.WriteByte(0x05);

                    if (channel.IsOpen)
                    {
                        writeStream.WriteByte(0x00);
                    }
                    else
                    {
                        writeStream.WriteByte(0x01);
                    }

                    writeStream.WriteByte(0x00);

                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        writeStream.WriteByte(0x01);
                    }
                    else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        writeStream.WriteByte(0x04);
                    }
                    else
                    {
                        throw new NotSupportedException("Not supported address family.");
                    }

                    var addressBytes = ipAddress.GetAddressBytes();
                    writeStream.Write(addressBytes, 0, addressBytes.Length);
                    writeStream.Write(portBuffer, 0, portBuffer.Length);

                    // write buffer to stream
                    var writeBuffer = writeStream.ToArray();
                    stream.Write(writeBuffer, 0, writeBuffer.Length);
                    stream.Flush();
                }
            }
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
            RaiseExceptionEvent(e.Exception);
        }

        private static string ReadString(Stream stream)
        {
            var text = new StringBuilder();
            while (true)
            {
                var byteRead = stream.ReadByte();
                if (byteRead == 0)
                {
                    // end of the string
                    break;
                }

                if (byteRead == -1)
                {
                    // the client shut down the socket
                    break;
                }

                var c = (char) byteRead;
                text.Append(c);
            }
            return text.ToString();
        }
    }
}
