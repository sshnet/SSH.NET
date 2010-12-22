using System.Diagnostics;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelSession : Channel
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

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        /// <summary>
        /// Opens the channel
        /// </summary>
        public virtual void Open()
        {
            if (!this.IsOpen)
            {
                //  Try to opend channel several times
                while (this._failedOpenAttempts < this.ConnectionInfo.RetryAttempts && !this.IsOpen)
                {
                    this.SendChannelOpenMessage();
                    this.WaitHandle(this._channelOpenResponseWaitHandle);
                }

                if (!this.IsOpen)
                {
                    throw new SshException(string.Format("Failed to open a channel after {0} attemps.", this._failedOpenAttempts));
                }
            }
        }

        /// <summary>
        /// Called when chanel is open
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            Debug.WriteLine(string.Format("channel {0} open.", this.RemoteChannelNumber));

            this._channelOpenResponseWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel failed to open
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="description">The description.</param>
        /// <param name="language">The language.</param>
        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            this._failedOpenAttempts++;

            Debug.WriteLine(string.Format("Local channel: {0} attempts: {1}.", this.LocalChannelNumber, this._failedOpenAttempts));

            this.SessionSemaphore.Release();

            this._channelOpenResponseWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel is closed
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();

            Debug.WriteLine(string.Format("channel {0} closed", this.RemoteChannelNumber));


            //  This timeout needed since when channel is closed it does not immidiatly becomes availble
            //  but it takes time for the server to clean up resource and allow new channels to be created.
            Thread.Sleep(100);

            this.SessionSemaphore.Release();
        }

        public bool SendPseudoTerminalRequest(string environmentVariable, uint columns, uint rows, uint width, uint height, string terminalMode)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new PseudoTerminalRequestInfo(environmentVariable, columns, rows, width, height, terminalMode)));

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendX11ForwardingRequest(bool isSignleConnection, string protocol, string cookie, uint screenNumber)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new X11ForwardingRequestInfo(isSignleConnection, protocol, cookie, screenNumber)));

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendEnvironmentVariableRequest(string variableName, string variableValue)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new EnvironmentVariableRequestInfo(variableName, variableValue)));

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendShellRequest()
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ShellRequestInfo()));

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendExecRequest(string command)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ExecRequestInfo(command)));

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendSubsystemRequest(string subsystem)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new SubsystemRequestInfo(subsystem)));

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new WindowChangeRequestInfo(columns, rows, width, height)));

            return true;
        }

        public bool SendLocalFlowRequest(bool clientCanDo)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new XonXoffRequestInfo(clientCanDo)));

            return true;
        }

        public bool SendSignalRequest(string signalName)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new SignalRequestInfo(signalName)));

            return true;
        }

        public bool SendExitStatusRequest(uint exitStatus)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ExitStatusRequestInfo(exitStatus)));

            return true;
        }

        public bool SendExitSignalRequest(string signalName, bool coreDumped, string errorMessage, string language)
        {
            this.SendMessage(new ChannelRequestMessage(this.RemoteChannelNumber, new ExitSignalRequestInfo(signalName, coreDumped, errorMessage, language)));

            return true;
        }

        protected override void OnSuccess()
        {
            base.OnSuccess();
            this._channelRequestSucces = true;
            this._channelRequestResponse.Set();
        }

        protected override void OnFailure()
        {
            base.OnFailure();
            this._channelRequestSucces = false;
            this._channelRequestResponse.Set();
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

                this.SendMessage(new ChannelOpenMessage(this.LocalChannelNumber, this.LocalWindowSize, this.PacketSize, new SessionChannelOpenInfo()));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._channelOpenResponseWaitHandle != null)
            {
                this._channelOpenResponseWaitHandle.Dispose();
            }

            if (this._channelRequestResponse != null)
            {
                this._channelRequestResponse.Dispose();
            }
        }
    }
}
