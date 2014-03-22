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
    internal class ChannelSession : ClientChannel
    {
        /// <summary>
        /// Counts faile channel open attempts
        /// </summary>
        private int _failedOpenAttempts;

        /// <summary>
        /// Wait handle to signal when response was received to open the channel
        /// </summary>
        private EventWaitHandle _channelOpenResponseWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelRequestResponse = new ManualResetEvent(false);

        private bool _channelRequestSucces;

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
            if (!this.IsOpen)
            {
                //  Try to open channel several times
                while (this._failedOpenAttempts < this.ConnectionInfo.RetryAttempts && !this.IsOpen)
                {
                    this.SendChannelOpenMessage();
                    this.WaitOnHandle(this._channelOpenResponseWaitHandle);
                }

                if (!this.IsOpen)
                    throw new SshException(string.Format(CultureInfo.CurrentCulture, "Failed to open a channel after {0} attempts.", this._failedOpenAttempts));
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
            this._channelOpenResponseWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel failed to open.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="description">The description.</param>
        /// <param name="language">The language.</param>
        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            this._failedOpenAttempts++;
            this.SessionSemaphore.Release();
            this._channelOpenResponseWaitHandle.Set();
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

            this.SessionSemaphore.Release();
        }

        protected override void Close(bool wait)
        {
            base.Close(wait);

            if (!wait)
                this.SessionSemaphore.Release();
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
        /// true if request was successful; otherwise false.
        /// </returns>
        public bool SendPseudoTerminalRequest(string environmentVariable, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues)
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new PseudoTerminalRequestInfo(environmentVariable, columns, rows, width, height, terminalModeValues)));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the X11 forwarding request.
        /// </summary>
        /// <param name="isSingleConnection">if set to <c>true</c> the it is single connection.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="cookie">The cookie.</param>
        /// <param name="screenNumber">The screen number.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendX11ForwardingRequest(bool isSingleConnection, string protocol, byte[] cookie, uint screenNumber)
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new X11ForwardingRequestInfo(isSingleConnection, protocol, cookie, screenNumber)));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the environment variable request.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendEnvironmentVariableRequest(string variableName, string variableValue)
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new EnvironmentVariableRequestInfo(variableName, variableValue)));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the shell request.
        /// </summary>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendShellRequest()
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ShellRequestInfo()));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendExecRequest(string command)
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ExecRequestInfo(command, this.ConnectionInfo.Encoding)));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the exec request.
        /// </summary>
        /// <param name="breakLength">Length of the break.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendBreakRequest(uint breakLength)
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new BreakRequestInfo(breakLength)));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the subsystem request.
        /// </summary>
        /// <param name="subsystem">The subsystem.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendSubsystemRequest(string subsystem)
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new SubsystemRequestInfo(subsystem)));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends the window change request.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new WindowChangeRequestInfo(columns, rows, width, height)));
            return true;
        }

        /// <summary>
        /// Sends the local flow request.
        /// </summary>
        /// <param name="clientCanDo">if set to <c>true</c> [client can do].</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendLocalFlowRequest(bool clientCanDo)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new XonXoffRequestInfo(clientCanDo)));
            return true;
        }

        /// <summary>
        /// Sends the signal request.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendSignalRequest(string signalName)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new SignalRequestInfo(signalName)));
            return true;
        }

        /// <summary>
        /// Sends the exit status request.
        /// </summary>
        /// <param name="exitStatus">The exit status.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendExitStatusRequest(uint exitStatus)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ExitStatusRequestInfo(exitStatus)));
            return true;
        }

        /// <summary>
        /// Sends the exit signal request.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <param name="coreDumped">if set to <c>true</c> [core dumped].</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="language">The language.</param>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendExitSignalRequest(string signalName, bool coreDumped, string errorMessage, string language)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ExitSignalRequestInfo(signalName, coreDumped, errorMessage, language)));
            return true;
        }

        /// <summary>
        /// Sends eow@openssh.com request.
        /// </summary>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendEndOfWriteRequest()
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new EndOfWriteRequestInfo()));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Sends keepalive@openssh.com request.
        /// </summary>
        /// <returns>true if request was successful; otherwise false.</returns>
        public bool SendKeepAliveRequest()
        {
            this._channelRequestResponse.Reset();
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new KeepAliveRequestInfo()));
            this.WaitOnHandle(this._channelRequestResponse);
            return this._channelRequestSucces;
        }

        /// <summary>
        /// Called when channel request was successful
        /// </summary>
        protected override void OnSuccess()
        {
            base.OnSuccess();
            this._channelRequestSucces = true;

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
            lock (this.SessionSemaphore)
            {
                //  Ensure that channels are available
                this.SessionSemaphore.Wait();
                this.SendMessage(new ChannelOpenMessage(this.LocalChannelNumber, this.LocalWindowSize, this.LocalPacketSize, new SessionChannelOpenInfo()));
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (this._channelOpenResponseWaitHandle != null)
            {
                this._channelOpenResponseWaitHandle.Dispose();
                this._channelOpenResponseWaitHandle = null;
            }

            if (this._channelRequestResponse != null)
            {
                this._channelRequestResponse.Dispose();
                this._channelRequestResponse = null;
            }

            base.Dispose(disposing);
        }
    }
}
