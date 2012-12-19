using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Renci.SshNet.Channels;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal
    {
        private TcpListener _listener;
        private object _listenerLocker = new object();

        partial void InternalStart()
        {
            //  If port already started don't start it again
            if (this.IsStarted)
                return;

            IPAddress addr;
            if (!IPAddress.TryParse(this.BoundHost, out addr))
                addr = Dns.GetHostAddresses(this.BoundHost).First();

            var ep = new IPEndPoint(addr, (int)this.BoundPort); 

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
                                IPEndPoint originatorEndPoint = socket.RemoteEndPoint as IPEndPoint;

                                this.RaiseRequestReceived(originatorEndPoint.Address.ToString(), (uint)originatorEndPoint.Port);

                                var channel = this.Session.CreateChannel<ChannelDirectTcpip>();

                                channel.Open(this.Host, this.Port, socket);

                                channel.Bind();
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
    }
}
