
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
