using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Renci.SshNet.Channels;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public class ForwardedPortLocal : ForwardedPort, IDisposable
    {
        private TcpListener _listener;

        private Task _listenerTask;

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        public override void Start()
        {
            //  If port already started don't start it again
            if (this.IsStarted)
                return;

            var ep = new IPEndPoint(Dns.GetHostAddresses(this.BoundHost)[0], (int)this.BoundPort);

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

                this.Stop();
            });

            this.IsStarted = true;
        }

        /// <summary>
        /// Stops local port forwarding.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            //  If port not started you cant stop it
            if (!this.IsStarted)
                return;

            this._listener.Stop();
            this._listenerTask.Wait();

            this.IsStarted = false;
        }


        #region IDisposable Members

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    // Dispose managed ResourceMessages.
                    if (this._listenerTask != null)
                    {
                        this._listenerTask.Dispose();
                        this._listenerTask = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Channel"/> is reclaimed by garbage collection.
        /// </summary>
        ~ForwardedPortLocal()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
