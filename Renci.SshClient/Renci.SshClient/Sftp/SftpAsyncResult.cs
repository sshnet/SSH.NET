using System;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshClient.Sftp
{
    public class SftpAsyncResult : IAsyncResult
    {
        private bool _isCompleted;

        private AsyncCallback _callback;

        private EventWaitHandle _completedWaitHandle = new ManualResetEvent(false);

        internal SftpCommand Command { get; private set; }

        internal SftpAsyncResult(SftpCommand command, AsyncCallback callback, object state)
        {
            this.Command = command;
            this._callback = callback;
            this.AsyncState = state;
            this.AsyncWaitHandle = _completedWaitHandle;
        }

        internal T GetCommand<T>() where T : SftpCommand
        {
            return this.Command as T;
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
                        //  Execute callback on new pool thread
                        Task.Factory.StartNew(() =>
                        {
                            this._callback(this);
                        });
                    }
                    this._completedWaitHandle.Set();
                }
            }
        }

        #endregion
    }
}
