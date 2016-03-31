using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements Session SSH channel.
    /// </summary>
    internal class ChannelSession : ClientChannel, IChannelSession
    {
        /// <summary>
        /// Counts failed channel open attempts
        /// </summary>
        private int _failedOpenAttempts;

        /// <summary>
        /// Holds a value indicating whether the session semaphore has been obtained by the current
        /// channel.
        /// </summary>
        /// <value>
        /// <c>0</c> when the session semaphore has not been obtained or has already been released,
        /// and <c>1</c> when the session has been obtained and still needs to be released.
        /// </value>
        private int _sessionSemaphoreObtained;

        /// <summary>
        /// Wait handle to signal when response was received to open the channel
        /// </summary>
        private EventWaitHandle _channelOpenResponseWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelRequestResponse = new ManualResetEvent(false);

        private bool _channelRequestSucces;

        /// <summary>
        /// Initializes a new <see cref="ChannelSession"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        public ChannelSession(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
        {
        }

        /// <summary>
        /// Gets the type of the channel.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        /// <summary>
        /// Opens the channel.
        /// </summary>
        public virtual void Open()
        {
            if (!IsOpen)
            {
                //  Try to open channel several times
                while (!IsOpen && _failedOpenAttempts < ConnectionInfo.RetryAttempts)
                {
                    SendChannelOpenMessage();
                    try
                    {
                        WaitOnHandle(_channelOpenResponseWaitHandle);
                    }
                    catch (Exception)
                    {
                        // avoid leaking session semaphore
                        ReleaseSemaphore();
                        throw;
                    }
                }

                if (!IsOpen)
                    throw new SshException(string.Format(CultureInfo.CurrentCulture, "Failed to open a channel after {0} attempts.", _failedOpenAttempts));
            }
        }

        /// <summary>
        /// Called when channel is opened by the server.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);
            _channelOpenResponseWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel failed to open.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="description">The description.</param>
        /// <param name="language">The language.</param>
        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            _failedOpenAttempts++;
            ReleaseSemaphore();
            _channelOpenResponseWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel is closed by the server.
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();

            //  This timeout needed since when channel is closed it does not immediately becomes available
            //  but it takes time for the server to clean up resource and allow new channels to be created.
            Thread.Sleep(100);
        }

        protected override void Close(bool wait)
        {
            base.Close(wait);

            ReleaseSemaphore();
        }

        /// <summary>
        /// Sends the pseudo terminal request.
        /// </summary>
        /// <param name="environmentVariable">The environment variable.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendPseudoTerminalRequest(string environmentVariable, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues)
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new PseudoTerminalRequestInfo(environmentVariable, columns, rows, width, height, terminalModeValues)));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the X11 forwarding request.
        /// </summary>
        /// <param name="isSingleConnection">if set to <c>true</c> the it is single connection.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="screenNumber">The screen number.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendX11ForwardingRequest(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber)
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new X11ForwardingRequestInfo(isSingleConnection, protocol, cookie, screenNumber)));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the environment variable request.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendEnvironmentVariableRequest(string variableName, string variableValue)
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new EnvironmentVariableRequestInfo(variableName, variableValue)));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the shell request.
        /// </summary>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendShellRequest()
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new ShellRequestInfo()));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendExecRequest(string command)
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new ExecRequestInfo(command, ConnectionInfo.Encoding)));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="breakLength">Length of the break.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendBreakRequest(uint breakLength)
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new BreakRequestInfo(breakLength)));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the subsystem request.
        /// </summary>
        /// <param name="subsystem">The subsystem.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendSubsystemRequest(string subsystem)
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new SubsystemRequestInfo(subsystem)));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends the window change request.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height)
        {
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new WindowChangeRequestInfo(columns, rows, width, height)));
            return true;
        }

        /// <summary>
        /// Sends the local flow request.
        /// </summary>
        /// <param name="clientCanDo">if set to <c>true</c> [client can do].</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendLocalFlowRequest(bool clientCanDo)
        {
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new XonXoffRequestInfo(clientCanDo)));
            return true;
        }

        /// <summary>
        /// Sends the signal request.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendSignalRequest(string signalName)
        {
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new SignalRequestInfo(signalName)));
            return true;
        }

        /// <summary>
        /// Sends the exit status request.
        /// </summary>
        /// <param name="exitStatus">The exit status.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendExitStatusRequest(uint exitStatus)
        {
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new ExitStatusRequestInfo(exitStatus)));
            return true;
        }

        /// <summary>
        /// Sends the exit signal request.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <param name="coreDumped">if set to <c>true</c> [core dumped].</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="language">The language.</param>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendExitSignalRequest(string signalName, bool coreDumped, string errorMessage, string language)
        {
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new ExitSignalRequestInfo(signalName, coreDumped, errorMessage, language)));
            return true;
        }

        /// <summary>
        /// Sends eow@openssh.com request.
        /// </summary>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendEndOfWriteRequest()
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new EndOfWriteRequestInfo()));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Sends keepalive@openssh.com request.
        /// </summary>
        /// <returns>
        /// <c>true</c> if request was successful; otherwise <c>false</c>.
        /// </returns>
        public bool SendKeepAliveRequest()
        {
            _channelRequestResponse.Reset();
            SendMessage(new ChannelRequestMessage(RemoteChannelNumber, new KeepAliveRequestInfo()));
            WaitOnHandle(_channelRequestResponse);
            return _channelRequestSucces;
        }

        /// <summary>
        /// Called when channel request was successful
        /// </summary>
        protected override void OnSuccess()
        {
            base.OnSuccess();
            _channelRequestSucces = true;

            var channelRequestResponse = _channelRequestResponse;
            if (channelRequestResponse != null)
                channelRequestResponse.Set();
        }

        /// <summary>
        /// Called when channel request failed.
        /// </summary>
        protected override void OnFailure()
        {
            base.OnFailure();
            _channelRequestSucces = false;

            var channelRequestResponse = _channelRequestResponse;
            if (channelRequestResponse != null)
                channelRequestResponse.Set();
        }

        /// <summary>
        /// Sends the channel open message.
        /// </summary>
        protected void SendChannelOpenMessage()
        {
            // do not allow open to be ChannelOpenMessage to be sent again until we've
            // had a response on the previous attempt for the current channel
            if (Interlocked.CompareExchange(ref _sessionSemaphoreObtained, 1, 0) == 0)
            {
                SessionSemaphore.Wait();
                SendMessage(
                    new ChannelOpenMessage(
                        LocalChannelNumber,
                        LocalWindowSize,
                        LocalPacketSize,
                        new SessionChannelOpenInfo()));
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_channelOpenResponseWaitHandle != null)
                {
                    _channelOpenResponseWaitHandle.Dispose();
                    _channelOpenResponseWaitHandle = null;
                }

                if (_channelRequestResponse != null)
                {
                    _channelRequestResponse.Dispose();
                    _channelRequestResponse = null;
                }
            }
        }

        /// <summary>
        /// Releases the session semaphore.
        /// </summary>
        /// <remarks>
        /// When the session semaphore has already been released, or was never obtained by
        /// this instance, then this method does nothing.
        /// </remarks>
        private void ReleaseSemaphore()
        {
            if (Interlocked.CompareExchange(ref _sessionSemaphoreObtained, 0, 1) == 1)
                SessionSemaphore.Release();
        }
    }
}
