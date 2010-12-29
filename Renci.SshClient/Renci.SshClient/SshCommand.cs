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
using System.Threading.Tasks;

namespace Renci.SshClient
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
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

        /// <summary>
        /// Gets the command text.
        /// </summary>
        public string CommandText { get; private set; }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        /// <value>
        /// The command timeout.
        /// </value>
        public TimeSpan CommandTimeout { get; set; }

        /// <summary>
        /// Gets the command exit status.
        /// </summary>
        public uint ExitStatus { get; private set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        public MemoryStream OutputStream { get; private set; }

        /// <summary>
        /// Gets the extended output stream.
        /// </summary>
        public MemoryStream ExtendedOutputStream { get; private set; }

        /// <summary>
        /// Gets the command execution result.
        /// </summary>
        public string Result
        {
            get
            {
                return this._encoding.GetString(this.OutputStream.ToArray());
            }
        }

        /// <summary>
        /// Gets the command execution error.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SshCommand"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding.</param>
        internal SshCommand(Session session, string commandText, Encoding encoding)
        {
            this._encoding = encoding;
            this._session = session;
            this.CommandText = commandText;
            this.CommandTimeout = new TimeSpan(0, 0, 0, 0, -1);

            this._session.Disconnected += Session_Disconnected;
            this._session.ErrorOccured += Session_ErrorOccured;
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>An <see cref="System.IAsyncResult"/> that represents the asynchronous command execution, which could still be pending.</returns>
        /// <exception cref="Renci.SshClient.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshClient.Common.SshOperationTimeoutException">Operation has timed out.</exception>
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

            //  When command re-executed again, create a new channel
            if (this._channel != null)
            {
                throw new SshException("Invalid operation.");
            }

            this.CreateChannel();

            if (string.IsNullOrEmpty(this.CommandText))
                throw new ArgumentException("CommandText property is empty.");

            this._callback = callback;

            this._channel.Open();

            //  Send channel command request
            this._channel.SendExecRequest(this.CommandText);

            return _asyncResult;
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>An <see cref="System.IAsyncResult"/> that represents the asynchronous command execution, which could still be pending.</returns>
        /// <exception cref="Renci.SshClient.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshClient.Common.SshOperationTimeoutException">Operation has timed out.</exception>
        public IAsyncResult BeginExecute(string commandText, AsyncCallback callback, object state)
        {
            this.CommandText = commandText;
            return BeginExecute(callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous command execution to complete.
        /// </summary>
        /// <param name="asynchResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns></returns>
        public string EndExecute(IAsyncResult asynchResult)
        {
            CommandAsyncResult channelAsyncResult = asynchResult as CommandAsyncResult;

            //  TODO: Create a method to perform this check in the method
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

        /// <summary>
        /// Executes command specified by <see cref="CommandText"/> property.
        /// </summary>
        /// <returns>Command execution result</returns>
        /// <exception cref="Renci.SshClient.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshClient.Common.SshOperationTimeoutException">Operation has timed out.</exception>
        public string Execute()
        {
            return this.EndExecute(this.BeginExecute(null, null));
        }

        /// <summary>
        /// Executes the specified command text.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Command execution result</returns>
        /// <exception cref="Renci.SshClient.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshClient.Common.SshOperationTimeoutException">Operation has timed out.</exception>
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
                //  Execute callback on different thread
                Task.Factory.StartNew(() => { this._callback(this._asyncResult); });
            }
            ((EventWaitHandle)_asyncResult.AsyncWaitHandle).Set();
        }

        private void Channel_RequestReceived(object sender, Common.ChannelRequestEventArgs e)
        {
            Message replyMessage = new ChannelFailureMessage(this._channel.LocalChannelNumber);

            if (e.Info is ExitStatusRequestInfo)
            {
                ExitStatusRequestInfo exitStatusInfo = e.Info as ExitStatusRequestInfo;

                this.ExitStatus = exitStatusInfo.ExitStatus;

                replyMessage = new ChannelSuccessMessage(this._channel.LocalChannelNumber);
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
                this.ExtendedOutputStream.Flush();
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
                this.OutputStream.Flush();
            }

            if (this._asyncResult != null)
            {
                lock (this._asyncResult)
                {
                    this._asyncResult.BytesReceived += e.Data.Length;
                }
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

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
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

                    // Dispose managed resources.
                    if (this._sessionErrorOccuredWaitHandle != null)
                    {
                        this._sessionErrorOccuredWaitHandle.Dispose();
                    }

                    // Dispose managed resources.
                    if (this._channel != null)
                    {
                        this._channel.Dispose();
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SshCommand"/> is reclaimed by garbage collection.
        /// </summary>
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
