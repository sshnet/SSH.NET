using System;
using System.Threading;

namespace Renci.SshClient.Channels
{
    public class ChannelAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Gets or sets the channel that async result was created for.
        /// </summary>
        /// <value>The channel.</value>
        internal ChannelExec Channel { get; private set; }

        public int BytesReceived { get; set; }

        #region IAsyncResult Members

        public object AsyncState { get; internal set; }

        public WaitHandle AsyncWaitHandle { get; internal set; }

        public bool CompletedSynchronously { get; internal set; }

        public bool IsCompleted { get; internal set; }

        #endregion

        internal ChannelAsyncResult(ChannelExec channel)
        {
            this.Channel = channel;
        }
    }
}
