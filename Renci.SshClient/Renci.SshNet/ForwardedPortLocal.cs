using System;
using System.Net;
using System.Net.Sockets;
using Renci.SshNet.Channels;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal : ForwardedPort, IDisposable
    {
        private TcpListener _listener;

        private EventWaitHandle _listenerTaskCompleted;

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
            //  TODO:   Add timeout to WaitOne method
            this._listenerTaskCompleted.WaitOne();
            this._listenerTaskCompleted.Dispose();
            this._listenerTaskCompleted = null;

            this.IsStarted = false;
        }

        partial void ExecuteThread(Action action);

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
                    if (this._listenerTaskCompleted != null)
                    {
                        this._listenerTaskCompleted.Dispose();
                        this._listenerTaskCompleted = null;
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
