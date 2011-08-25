using System.Linq;
using System;
using System.Net.Sockets;
using System.Net;
using Renci.SshNet.Messages;
using Renci.SshNet.Common;
using System.Threading;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    public partial class Session
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                foreach (var m in from m in this._messagesMetadata where m.Name == messageName select m)
                {
                    m.Enabled = true; 
                    m.Activated = true;
                }
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                foreach (var m in from m in this._messagesMetadata where m.Name == messageName select m)
                {
                    m.Enabled = false;
                    m.Activated = false;
                }
            }
        }

        partial void OpenSocket()
        {
            var ep = new IPEndPoint(Dns.GetHostAddresses(this.ConnectionInfo.Host)[0], this.ConnectionInfo.Port);
            this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var socketBufferSize = 2 * MAXIMUM_PACKET_SIZE;

            this._socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, socketBufferSize);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, socketBufferSize);


            //  Connect socket with 5 seconds timeout
            var connectResult = this._socket.BeginConnect(ep, null, null);

            connectResult.AsyncWaitHandle.WaitOne(this.ConnectionInfo.Timeout);

            //  Build list of available messages while connecting
            this._messagesMetadata = (from type in this.GetType().Assembly.GetTypes()
                                      from messageAttribute in type.GetCustomAttributes(false).OfType<MessageAttribute>()
                                      select new MessageMetadata
                                      {
                                          Name = messageAttribute.Name,
                                          Number = messageAttribute.Number,
                                          Enabled = false,
                                          Activated = false,
                                          Type = type,
                                      }).ToList();

            this._socket.EndConnect(connectResult);
        }

        partial void InternalRead(int length, ref byte[] buffer)
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

        partial void Write(byte[] data)
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
    }
}
