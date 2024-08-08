using System;
using System.Globalization;
using System.Net;
using System.Threading;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for remote port forwarding.
    /// </summary>
    public class ForwardedPortRemote : ForwardedPort
    {
        private ForwardedPortStatus _status;
        private bool _requestStatus;
        private EventWaitHandle _globalRequestResponse = new AutoResetEvent(initialState: false);
        private CountdownEvent _pendingChannelCountdown;
        private bool _isDisposed;

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if port forwarding is started; otherwise, <see langword="false"/>.
        /// </value>
        public override bool IsStarted
        {
            get { return _status == ForwardedPortStatus.Started; }
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
        /// <exception cref="ArgumentNullException"><paramref name="boundHostAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="hostAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortRemote(IPAddress boundHostAddress, uint boundPort, IPAddress hostAddress, uint port)
        {
            if (boundHostAddress is null)
            {
                throw new ArgumentNullException(nameof(boundHostAddress));
            }

            if (hostAddress is null)
            {
                throw new ArgumentNullException(nameof(hostAddress));
            }

            boundPort.ValidatePort("boundPort");
            port.ValidatePort("port");

            BoundHostAddress = boundHostAddress;
            BoundPort = boundPort;
            HostAddress = hostAddress;
            Port = port;
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortRemote"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public ForwardedPortRemote(uint boundPort, string host, uint port)
            : this(string.Empty, boundPort, host, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortRemote"/> class.
        /// </summary>
        /// <param name="boundHost">The bound host.</param>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public ForwardedPortRemote(string boundHost, uint boundPort, string host, uint port)
            : this(Dns.GetHostAddresses(boundHost)[0],
                   boundPort,
                   Dns.GetHostAddresses(host)[0],
                   port)
        {
        }

        /// <summary>
        /// Starts remote port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            if (!ForwardedPortStatus.ToStarting(ref _status))
            {
                return;
            }

            InitializePendingChannelCountdown();

            try
            {
                Session.RegisterMessage("SSH_MSG_REQUEST_FAILURE");
                Session.RegisterMessage("SSH_MSG_REQUEST_SUCCESS");
                Session.RegisterMessage("SSH_MSG_CHANNEL_OPEN");

                Session.RequestSuccessReceived += Session_RequestSuccess;
                Session.RequestFailureReceived += Session_RequestFailure;
                Session.ChannelOpenReceived += Session_ChannelOpening;

                // send global request to start forwarding
                Session.SendMessage(new TcpIpForwardGlobalRequestMessage(BoundHost, BoundPort));

                // wat for response on global request to start direct tcpip
                Session.WaitOnHandle(_globalRequestResponse);

                if (!_requestStatus)
                {
                    throw new SshException(string.Format(CultureInfo.CurrentCulture, "Port forwarding for '{0}' port '{1}' failed to start.", Host, Port));
                }
            }
            catch (Exception)
            {
                // mark port stopped
                _status = ForwardedPortStatus.Stopped;

                // when the request to start port forward was rejected or failed, then we're no longer
                // interested in these events
                Session.RequestSuccessReceived -= Session_RequestSuccess;
                Session.RequestFailureReceived -= Session_RequestFailure;
                Session.ChannelOpenReceived -= Session_ChannelOpening;

                throw;
            }

            _status = ForwardedPortStatus.Started;
        }

        /// <summary>
        /// Stops remote port forwarding.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for the port to stop.</param>
        protected override void StopPort(TimeSpan timeout)
        {
            timeout.EnsureValidTimeout(nameof(timeout));

            if (!ForwardedPortStatus.ToStopping(ref _status))
            {
                return;
            }

            base.StopPort(timeout);

            // send global request to cancel direct tcpip
            Session.SendMessage(new CancelTcpIpForwardGlobalRequestMessage(BoundHost, BoundPort));

            // wait for response on global request to cancel direct tcpip or completion of message
            // listener loop (in which case response on global request can never be received)
            _ = WaitHandle.WaitAny(new[] { _globalRequestResponse, Session.MessageListenerCompleted }, timeout);

            // unsubscribe from session events as either the tcpip forward is cancelled at the
            // server, or our session message loop has completed
            Session.RequestSuccessReceived -= Session_RequestSuccess;
            Session.RequestFailureReceived -= Session_RequestFailure;
            Session.ChannelOpenReceived -= Session_ChannelOpening;

            // wait for pending channels to close
            _ = _pendingChannelCountdown.Signal();

            if (!_pendingChannelCountdown.Wait(timeout))
            {
                // TODO: log as warning
                DiagnosticAbstraction.Log("Timeout waiting for pending channels in remote forwarded port to close.");
            }

            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Ensures the current instance is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance is disposed.</exception>
        protected override void CheckDisposed()
        {
            ThrowHelper.ThrowObjectDisposedIf(_isDisposed, this);
        }

        private void Session_ChannelOpening(object sender, MessageEventArgs<ChannelOpenMessage> e)
        {
            var channelOpenMessage = e.Message;
            if (channelOpenMessage.Info is ForwardedTcpipChannelInfo info)
            {
                // Ensure this is the corresponding request
                if (info.ConnectedAddress == BoundHost && info.ConnectedPort == BoundPort)
                {
                    if (!IsStarted)
                    {
                        Session.SendMessage(new ChannelOpenFailureMessage(channelOpenMessage.LocalChannelNumber,
                                                                          string.Empty,
                                                                          ChannelOpenFailureMessage.AdministrativelyProhibited));
                        return;
                    }

                    ThreadAbstraction.ExecuteThread(() =>
                        {
                            // capture the countdown event that we're adding a count to, as we need to make sure that we'll be signaling
                            // that same instance; the instance field for the countdown event is re-initialize when the port is restarted
                            // and that time there may still be pending requests
                            var pendingChannelCountdown = _pendingChannelCountdown;

                            pendingChannelCountdown.AddCount();

                            try
                            {
                                RaiseRequestReceived(info.OriginatorAddress, info.OriginatorPort);

                                using (var channel = Session.CreateChannelForwardedTcpip(channelOpenMessage.LocalChannelNumber, channelOpenMessage.InitialWindowSize, channelOpenMessage.MaximumPacketSize))
                                {
                                    channel.Exception += Channel_Exception;
                                    channel.Bind(new IPEndPoint(HostAddress, (int)Port), this);
                                }
                            }
                            catch (Exception exp)
                            {
                                RaiseExceptionEvent(exp);
                            }
                            finally
                            {
                                // take into account that CountdownEvent has since been disposed; when stopping the port we
                                // wait for a given time for the channels to close, but once that timeout period has elapsed
                                // the CountdownEvent will be disposed
                                try
                                {
                                    _ = pendingChannelCountdown.Signal();
                                }
                                catch (ObjectDisposedException)
                                {
                                    // Ignore any ObjectDisposedException
                                }
                            }
                        });
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="CountdownEvent"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the port is started for the first time, a <see cref="CountdownEvent"/> is created with an initial count
        /// of <c>1</c>.
        /// </para>
        /// <para>
        /// On subsequent (re)starts, we'll dispose the current <see cref="CountdownEvent"/> and create a new one with
        /// initial count of <c>1</c>.
        /// </para>
        /// </remarks>
        private void InitializePendingChannelCountdown()
        {
            var original = Interlocked.Exchange(ref _pendingChannelCountdown, new CountdownEvent(1));
            original?.Dispose();
        }

        private void Channel_Exception(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            RaiseExceptionEvent(exceptionEventArgs.Exception);
        }

        private void Session_RequestFailure(object sender, EventArgs e)
        {
            _requestStatus = false;
            _ = _globalRequestResponse.Set();
        }

        private void Session_RequestSuccess(object sender, MessageEventArgs<RequestSuccessMessage> e)
        {
            _requestStatus = true;

            if (BoundPort == 0)
            {
                BoundPort = (e.Message.BoundPort is null) ? 0 : e.Message.BoundPort.Value;
            }

            _ = _globalRequestResponse.Set();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            base.Dispose(disposing);

            if (disposing)
            {
                var session = Session;
                if (session != null)
                {
                    Session = null;
                    session.RequestSuccessReceived -= Session_RequestSuccess;
                    session.RequestFailureReceived -= Session_RequestFailure;
                    session.ChannelOpenReceived -= Session_ChannelOpening;
                }

                var globalRequestResponse = _globalRequestResponse;
                if (globalRequestResponse != null)
                {
                    _globalRequestResponse = null;
                    globalRequestResponse.Dispose();
                }

                var pendingRequestsCountdown = _pendingChannelCountdown;
                if (pendingRequestsCountdown != null)
                {
                    _pendingChannelCountdown = null;
                    pendingRequestsCountdown.Dispose();
                }
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ForwardedPortRemote"/> class.
        /// </summary>
        ~ForwardedPortRemote()
        {
            Dispose(disposing: false);
        }
    }
}
