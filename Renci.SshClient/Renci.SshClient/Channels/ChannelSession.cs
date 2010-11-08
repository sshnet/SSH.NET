
using System;
using System.Text;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;
namespace Renci.SshClient.Channels
{
    internal abstract class ChannelSession : Channel
    {
        private volatile static int _channelSessionCounter = 0;

        private static object _lock = new object();

        /// <summary>
        /// If error occured holds error message
        /// </summary>
        private StringBuilder _errorMessage;

        /// <summary>
        /// Counts faile channel open attempts
        /// </summary>
        private int _failedOpenAttempts;

        private EventWaitHandle _channelOpenResponseWaitHandle = new AutoResetEvent(false);


        public uint ExitStatus { get; private set; }

        public bool CanCreateChannel
        {
            get
            {
                if (ChannelSession._channelSessionCounter < 10)
                    return true;
                else
                    return false;
            }
        }

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

        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            this._channelOpenResponseWaitHandle.Set();
        }

        protected override void OnClose()
        {
            base.OnClose();

            //  Throw an error if exit status is not 0
            if (this.ExitStatus > 0)
            {
                throw new SshException(this._errorMessage.ToString(), this.ExitStatus);
            }

            lock (_lock)
            {
                ChannelSession._channelSessionCounter--;
                Monitor.Pulse(_lock);
            }
        }

        protected override void OnExtendedData(string data, uint dataTypeCode)
        {
            base.OnExtendedData(data, dataTypeCode);

            if (dataTypeCode == 1)
            {
                if (this._errorMessage == null)
                    this._errorMessage = new StringBuilder();
                this._errorMessage.Append(data);
            }
        }

        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            //  TODO:   See why occasionaly open channel will fail when try to utilze maximum number of channels

            this._failedOpenAttempts++;

            lock (_lock)
            {
                ChannelSession._channelSessionCounter--;
                Monitor.Pulse(_lock);
            }

            this._channelOpenResponseWaitHandle.Set();
        }

        protected override void OnRequest(ChannelRequestNames requestName, bool wantReply, string command, string subsystemName, uint exitStatus)
        {
            base.OnRequest(requestName, wantReply, command, subsystemName, exitStatus);

            Message replyMessage = new ChannelFailureMessage()
            {
                LocalChannelNumber = this.LocalChannelNumber,
            };

            if (requestName == ChannelRequestNames.ExitStatus)
            {
                this.ExitStatus = exitStatus;

                replyMessage = new ChannelSuccessMessage()
                {
                    LocalChannelNumber = this.LocalChannelNumber,
                };
            }
            else if (requestName == ChannelRequestNames.PseudoTerminal)
            {
                //  TODO:   Check if when this request is received what to do, I suspect we receive this request when no more channel sessions are available
            }
            else
            {
                throw new NotImplementedException(string.Format("Request name {0} is not implemented.", requestName));
            }

            if (wantReply)
            {
                this.SendMessage(replyMessage);
            }
        }

        protected override void OnDisposing()
        {
            if (this._channelOpenResponseWaitHandle != null)
            {
                this._channelOpenResponseWaitHandle.Dispose();
            }
        }

        protected void SendChannelOpenMessage()
        {
            lock (_lock)
            {
                while (true)
                {
                    //  TODO:   Make sure that channel session counter is per session and not static, possible scenario where multiple SshClients can have only 10 session channels max
                    if (ChannelSession._channelSessionCounter < 10)
                        break;
                    Monitor.Wait(_lock);
                }
                this.SendMessage(new ChannelOpenMessage
                {
                    ChannelType = ChannelTypes.Session,
                    LocalChannelNumber = this.LocalChannelNumber,
                    InitialWindowSize = this.LocalWindowSize,
                    MaximumPacketSize = this.PacketSize,
                });

                ChannelSession._channelSessionCounter++;
            }
        }
    }
}
