using System;
using System.Collections.Generic;
using System.Threading;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
    public class SftpAsyncResult : IAsyncResult
    {
        private bool _isCompleted;

        private Channels.ChannelSession _channelSession;

        private AsyncCallback _callback;

        private EventWaitHandle _completedWaitHandle = new ManualResetEvent(false);

        public IList<FtpFileInfo> Names { get; internal set; }

        internal SftpAsyncResult(Channels.ChannelSession channelSession, AsyncCallback callback, object state)
        {
            this._channelSession = channelSession;
            this._callback = callback;
            this.AsyncState = state;
            this.AsyncWaitHandle = _completedWaitHandle;
        }

        #region IAsyncResult Members

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        public bool IsCompleted
        {
            get
            {
                return this._isCompleted;
            }
            internal set
            {
                this._isCompleted = value;
                if (value)
                {
                    if (this._callback != null)
                    {
                        this._callback(this);
                    }
                    this._completedWaitHandle.Set();
                }
            }
        }

        #endregion
    }
}
