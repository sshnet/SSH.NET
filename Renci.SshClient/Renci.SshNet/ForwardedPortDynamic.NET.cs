using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    public partial class ForwardedPortDynamic
    {
        private TcpListener _listener;
        private readonly object _listenerLocker = new object();

        partial void InternalStart()
        {
            //  If port already started don't start it again
            if (this.IsStarted)
                return;

            var ip = IPAddress.Any;
            if (!string.IsNullOrEmpty(this.BoundHost))
            {
                ip = this.BoundHost.GetIPAddress();
            }

            var ep = new IPEndPoint(ip, (int)this.BoundPort);

            this._listener = new TcpListener(ep);
            this._listener.Start();

            this._listenerTaskCompleted = new ManualResetEvent(false);
            this.ExecuteThread(() =>
            {
                try
                {
                    while (true)
                    {
                        lock (this._listenerLocker)
                        {
                            if (this._listener == null)
                                break;
                        }

                        var socket = this._listener.AcceptSocket();

                        this.ExecuteThread(() =>
                        {
                            try
                            {
                                using (var channel = this.Session.CreateClientChannel<ChannelDirectTcpip>())
                                {
                                    var version = new byte[1];

                                    socket.Receive(version);

                                    if (version[0] == 4)
                                    {
                                        this.HandleSocks4(socket, channel);
                                    }
                                    else if (version[0] == 5)
                                    {
                                        this.HandleSocks5(socket, channel);
                                    }
                                    else
                                    {
                                        throw new NotSupportedException(string.Format("SOCKS version {0} is not supported.", version));
                                    }

                                    channel.Bind();

                                    channel.Close();
                                }
                            }
                            catch (Exception exp)
                            {
                                this.RaiseExceptionEvent(exp);
                            }
                        });
                    }
                }
                catch (SocketException exp)
                {
                    if (!(exp.SocketErrorCode == SocketError.Interrupted))
                    {
                        this.RaiseExceptionEvent(exp);
                    }
                }
                catch (Exception exp)
                {
                    this.RaiseExceptionEvent(exp);
                }
                finally
                {
                    this._listenerTaskCompleted.Set();
                }
            });

            this.IsStarted = true;
        }

        partial void InternalStop()
        {
            //  If port not started you cant stop it
            if (!this.IsStarted)
                return;

            lock (this._listenerLocker)
            {
                this._listener.Stop();
                this._listener = null;
            }
            this._listenerTaskCompleted.WaitOne(this.Session.ConnectionInfo.Timeout);
            this._listenerTaskCompleted.Dispose();
            this._listenerTaskCompleted = null;
            this.IsStarted = false;
        }

        private void HandleSocks4(Socket socket, ChannelDirectTcpip channel)
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

                this.RaiseRequestReceived(host, port);

                channel.Open(host, port, socket);

                stream.WriteByte(0x00);

                if (channel.IsOpen)
                {
                    stream.WriteByte(0x5a);
                }
                else
                {
                    stream.WriteByte(0x5b);
                }

                stream.Write(portBuffer, 0, portBuffer.Length);
                stream.Write(ipBuffer, 0, ipBuffer.Length);
            }
        }

        private void HandleSocks5(Socket socket, ChannelDirectTcpip channel)
        {
            using (var stream = new NetworkStream(socket))
            {
                var authenticationMethodsCount = stream.ReadByte();

                var authenticationMethods = new byte[authenticationMethodsCount];
                stream.Read(authenticationMethods, 0, authenticationMethods.Length);

                stream.WriteByte(0x05);

                if (authenticationMethods.Min() == 0)
                {
                    stream.WriteByte(0x00);
                }
                else
                {
                    stream.WriteByte(0xFF);
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

                            ipAddress = IPAddress.Parse(new Common.ASCIIEncoding().GetString(addressBuffer));
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

                this.RaiseRequestReceived(host, port);

                channel.Open(host, port, socket);

                stream.WriteByte(0x05);

                if (channel.IsOpen)
                {
                    stream.WriteByte(0x00);
                }
                else
                {
                    stream.WriteByte(0x01);
                }

                stream.WriteByte(0x00);

                var buffer = ipAddress.GetAddressBytes();

                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    stream.WriteByte(0x01);
                }
                else if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    stream.WriteByte(0x04);
                }
                else
                {
                    throw new NotSupportedException("Not supported address family.");
                }

                stream.Write(buffer, 0, buffer.Length);
                stream.Write(portBuffer, 0, portBuffer.Length);
            }
        }

        private static string ReadString(NetworkStream stream)
        {
            StringBuilder text = new StringBuilder();
            var aa = (char)stream.ReadByte();
            while (aa != 0)
            {
                text.Append(aa);
                aa = (char)stream.ReadByte();
            }
            return text.ToString();
        }
    }
}
