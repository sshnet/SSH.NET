
using System.Diagnostics;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Connection;
namespace Renci.SshClient.Channels
{
    internal abstract class ChannelSession : Channel
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

            this.SendMessage(new ChannelRequestPseudoTerminalMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
                //RequestName = ChannelRequestNames.PseudoTerminal,
                WantReply = true,
                EnvironmentVariable = environmentVariable,
                Columns = columns,
                Rows = rows,
                PixelWidth = width,
                PixelHeight = height,
                TerminalMode = terminalMode,
            });

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendX11ForwardingRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "x11-req"
            //boolean   want reply
            //boolean   single connection
            //string    x11 authentication protocol
            //string    x11 authentication cookie
            //uint32    x11 screen number
            return false;
        }

        public bool SendEnvironmentVariableRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "env"
            //boolean   want reply
            //string    variable name
            //string    variable value
            return false;
        }

        public bool SendShellRequest()
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestShellMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
                //RequestName = ChannelRequestNames.Shell,
                WantReply = true,
            });

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendExecRequest(string command)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestExecMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
                WantReply = true,
                Command = command,
            });

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendSubsystemRequest(string subsystem)
        {
            this._channelRequestResponse.Reset();

            this.SendMessage(new ChannelRequestSubsystemMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
                WantReply = true,
                SubsystemName = subsystem,
            });

            this._channelRequestResponse.WaitOne();

            return this._channelRequestSucces;
        }

        public bool SendWindowChangeRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "window-change"
            //boolean   FALSE
            //uint32    terminal width, columns
            //uint32    terminal height, rows
            //uint32    terminal width, pixels
            //uint32    terminal height, pixels
            return false;
        }

        public bool SendLocalFlowRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "xon-xoff"
            //boolean   FALSE
            //boolean   client can do
            return false;
        }

        public bool SendSignalRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "signal"
            //boolean   FALSE
            //string    signal name (without the "SIG" prefix)
            return false;
        }

        public bool SendExitStatusRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "exit-status"
            //boolean   FALSE
            //uint32    exit_status
            return false;
        }

        public bool SendExitSignalRequest()
        {
            //byte      SSH_MSG_CHANNEL_REQUEST
            //uint32    recipient channel
            //string    "exit-signal"
            //boolean   FALSE
            //string    signal name (without the "SIG" prefix)
            //boolean   core dumped
            //string    error message in ISO-10646 UTF-8 encoding
            //string    language tag [RFC3066]
            return false;
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
        /// Called when object is being disposed.
        /// </summary>
        protected override void OnDisposing()
        {
            if (this._channelOpenResponseWaitHandle != null)
            {
                this._channelOpenResponseWaitHandle.Dispose();
            }
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

                this.SendMessage(new ChannelOpenMessage
                {
                    ChannelType = ChannelTypes.Session,
                    LocalChannelNumber = this.LocalChannelNumber,
                    InitialWindowSize = this.LocalWindowSize,
                    MaximumPacketSize = this.PacketSize,
                });
            }
        }
    }
}
