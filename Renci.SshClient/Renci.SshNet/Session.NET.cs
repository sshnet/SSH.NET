using System.Linq;
using System;
using System.Net.Sockets;
using System.Net;
using Renci.SshNet.Common;
using System.Threading;
using Renci.SshNet.Messages.Transport;
using System.Diagnostics;
using System.Collections.Generic;

namespace Renci.SshNet
{
    public partial class Session
    {
        private readonly TraceSource _log =
#if DEBUG
            new TraceSource("SshNet.Logging", SourceLevels.All);
#else
            new TraceSource("SshNet.Logging");
#endif

        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if the socket is connected; otherwise, <c>false</c>.
        /// </value>
        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (_socket != null && _socket.Connected);
            if (isConnected)
            {
                var connectionClosedOrDataAvailable = _socket.Poll(1000, SelectMode.SelectRead);
                isConnected = !(connectionClosedOrDataAvailable && _socket.Available == 0);
            }
        }

        partial void SocketConnect(string host, int port)
        {
            const int socketBufferSize = 2 * MaximumSshPacketSize;

            var addr = host.GetIPAddress();

            var ep = new IPEndPoint(addr, port);
            this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            this._socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, socketBufferSize);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, socketBufferSize);

            this.Log(string.Format("Initiating connect to '{0}:{1}'.", this.ConnectionInfo.Host, this.ConnectionInfo.Port));

            //  Connect socket with specified timeout
            var connectResult = this._socket.BeginConnect(ep, null, null);

            if (!connectResult.AsyncWaitHandle.WaitOne(this.ConnectionInfo.Timeout, false))
            {
                throw new SshOperationTimeoutException("Connection Could Not Be Established");
            }

            this._socket.EndConnect(connectResult);
        }

        partial void SocketDisconnect()
        {
            _socket.Disconnect(true);
        }

        partial void SocketReadLine(ref string response)
        {
            var encoding = new ASCIIEncoding();

            //  Read data one byte at a time to find end of line and leave any unhandled information in the buffer to be processed later
            var buffer = new List<byte>();

            var data = new byte[1];
            do
            {
                var asyncResult = this._socket.BeginReceive(data, 0, data.Length, SocketFlags.None, null, null);

                if (!asyncResult.AsyncWaitHandle.WaitOne(this.ConnectionInfo.Timeout))
                    throw new SshOperationTimeoutException("Socket read operation has timed out");

                var received = this._socket.EndReceive(asyncResult);

                //  If zero bytes received then exit
                if (received == 0)
                    break;

                buffer.Add(data[0]);
            }
            while (!(buffer.Count > 0 && (buffer[buffer.Count - 1] == 0x0A || buffer[buffer.Count - 1] == 0x00)));

            // Return an empty version string if the buffer consists of a 0x00 character.
            if (buffer.Count > 0 && buffer[buffer.Count - 1] == 0x00)
            {
                response = string.Empty;
            }
            else if (buffer.Count > 1 && buffer[buffer.Count - 2] == 0x0D)
                response = encoding.GetString(buffer.Take(buffer.Count - 2).ToArray());
            else
                response = encoding.GetString(buffer.Take(buffer.Count - 1).ToArray());
        }

        /// <summary>
        /// Function to read <paramref name="length"/> amount of data before returning, or throwing an exception.
        /// </summary>
        /// <param name="length">The amount wanted.</param>
        /// <param name="buffer">The buffer to read to.</param>
        /// <exception cref="SshConnectionException">Happens when the socket is closed.</exception>
        /// <exception cref="Exception">Unhandled exception.</exception>
        partial void SocketRead(int length, ref byte[] buffer)
        {
            var receivedTotal = 0;  // how many bytes is already received

            do
            {
                try
                {
                    var receivedBytes = this._socket.Receive(buffer, receivedTotal, length - receivedTotal, SocketFlags.None);
                    if (receivedBytes > 0)
                    {
                        receivedTotal += receivedBytes;
                        continue;
                    }

                    // 2012-09-11: Kenneth_aa
                    // When Disconnect or Dispose is called, this throws SshConnectionException(), which...
                    // 1 - goes up to ReceiveMessage() 
                    // 2 - up again to MessageListener()
                    // which is where there is a catch-all exception block so it can notify event listeners.
                    // 3 - MessageListener then again calls RaiseError().
                    // There the exception is checked for the exception thrown here (ConnectionLost), and if it matches it will not call Session.SendDisconnect().
                    //
                    // Adding a check for this._isDisconnecting causes ReceiveMessage() to throw SshConnectionException: "Bad packet length {0}".
                    //

                    if (_isDisconnecting)
                        throw new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);
                    throw new SshConnectionException("An established connection was aborted by the server.", DisconnectReason.ConnectionLost);
                }
                catch (SocketException exp)
                {
                    if (exp.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        buffer = new byte[length];
                        this.Disconnect();
                        return;
                    }

                    if (exp.SocketErrorCode == SocketError.WouldBlock ||
                        exp.SocketErrorCode == SocketError.IOPending ||
                        exp.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurred
                }
            } while (receivedTotal < length);
        }

        partial void SocketWrite(byte[] data)
        {
            var sent = 0;  // how many bytes is already sent
            var length = data.Length;

            do
            {
                try
                {
                    sent += this._socket.Send(data, sent, length - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (sent < length);
        }

        [Conditional("DEBUG")]
        partial void Log(string text)
        {
            this._log.TraceEvent(TraceEventType.Verbose, 1, text);
        }
    }
}
