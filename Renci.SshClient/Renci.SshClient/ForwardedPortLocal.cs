using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Renci.SshClient.Channels;

namespace Renci.SshClient
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public class ForwardedPortLocal : ForwardedPort
    {
        private TcpListener _listener;

        private Task _listenerTask;

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        public override void Start()
        {
            base.Start();

            //  If port already started dont start it again
            if (this.IsStarted)
                return;

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

                this.Stop();
            });

            this.IsStarted = true;
        }

        /// <summary>
        /// Stops local port forwarding.
        /// </summary>
        public override void Stop()
        {
            //  If port not started you cant stop it
            if (!this.IsStarted)
                return;

            this._listener.Stop();
            this._listenerTask.Wait();

            this.IsStarted = false;
        }
    }
}
