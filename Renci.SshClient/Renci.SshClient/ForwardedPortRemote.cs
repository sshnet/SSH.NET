
using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Channels;
using Renci.SshClient.Messages.Connection;
namespace Renci.SshClient
{
    public class ForwardedPortRemote : ForwardedPort
    {
        private bool _requestStatus;

        private EventWaitHandle _globalRequestResponse = new AutoResetEvent(false);

        public override void Start()
        {
            base.Start();

            this.Session.RegisterMessageType<RequestFailureMessage>(Messages.MessageTypes.RequestFailure);
            this.Session.RegisterMessageType<RequestSuccessMessage>(Messages.MessageTypes.RequestSuccess);
            this.Session.RegisterMessageType<ChannelOpenMessage>(Messages.MessageTypes.ChannelOpen);

            this.Session.RequestSuccess += Session_RequestSuccess;
            this.Session.RequestFailure += Session_RequestFailure;
            this.Session.ChannelOpening += Session_ChannelOpening;

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
                this.Session.ChannelOpening -= Session_ChannelOpening;
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

            this.Session.RequestSuccess -= Session_RequestSuccess;
            this.Session.RequestFailure -= Session_RequestFailure;
            this.Session.ChannelOpening -= Session_ChannelOpening;
        }

        private void Session_ChannelOpening(object sender, ChannelOpeningEventArgs e)
        {
            //  Ensure that this is corresponding request
            if (e.Message.ConnectedAddress == this.ConnectedHost && e.Message.ConnectedPort == this.BoundPort)
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

        private void Session_RequestFailure(object sender, System.EventArgs e)
        {
            this._requestStatus = false;
            this._globalRequestResponse.Set();
        }

        private void Session_RequestSuccess(object sender, RequestSuccessEventArgs e)
        {
            this._requestStatus = true;
            if (this.BoundPort == 0)
            {
                this.BoundPort = e.BoundPort;
            }

            this._globalRequestResponse.Set();
        }
    }
}
