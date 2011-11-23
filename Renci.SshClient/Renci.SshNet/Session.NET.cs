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

        partial void SocketConnect()
        {
            var ep = new IPEndPoint(Dns.GetHostAddresses(this.ConnectionInfo.Host)[0], this.ConnectionInfo.Port);
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
            //  Get server version from the server,
            //  ignore text lines which are sent before if any
            using (var ns = new NetworkStream(this._socket))
            {
                using (var sr = new StreamReader(ns))
                {
                    response = sr.ReadLine();
                }
            }
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
