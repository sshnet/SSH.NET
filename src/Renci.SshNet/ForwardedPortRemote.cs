using System;
using System.Threading;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Common;
using System.Globalization;
using System.Net;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    public class ForwardedPortRemote : ForwardedPort, IDisposable
    {
        private ForwardedPortStatus _status;
        private bool _requestStatus;

        private EventWaitHandle _globalRequestResponse = new AutoResetEvent(false);
        private CountdownEvent _pendingChannelCountdown;

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
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
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortRemote"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\ForwardedPortRemoteTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortRemote" language="C#" title="Remote port forwarding" />
        /// </example>
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
            : this(DnsAbstraction.GetHostAddresses(boundHost)[0],
                   boundPort,
                   DnsAbstraction.GetHostAddresses(host)[0],
                   port)
        {
        }

        /// <summary>
        /// Starts remote port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            if (!ForwardedPortStatus.ToStarting(ref _status))
                return;

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
            if (!ForwardedPortStatus.ToStopping(ref _status))
                return;

            base.StopPort(timeout);

            // send global request to cancel direct tcpip
            Session.SendMessage(new CancelTcpIpForwardGlobalRequestMessage(BoundHost, BoundPort));
            // wait for response on global request to cancel direct tcpip or completion of message
            // listener loop (in which case response on global request can never be received)
            WaitHandle.WaitAny(new[] { _globalRequestResponse, Session.MessageListenerCompleted }, timeout);

            // unsubscribe from session events as either the tcpip forward is cancelled at the
            // server, or our session message loop has completed
            Session.RequestSuccessReceived -= Session_RequestSuccess;
            Session.RequestFailureReceived -= Session_RequestFailure;
            Session.ChannelOpenReceived -= Session_ChannelOpening;

            // wait for pending channels to close
            _pendingChannelCountdown.Signal();
            
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
                    if (!IsStarted)
                    {
                        Session.SendMessage(new ChannelOpenFailureMessage(channelOpenMessage.LocalChannelNumber, "", ChannelOpenFailureMessage.AdministrativelyProhibited));
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
                                    channel.Bind(new IPEndPoint(HostAddress, (int) Port), this);
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
                                    pendingChannelCountdown.Signal();
                                }
                                catch (ObjectDisposedException)
                                {
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
            if (original != null)
            {
                original.Dispose();
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
            if (_isDisposed)
                return;

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
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortRemote"/> is reclaimed by garbage collection.
        /// </summary>
        ~ForwardedPortRemote()
        {
            Dispose(false);
        }

        #endregion
    }
}
