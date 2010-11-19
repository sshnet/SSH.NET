
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

            this.Session.RegisterMessageType<RequestFailureMessage>(Messages.MessageTypes.RequestFailure);
            this.Session.RegisterMessageType<RequestSuccessMessage>(Messages.MessageTypes.RequestSuccess);
            this.Session.RegisterMessageType<ChannelOpenMessage>(Messages.MessageTypes.ChannelOpen);

            this.Session.RequestSuccessReceived += Session_RequestSuccess;
            this.Session.RequestFailureReceived += Session_RequestFailure;
            this.Session.ChannelOpenReceived += Session_ChannelOpening;

            //  Send global request to start direct tcpip
            this.Session.SendMessage(new GlobalRequestMessage
            {
                RequestName = GlobalRequestNames.TcpIpForward,
                WantReply = true,
                AddressToBind = this.ConnectedHost,
                PortToBind = this.BoundPort,
            });

            this.Session.WaitHandle(this._globalRequestResponse);

            if (!this._requestStatus)
            {
                //  If request  failed dont handle channel opening for this request
                this.Session.ChannelOpenReceived -= Session_ChannelOpening;
            }
        }

        public override void Stop()
        {
            //  Send global request to cancel direct tcpip
            this.Session.SendMessage(new GlobalRequestMessage
            {
                RequestName = GlobalRequestNames.CancelTcpIpForward,
                WantReply = true,
                AddressToBind = this.ConnectedHost,
                PortToBind = this.BoundPort,
            });

            this.Session.WaitHandle(this._globalRequestResponse);

            this.Session.RequestSuccessReceived -= Session_RequestSuccess;
            this.Session.RequestFailureReceived -= Session_RequestFailure;
            this.Session.ChannelOpenReceived -= Session_ChannelOpening;
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

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
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
                disposed = true;
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
