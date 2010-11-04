
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Renci.SshClient.Channels;
namespace Renci.SshClient
{
    public class ForwardedPortLocal : ForwardedPort
    {
        private TcpListener _listener;

        private Task _listenerTask;

        public override void Start()
        {
            base.Start();

            var ep = new IPEndPoint(Dns.GetHostAddresses("localhost")[0], (int)this.BoundPort);
            this._listener = new TcpListener(ep);
            this._listener.Start();

            this._listenerTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        var socket = this._listener.AcceptSocket();

                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                var channel = this.Session.CreateChannel<ChannelDirectTcpip>();
                                channel.Bind(this.ConnectedHost, this.ConnectedPort, socket);
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
            });
        }

        public override void Stop()
        {
            this._listener.Stop();
            this._listenerTask.Wait();
        }
    }
}
