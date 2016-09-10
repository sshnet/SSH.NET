using System;
using System.IO;
using System.Text;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;
using System.Globalization;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    public class SshCommand : IDisposable
    {
        private ISession _session;
        private readonly Encoding _encoding;
        private IChannelSession _channel;
        private CommandAsyncResult _asyncResult;
        private AsyncCallback _callback;
        private EventWaitHandle _sessionErrorOccuredWaitHandle;
        private Exception _exception;
        private bool _hasError;
        private readonly object _endExecuteLock = new object();

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
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Execute CommandTimeout" language="C#" title="Specify command execution timeout" />
        /// </example>
        public TimeSpan CommandTimeout { get; set; }

        /// <summary>
        /// Gets the command exit status.
        /// </summary>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand RunCommand ExitStatus" language="C#" title="Get command execution exit status" />
        /// </example>
        public int ExitStatus { get; private set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Execute OutputStream" language="C#" title="Use OutputStream to get command execution output" />
        /// </example>
        public Stream OutputStream { get; private set; }

        /// <summary>
        /// Gets the extended output stream.
        /// </summary>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Execute ExtendedOutputStream" language="C#" title="Use ExtendedOutputStream to get command debug execution output" />
        /// </example>
        public Stream ExtendedOutputStream { get; private set; }

        private StringBuilder _result;
        /// <summary>
        /// Gets the command execution result.
        /// </summary>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand RunCommand Result" language="C#" title="Running simple command" />
        /// </example>
        public string Result
        {
            get
            {
                if (_result == null)
                {
                    _result = new StringBuilder();
                }

                if (OutputStream != null && OutputStream.Length > 0)
                {
                    // do not dispose the StreamReader, as it would also dispose the stream
                    var sr = new StreamReader(OutputStream, _encoding);
                    _result.Append(sr.ReadToEnd());
                }

                return _result.ToString();
            }
        }

        private StringBuilder _error;
        /// <summary>
        /// Gets the command execution error.
        /// </summary>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Error" language="C#" title="Display command execution error" />
        /// </example>
        public string Error
        {
            get
            {
                if (_hasError)
                {
                    if (_error == null)
                    {
                        _error = new StringBuilder();
                    }

                    if (ExtendedOutputStream != null && ExtendedOutputStream.Length > 0)
                    {
                        // do not dispose the StreamReader, as it would also dispose the stream
                        var sr = new StreamReader(ExtendedOutputStream, _encoding);
                        _error.Append(sr.ReadToEnd());
                    }

                    return _error.ToString();
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshCommand"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding to use for the results.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="session"/>, <paramref name="commandText"/> is <c>null</c>.</exception>
        internal SshCommand(ISession session, string commandText, Encoding encoding)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (commandText == null)
                throw new ArgumentNullException("commandText");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _session = session;
            CommandText = commandText;
            _encoding = encoding;
            CommandTimeout = Session.InfiniteTimeSpan;
            _sessionErrorOccuredWaitHandle = new AutoResetEvent(false);

            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccured;
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <returns>
        /// An <see cref="System.IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand BeginExecute IsCompleted EndExecute" language="C#" title="Asynchronous Command Execution" />
        /// </example>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshException">Invalid operation.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        public IAsyncResult BeginExecute()
        {
            return BeginExecute(null, null);
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <returns>
        /// An <see cref="System.IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshException">Invalid operation.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        public IAsyncResult BeginExecute(AsyncCallback callback)
        {
            return BeginExecute(callback, null);
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshException">Invalid operation.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            //  Prevent from executing BeginExecute before calling EndExecute
            if (_asyncResult != null && !_asyncResult.EndCalled)
            {
                throw new InvalidOperationException("Asynchronous operation is already in progress.");
            }

            //  Create new AsyncResult object
            _asyncResult = new CommandAsyncResult
                {
                    AsyncWaitHandle = new ManualResetEvent(false),
                    IsCompleted = false,
                    AsyncState = state,
                };

            //  When command re-executed again, create a new channel
            if (_channel != null)
            {
                throw new SshException("Invalid operation.");
            }

            if (string.IsNullOrEmpty(CommandText))
                throw new ArgumentException("CommandText property is empty.");

            var outputStream = OutputStream;
            if (outputStream != null)
            {
                outputStream.Dispose();
                OutputStream = null;
            }

            var extendedOutputStream = ExtendedOutputStream;
            if (extendedOutputStream != null)
            {
                extendedOutputStream.Dispose();
                ExtendedOutputStream = null;
            }

            //  Initialize output streams
            OutputStream = new PipeStream();
            ExtendedOutputStream = new PipeStream();

            _result = null;
            _error = null;
            _callback = callback;

            _channel = CreateChannel();
            _channel.Open();
            _channel.SendExecRequest(CommandText);

            return _asyncResult;
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="System.IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <exception cref="Renci.SshNet.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SshOperationTimeoutException">Operation has timed out.</exception>
        public IAsyncResult BeginExecute(string commandText, AsyncCallback callback, object state)
        {
            CommandText = commandText;

            return BeginExecute(callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous command execution to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>Command execution result.</returns>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand BeginExecute IsCompleted EndExecute" language="C#" title="Asynchronous Command Execution" />
        /// </example>
        /// <exception cref="ArgumentException">Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        public string EndExecute(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            var commandAsyncResult = asyncResult as CommandAsyncResult;
            if (commandAsyncResult == null || _asyncResult != commandAsyncResult)
            {
                throw new ArgumentException(string.Format("The {0} object was not returned from the corresponding asynchronous method on this class.", typeof(IAsyncResult).Name));
            }

            lock (_endExecuteLock)
            {
                if (commandAsyncResult.EndCalled)
                {
                    throw new ArgumentException("EndExecute can only be called once for each asynchronous operation.");
                }

                //  wait for operation to complete (or time out)
                WaitOnHandle(_asyncResult.AsyncWaitHandle);

                UnsubscribeFromEventsAndDisposeChannel(_channel);
                _channel = null;

                commandAsyncResult.EndCalled = true;

                return Result;
            }
        }

        /// <summary>
        /// Executes command specified by <see cref="CommandText"/> property.
        /// </summary>
        /// <returns>Command execution result</returns>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Execute" language="C#" title="Simple command execution" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Error" language="C#" title="Display command execution error" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand Execute CommandTimeout" language="C#" title="Specify command execution timeout" />
        /// </example>
        /// <exception cref="Renci.SshNet.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SshOperationTimeoutException">Operation has timed out.</exception>
        public string Execute()
        {
            return EndExecute(BeginExecute(null, null));
        }

        /// <summary>
        /// Cancels command execution in asynchronous scenarios. 
        /// </summary>
        public void CancelAsync()
        {
            if (_channel != null && _channel.IsOpen && _asyncResult != null)
            {
                // TODO: check with Oleg if we shouldn't dispose the channel and uninitialize it ?
                _channel.Dispose();
            }
        }

        /// <summary>
        /// Executes the specified command text.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Command execution result</returns>
        /// <exception cref="Renci.SshNet.Common.SshConnectionException">Client is not connected.</exception>
        /// <exception cref="Renci.SshNet.Common.SshOperationTimeoutException">Operation has timed out.</exception>
        public string Execute(string commandText)
        {
            CommandText = commandText;

            return Execute();
        }

        private IChannelSession CreateChannel()
        {
            var channel = _session.CreateChannelSession();
            channel.DataReceived += Channel_DataReceived;
            channel.ExtendedDataReceived += Channel_ExtendedDataReceived;
            channel.RequestReceived += Channel_RequestReceived;
            channel.Closed += Channel_Closed;
            return channel;
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            //  If objected is disposed or being disposed don't handle this event
            if (_isDisposed)
                return;

            _exception = new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);

            _sessionErrorOccuredWaitHandle.Set();
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            //  If objected is disposed or being disposed don't handle this event
            if (_isDisposed)
                return;

            _exception = e.Exception;

            _sessionErrorOccuredWaitHandle.Set();
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            var outputStream = OutputStream;
            if (outputStream != null)
            {
                outputStream.Flush();
            }

            var extendedOutputStream = ExtendedOutputStream;
            if (extendedOutputStream != null)
            {
                extendedOutputStream.Flush();
            }

            _asyncResult.IsCompleted = true;

            if (_callback != null)
            {
                //  Execute callback on different thread
                ThreadAbstraction.ExecuteThread(() => _callback(_asyncResult));
            }
            ((EventWaitHandle) _asyncResult.AsyncWaitHandle).Set();
        }

        private void Channel_RequestReceived(object sender, ChannelRequestEventArgs e)
        {
            var exitStatusInfo = e.Info as ExitStatusRequestInfo;
            if (exitStatusInfo != null)
            {
                ExitStatus = (int) exitStatusInfo.ExitStatus;

                if (exitStatusInfo.WantReply)
                {
                    var replyMessage = new ChannelSuccessMessage(_channel.LocalChannelNumber);
                    _session.SendMessage(replyMessage);
                }
            }
            else
            {
                if (e.Info.WantReply)
                {
                    var replyMessage = new ChannelFailureMessage(_channel.LocalChannelNumber);
                    _session.SendMessage(replyMessage);
                }
            }
        }

        private void Channel_ExtendedDataReceived(object sender, ChannelExtendedDataEventArgs e)
        {
            if (ExtendedOutputStream != null)
            {
                ExtendedOutputStream.Write(e.Data, 0, e.Data.Length);
                ExtendedOutputStream.Flush();
            }

            if (e.DataTypeCode == 1)
            {
                _hasError = true;
            }
        }

        private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
        {
            if (OutputStream != null)
            {
                OutputStream.Write(e.Data, 0, e.Data.Length);
                OutputStream.Flush();
            }

            if (_asyncResult != null)
            {
                lock (_asyncResult)
                {
                    _asyncResult.BytesReceived += e.Data.Length;
                }
            }
        }

        /// <exception cref="SshOperationTimeoutException">Command '{0}' has timed out.</exception>
        /// <remarks>The actual command will be included in the exception message.</remarks>
        private void WaitOnHandle(WaitHandle waitHandle)
        {
            var waitHandles = new[]
                {
                    _sessionErrorOccuredWaitHandle,
                    waitHandle
                };

            switch (WaitHandle.WaitAny(waitHandles, CommandTimeout))
            {
                case 0:
                    throw _exception;
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.CurrentCulture, "Command '{0}' has timed out.", CommandText));
            }
        }

        /// <summary>
        /// Unsubscribes the current <see cref="SshCommand"/> from channel events, and disposes
        /// the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <remarks>
        /// Does nothing when <paramref name="channel"/> is <c>null</c>.
        /// </remarks>
        private void UnsubscribeFromEventsAndDisposeChannel(IChannel channel)
        {
            if (channel == null)
                return;

            // unsubscribe from events as we do not want to be signaled should these get fired
            // during the dispose of the channel
            channel.DataReceived -= Channel_DataReceived;
            channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            channel.RequestReceived -= Channel_RequestReceived;
            channel.Closed -= Channel_Closed;

            // actually dispose the channel
            channel.Dispose();
        }

        #region IDisposable Members

        private bool _isDisposed;

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
            if (_isDisposed)
                return;

            if (disposing)
            {
                // unsubscribe from session events to ensure other objects that we're going to dispose
                // are not accessed while disposing
                var session = _session;
                if (session != null)
                {
                    session.Disconnected -= Session_Disconnected;
                    session.ErrorOccured -= Session_ErrorOccured;
                    _session = null;
                }

                // unsubscribe from channel events to ensure other objects that we're going to dispose
                // are not accessed while disposing
                var channel = _channel;
                if (channel != null)
                {
                    UnsubscribeFromEventsAndDisposeChannel(channel);
                    _channel = null;
                }

                var outputStream = OutputStream;
                if (outputStream != null)
                {
                    outputStream.Dispose();
                    OutputStream = null;
                }

                var extendedOutputStream = ExtendedOutputStream;
                if (extendedOutputStream != null)
                {
                    extendedOutputStream.Dispose();
                    ExtendedOutputStream = null;
                }

                var sessionErrorOccuredWaitHandle = _sessionErrorOccuredWaitHandle;
                if (sessionErrorOccuredWaitHandle != null)
                {
                    sessionErrorOccuredWaitHandle.Dispose();
                    _sessionErrorOccuredWaitHandle = null;
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SshCommand"/> is reclaimed by garbage collection.
        /// </summary>
        ~SshCommand()
        {
            Dispose(false);
        }

        #endregion
    }
}
