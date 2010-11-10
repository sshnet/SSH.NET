
using System.Diagnostics;
using System.Threading;
using Renci.SshClient.Common;
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
        /// Counts faile channel open attempts
        /// </summary>
        private int _failedOpenAttempts;

        private EventWaitHandle _channelOpenResponseWaitHandle = new AutoResetEvent(false);

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

            ChannelSession._channelSessionCounter--;

            Debug.WriteLine(string.Format("channel {0} closed. open channels {1}", this.RemoteChannelNumber, ChannelSession._channelSessionCounter));

            _totalClose++;

            _slim.Release();
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
