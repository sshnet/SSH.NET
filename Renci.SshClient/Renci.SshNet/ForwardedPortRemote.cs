using System;
using System.Diagnostics;
using System.Threading;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Common;
using System.Globalization;
using System.Net;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    public partial class ForwardedPortRemote : ForwardedPort, IDisposable
    {
        private bool _requestStatus;

        private EventWaitHandle _globalRequestResponse = new AutoResetEvent(false);
        private int _pendingRequests;
        private bool _isStarted;

        /// <summary>
        /// Gets or sets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
        /// </value>
        public override bool IsStarted
        {
            get { return _isStarted; }
        }

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public IPAddress BoundHostAddress { get; private set; }

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public string BoundHost
        {
            get
            {
                return BoundHostAddress.ToString();
            }
        }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; private set; }

        /// <summary>
        /// Gets the forwarded host.
        /// </summary>
        public IPAddress HostAddress { get; private set; }

        /// <summary>
        /// Gets the forwarded host.
        /// </summary>
        public string Host
        {
            get
            {
                return HostAddress.ToString();
            }
        }

        /// <summary>
        /// Gets the forwarded port.
        /// </summary>
        public uint Port { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortRemote" /> class.
        /// </summary>
        /// <param name="boundHostAddress">The bound host address.</param>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="hostAddress">The host address.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="boundHostAddress"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="hostAddress"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortRemote(IPAddress boundHostAddress, uint boundPort, IPAddress hostAddress, uint port)
        {
            if (boundHostAddress == null)
                throw new ArgumentNullException("boundHostAddress");
            if (hostAddress == null)
                throw new ArgumentNullException("hostAddress");

            boundPort.ValidatePort("boundPort");
            port.ValidatePort("port");

            BoundHostAddress = boundHostAddress;
            BoundPort = boundPort;
            HostAddress = hostAddress;
            Port = port;
        }

        /// <summary>
        /// Starts remote port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            Session.RegisterMessage("SSH_MSG_REQUEST_FAILURE");
            Session.RegisterMessage("SSH_MSG_REQUEST_SUCCESS");
            Session.RegisterMessage("SSH_MSG_CHANNEL_OPEN");

            Session.RequestSuccessReceived += Session_RequestSuccess;
            Session.RequestFailureReceived += Session_RequestFailure;
            Session.ChannelOpenReceived += Session_ChannelOpening;

            // send global request to start direct tcpip
            Session.SendMessage(new GlobalRequestMessage(GlobalRequestName.TcpIpForward, true, BoundHost, BoundPort));
            // wat for response on global request to start direct tcpip
            Session.WaitOnHandle(_globalRequestResponse);

            if (!_requestStatus)
            {
                // when the request to start port forward was rejected, then we're no longer
                // interested in these events
                Session.RequestSuccessReceived -= Session_RequestSuccess;
                Session.RequestFailureReceived -= Session_RequestFailure;
                Session.ChannelOpenReceived -= Session_ChannelOpening;

                throw new SshException(string.Format(CultureInfo.CurrentCulture, "Port forwarding for '{0}' port '{1}' failed to start.", Host, Port));
            }

            _isStarted = true;
        }

        /// <summary>
        /// Stops remote port forwarding.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for pending requests to finish processing.</param>
        protected override void StopPort(TimeSpan timeout)
        {
            // if the port not started, then there's nothing to stop
            if (!IsStarted)
                return;

            // mark forwarded port stopped, this also causes open of new channels to be rejected
            _isStarted = false;

            base.StopPort(timeout);

            // send global request to cancel direct tcpip
            Session.SendMessage(new GlobalRequestMessage(GlobalRequestName.CancelTcpIpForward, true, BoundHost, BoundPort));
            // wait for response on global request to cancel direct tcpip or completion of message
            // listener loop (in which case response on global request can never be received)
            WaitHandle.WaitAny(new[] { _globalRequestResponse, Session.MessageListenerCompleted }, timeout);

            // unsubscribe from session events as either the tcpip forward is cancelled at the
            // server, or our session message loop has completed
            Session.RequestSuccessReceived -= Session_RequestSuccess;
            Session.RequestFailureReceived -= Session_RequestFailure;
            Session.ChannelOpenReceived -= Session_ChannelOpening;

            var startWaiting = DateTime.Now;

            while (true)
            {
                // break out of loop when all pending requests have been processed
                if (Interlocked.CompareExchange(ref _pendingRequests, 0, 0) == 0)
                    break;
                // determine time elapsed since waiting for pending requests to finish
                var elapsed = DateTime.Now - startWaiting;
                // break out of loop when specified timeout has elapsed
                if (elapsed >= timeout && timeout != SshNet.Session.InfiniteTimeSpan)
                    break;
                // give channels time to process pending requests
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Ensures the current instance is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance is disposed.</exception>
        protected override void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private void Session_ChannelOpening(object sender, MessageEventArgs<ChannelOpenMessage> e)
        {
            var channelOpenMessage = e.Message;
            var info = channelOpenMessage.Info as ForwardedTcpipChannelInfo;
            if (info != null)
            {
                //  Ensure this is the corresponding request
                if (info.ConnectedAddress == BoundHost && info.ConnectedPort == BoundPort)
                {
                    if (!_isStarted)
                    {
                        Session.SendMessage(new ChannelOpenFailureMessage(channelOpenMessage.LocalChannelNumber, "", ChannelOpenFailureMessage.AdministrativelyProhibited));
                        return;
                    }

                    ExecuteThread(() =>
                        {
                            Interlocked.Increment(ref _pendingRequests);

                            try
                            {
                                RaiseRequestReceived(info.OriginatorAddress, info.OriginatorPort);

                                using (var channel = Session.CreateChannelForwardedTcpip(channelOpenMessage.LocalChannelNumber, channelOpenMessage.InitialWindowSize, channelOpenMessage.MaximumPacketSize))
                                {
                                    channel.Exception += Channel_Exception;
                                    channel.Bind(new IPEndPoint(HostAddress, (int) Port), this);
                                    channel.Close();
                                }
                            }
                            catch (Exception exp)
                            {
                                RaiseExceptionEvent(exp);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _pendingRequests);
                            }
                        });
                }
            }
        }

        private void Channel_Exception(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            RaiseExceptionEvent(exceptionEventArgs.Exception);
        }

        private void Session_RequestFailure(object sender, EventArgs e)
        {
            _requestStatus = false;
            _globalRequestResponse.Set();
        }

        private void Session_RequestSuccess(object sender, MessageEventArgs<RequestSuccessMessage> e)
        {
            _requestStatus = true;
            if (BoundPort == 0)
            {
                BoundPort = (e.Message.BoundPort == null) ? 0 : e.Message.BoundPort.Value;
            }

            _globalRequestResponse.Set();
        }

        partial void ExecuteThread(Action action);

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (Session != null)
                    {
                        Session.RequestSuccessReceived -= Session_RequestSuccess;
                        Session.RequestFailureReceived -= Session_RequestFailure;
                        Session.ChannelOpenReceived -= Session_ChannelOpening;
                        Session = null;
                    }
                    if (_globalRequestResponse != null)
                    {
                        _globalRequestResponse.Dispose();
                        _globalRequestResponse = null;
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortRemote"/> is reclaimed by garbage collection.
        /// </summary>
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
