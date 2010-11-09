
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;
namespace Renci.SshClient.Channels
{
    internal abstract class ChannelSession : Channel
    {
        //  TODO:   Some debug information to be removed later
        private static volatile int _totalOpenRequests;
        private static volatile int _totalConfirmation = 0;
        private static volatile int _totalClose = 0;
        private static volatile int _totalFailed = 0;

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

            ChannelSession._channelSessionCounter++;
            Debug.WriteLine(string.Format("channel {0} open. open channels {1}", this.RemoteChannelNumber, ChannelSession._channelSessionCounter));

            _totalConfirmation++;

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

            ChannelSession._channelSessionCounter--;

            Debug.WriteLine(string.Format("channel {0} closed. open channels {1}", this.RemoteChannelNumber, ChannelSession._channelSessionCounter));

            _totalClose++;

            _slim.Release();

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

            Debug.WriteLine(string.Format("Local channel: {0} attempts: {1} max channels: {2}", this.LocalChannelNumber, this._failedOpenAttempts, ChannelSession._channelSessionCounter));

            _totalFailed++;

            _slim.Release();

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

        private static SemaphoreSlim _slim = new SemaphoreSlim(10);

        protected void SendChannelOpenMessage()
        {
            _slim.Wait();

            Debug.WriteLine(string.Format("send open new channel, total channels {0}", ChannelSession._channelSessionCounter));

            this.SendMessage(new ChannelOpenMessage
            {
                ChannelType = ChannelTypes.Session,
                LocalChannelNumber = this.LocalChannelNumber,
                InitialWindowSize = this.LocalWindowSize,
                MaximumPacketSize = this.PacketSize,
            });

            _totalOpenRequests++;
        }
    }
}
