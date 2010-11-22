using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient
{
    public class SshCommand : IDisposable
    {
        private Encoding _encoding;

        private Session _session;

        private ChannelSession _channel;

        private CommandAsyncResult _asyncResult;

        private AsyncCallback _callback;

        private EventWaitHandle _sessionErrorOccuredWaitHandle = new AutoResetEvent(false);

        private Exception _exception;

        private bool _hasError;

        public string CommandText { get; private set; }

        public int CommandTimeout { get; set; }

        public uint ExitStatus { get; private set; }

        public MemoryStream OutputStream { get; private set; }

        public MemoryStream ExtendedOutputStream { get; private set; }

        public string Result
        {
            get
            {
                return this._encoding.GetString(this.OutputStream.ToArray());
            }
        }

        public string Error
        {
            get
            {
                if (this._hasError)
                    return this._encoding.GetString(this.ExtendedOutputStream.ToArray());
                else
                    return string.Empty;
            }
        }

        internal SshCommand(Session session, string commandText)
            : this(session, commandText, Encoding.ASCII)
        {
        }

        internal SshCommand(Session session, string commandText, Encoding encoding)
        {
            this._encoding = encoding;
            this._session = session;
            this.CommandText = commandText;
            this.CommandTimeout = -1;

            this.CreateChannel();

            this._session.Disconnected += Session_Disconnected;
            this._session.ErrorOccured += Session_ErrorOccured;
        }

        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            //  Prevent from executing BeginExecute before calling EndExecute
            if (this._asyncResult != null)
            {
                throw new InvalidOperationException("");
            }

            //  Create new AsyncResult object
            this._asyncResult = new CommandAsyncResult(this)
            {
                AsyncWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset),
                IsCompleted = false,
                AsyncState = state,
            };

            //  When comman rexecuted again, create a new channel
            if (this._channel != null)
            {
                this.CreateChannel();
            }

            if (string.IsNullOrEmpty(this.CommandText))
                throw new ArgumentException("CommandText property is empty.");

            this._callback = callback;

            this._channel.Open();

            //  Send channel command request
            this._channel.SendExecRequest(this.CommandText);

            return _asyncResult;
        }

        public IAsyncResult BeginExecute(string commandText, AsyncCallback callback, object state)
        {
            this.CommandText = commandText;
            return BeginExecute(callback, state);
        }

        public string EndExecute(IAsyncResult asynchResult)
        {
            CommandAsyncResult channelAsyncResult = asynchResult as CommandAsyncResult;

            if (channelAsyncResult.Command != this)
            {
                throw new InvalidOperationException("Invalid IAsyncResult parameter");
            }

            //  Make sure that operation completed if not wait for it to finish
            this.WaitHandle(this._asyncResult.AsyncWaitHandle);

            this._channel.Close();

            this.DeleteChannel();

            this._asyncResult = null;

            return this.Result;
        }

        public string Execute()
        {
            return this.EndExecute(this.BeginExecute(null, null));
        }

        public string Execute(string commandText)
        {
            this.CommandText = commandText;
            return this.Execute();
        }

        private void CreateChannel()
        {
            this._channel = this._session.CreateChannel<ChannelSession>();
            this._channel.DataReceived += Channel_DataReceived;
            this._channel.ExtendedDataReceived += Channel_ExtendedDataReceived;
            this._channel.RequestReceived += Channel_RequestReceived;
            this._channel.Closed += Channel_Closed;
            this.OutputStream = new MemoryStream();
            this.ExtendedOutputStream = new MemoryStream();
        }

        private void DeleteChannel()
        {
            this._channel.DataReceived -= Channel_DataReceived;
            this._channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            this._channel.RequestReceived -= Channel_RequestReceived;
            this._channel.Closed -= Channel_Closed;

            this._channel.Dispose();
            this._channel = null;
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            this._exception = new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReasons.ConnectionLost);

            this._sessionErrorOccuredWaitHandle.Set();
        }

        private void Session_ErrorOccured(object sender, ErrorEventArgs e)
        {
            this._exception = e.GetException();

            this._sessionErrorOccuredWaitHandle.Set();
        }

        private void Channel_Closed(object sender, Common.ChannelEventArgs e)
        {
            if (this.OutputStream != null)
            {
                this.OutputStream.Flush();
            }

            if (this.ExtendedOutputStream != null)
            {
                this.ExtendedOutputStream.Flush();
            }

            this._asyncResult.IsCompleted = true;
            if (this._callback != null)
            {
                //  TODO:   Execute this method on different thread since it will be run on message listener
                this._callback(this._asyncResult);
            }
            ((EventWaitHandle)_asyncResult.AsyncWaitHandle).Set();
        }

        private void Channel_RequestReceived(object sender, Common.ChannelRequestEventArgs e)
        {
            Message replyMessage = new ChannelFailureMessage()
            {
                LocalChannelNumber = this._channel.LocalChannelNumber,
            };

            if (e.Info is ExitStatusRequestInfo)
            {
                ExitStatusRequestInfo exitStatusInfo = e.Info as ExitStatusRequestInfo;

                this.ExitStatus = exitStatusInfo.ExitStatus;

                replyMessage = new ChannelSuccessMessage()
                {
                    LocalChannelNumber = this._channel.LocalChannelNumber,
                };
            }

            if (e.Info.WantReply)
            {
                this._session.SendMessage(replyMessage);
            }
        }

        private void Channel_ExtendedDataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            if (this.ExtendedOutputStream != null)
            {
                this.ExtendedOutputStream.Write(e.Data.GetSshBytes().ToArray(), 0, e.Data.Length);
            }

            if (e.DataTypeCode == 1)
            {
                this._hasError = true;
            }
        }

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            if (this.OutputStream != null)
            {
                this.OutputStream.Write(e.Data.GetSshBytes().ToArray(), 0, e.Data.Length);
            }

            if (this._asyncResult != null)
            {
                this._asyncResult.BytesReceived += e.Data.Length;
            }
        }

        private void WaitHandle(WaitHandle waitHandle)
        {
            var waitHandles = new WaitHandle[]
                {
                    this._sessionErrorOccuredWaitHandle,
                    waitHandle,
                };

            var index = EventWaitHandle.WaitAny(waitHandles, this.CommandTimeout);

            if (index < 1)
            {
                throw this._exception;
            }
            else if (index > 1)
            {
                //  throw time out error
                throw new SshOperationTimeoutException(string.Format("Command '{0}' has timed out.", this.CommandText));
            }
        }

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.OutputStream != null)
                    {
                        this.OutputStream.Dispose();
                    }

                    // Dispose managed resources.
                    if (this.ExtendedOutputStream != null)
                    {
                        this.ExtendedOutputStream.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~SshCommand()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
