
using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Channels;
using Renci.SshClient.Messages.Connection;
namespace Renci.SshClient
{
    public class ForwardedPortRemote : ForwardedPort, IDisposable
    {
        private bool _requestStatus;

        private EventWaitHandle _globalRequestResponse = new AutoResetEvent(false);

        public override void Start()
        {
            base.Start();

            //  If port already started dont start it again
            if (this.IsStarted)
                return;

            this.Session.RegisterMessage<RequestFailureMessage>();
            this.Session.RegisterMessage<RequestSuccessMessage>();
            this.Session.RegisterMessage<ChannelOpenMessage>();

            this.Session.RequestSuccessReceived += Session_RequestSuccess;
            this.Session.RequestFailureReceived += Session_RequestFailure;
            this.Session.ChannelOpenReceived += Session_ChannelOpening;

            //  Send global request to start direct tcpip
            this.Session.SendMessage(new GlobalRequestMessage(GlobalRequestNames.TcpIpForward, true, this.ConnectedHost, this.BoundPort));

            this.Session.WaitHandle(this._globalRequestResponse);

            if (!this._requestStatus)
            {
                //  If request  failed dont handle channel opening for this request
                this.Session.ChannelOpenReceived -= Session_ChannelOpening;
            }

            this.IsStarted = true;
        }

        public override void Stop()
        {
            //  If port not started you cant stop it
            if (!this.IsStarted)
                return;

            //  Send global request to cancel direct tcpip
            this.Session.SendMessage(new GlobalRequestMessage(GlobalRequestNames.CancelTcpIpForward, true, this.ConnectedHost, this.BoundPort));

            this.Session.WaitHandle(this._globalRequestResponse);

            this.Session.RequestSuccessReceived -= Session_RequestSuccess;
            this.Session.RequestFailureReceived -= Session_RequestFailure;
            this.Session.ChannelOpenReceived -= Session_ChannelOpening;

            this.IsStarted = false;
        }

        private void Session_ChannelOpening(object sender, MessageEventArgs<ChannelOpenMessage> e)
        {
            //  Ensure that this is corresponding request
            var info = e.Message.Info as ForwardedTcpipChannelInfo;
            if (info != null)
            {
                if (info.ConnectedAddress == this.ConnectedHost && info.ConnectedPort == this.BoundPort)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var channel = this.Session.CreateChannel<ChannelForwardedTcpip>(e.Message.LocalChannelNumber, e.Message.InitialWindowSize, e.Message.MaximumPacketSize);
                            channel.Bind(this.ConnectedHost, this.ConnectedPort);
                        }
                        catch (Exception exp)
                        {
                            this.RaiseExceptionEvent(exp);
                        }
                    });
                }
            }
        }

        private void Session_RequestFailure(object sender, System.EventArgs e)
        {
            this._requestStatus = false;
            this._globalRequestResponse.Set();
        }

        private void Session_RequestSuccess(object sender, MessageEventArgs<RequestSuccessMessage> e)
        {
            this._requestStatus = true;
            if (this.BoundPort == 0)
            {
                this.BoundPort = (e.Message.BoundPort == null) ? 0 : e.Message.BoundPort.Value;
            }

            this._globalRequestResponse.Set();
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._globalRequestResponse != null)
                    {
                        this._globalRequestResponse.Dispose();
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        ~ForwardedPortRemote()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
