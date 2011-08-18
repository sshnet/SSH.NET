using System.Threading.Tasks;
using System;
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

        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }

        partial void InternalStart()
        {
            //  If port already started don't start it again
            if (this.IsStarted)
                return;

            var ep = new IPEndPoint(Dns.GetHostAddresses(this.BoundHost)[0], (int)this.BoundPort);

            this._listener = new TcpListener(ep);
            this._listener.Start();

            this._listenerTaskCompleted = new ManualResetEvent(false);
            this.ExecuteThread(() =>
            {
                try
                {
                    while (true)
                    {
                        var socket = this._listener.AcceptSocket();

                        this.ExecuteThread(() =>
                        {
                            try
                            {
                                IPEndPoint originatorEndPoint = socket.RemoteEndPoint as IPEndPoint;

                                this.RaiseRequestReceived(originatorEndPoint.Address.ToString(), (uint)originatorEndPoint.Port);

                                var channel = this.Session.CreateChannel<ChannelDirectTcpip>();

                                channel.Bind(this.Host, this.Port, socket);
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

            this._listener.Stop();
            this._listenerTaskCompleted.WaitOne(this.Session.ConnectionInfo.Timeout);
            this._listenerTaskCompleted.Dispose();
            this._listenerTaskCompleted = null;

            this.IsStarted = false;
        }
    }
}
