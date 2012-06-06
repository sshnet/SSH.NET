using System.Linq;
using System;
using System.Net.Sockets;
using System.Net;
using Renci.SshNet.Messages;
using Renci.SshNet.Common;
using System.Threading;
using Renci.SshNet.Messages.Transport;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace Renci.SshNet
{
    public partial class Session
    {
        private TraceSource _log =
#if DEBUG
 new TraceSource("SshNet.Logging", SourceLevels.All);
#else
            new TraceSource("SshNet.Logging");
#endif

        partial void SocketConnect(string host, int port)
        {
            var ep = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);
            this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var socketBufferSize = 2 * MAXIMUM_PACKET_SIZE;

            this._socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, socketBufferSize);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, socketBufferSize);

            this.Log(string.Format("Initiating connect to '{0}:{1}'.", this.ConnectionInfo.Host, this.ConnectionInfo.Port));

            //  Connect socket with specified timeout
            var connectResult = this._socket.BeginConnect(ep, null, null);

            connectResult.AsyncWaitHandle.WaitOne(this.ConnectionInfo.Timeout, false);

            this._socket.EndConnect(connectResult);
        }

        partial void SocketDisconnect()
        {
            this._socket.Disconnect(true);
        }

        partial void SocketReadLine(ref string response)
        {
            var encoding = new Renci.SshNet.Common.ASCIIEncoding();

            var line = new StringBuilder();
            //  Read data one byte at a time to find end of line and leave any unhandled information in the buffer to be processed later
            var buffer = new List<byte>();

            var data = new byte[1];
            do
            {
                var received = this._socket.Receive(data);

                //  If zero bytes received then exit
                if (received == 0)
                    break;

                buffer.Add(data[0]);
            }
            while (!(buffer.Count > 1 && buffer[buffer.Count - 1] == 0x0A));

            if (buffer.Count > 1 && buffer[buffer.Count - 2] == 0x0D)
                response = encoding.GetString(buffer.Take(buffer.Count - 2).ToArray());
            else
                response = encoding.GetString(buffer.Take(buffer.Count - 1).ToArray());
        }

        partial void SocketRead(int length, ref byte[] buffer)
        {
            var offset = 0;
            int receivedTotal = 0;  // how many bytes is already received

            do
            {
                try
                {
                    var receivedBytes = this._socket.Receive(buffer, offset + receivedTotal, length - receivedTotal, SocketFlags.None);
                    if (receivedBytes > 0)
                    {
                        receivedTotal += receivedBytes;
                        continue;
                    }
                    else
                    {
                        throw new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);
                    }
                }
                catch (SocketException exp)
                {
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
            int sent = 0;  // how many bytes is already sent
            int length = data.Length;

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

        partial void Log(string text)
        {
            this._log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 1, text);
        }
    }
}
