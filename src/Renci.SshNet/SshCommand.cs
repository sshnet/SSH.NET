﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    public class SshCommand : IDisposable
    {
        private readonly Encoding _encoding;
        private readonly object _endExecuteLock = new object();

        private ISession _session;
        private IChannelSession _channel;
        private CommandAsyncResult _asyncResult;
        private AsyncCallback _callback;
        private EventWaitHandle _sessionErrorOccuredWaitHandle;
        private EventWaitHandle _commmandCancelledWaitHandle;
        private Exception _exception;
        private StringBuilder _result;
        private StringBuilder _error;
        private bool _hasError;
        private bool _isDisposed;
        private ChannelInputStream _inputStream;
        private TimeSpan _commandTimeout;

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
        public TimeSpan CommandTimeout
        {
            get
            {
                return _commandTimeout;
            }
            set
            {
                value.EnsureValidTimeout(nameof(CommandTimeout));

                _commandTimeout = value;
            }
        }

        /// <summary>
        /// Gets the command exit status.
        /// </summary>
        public int ExitStatus { get; private set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        public Stream OutputStream { get; private set; }
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

        /// <summary>
        /// Gets the extended output stream.
        /// </summary>
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        public Stream ExtendedOutputStream { get; private set; }
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

        /// <summary>
        /// Creates and returns the input stream for the command.
        /// </summary>
        /// <returns>
        /// The stream that can be used to transfer data to the command's input stream.
        /// </returns>
 #pragma warning disable CA1859 // Use concrete types when possible for improved performance
        public Stream CreateInputStream()
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        {
            if (_channel == null)
            {
                throw new InvalidOperationException($"The input stream can be used only after calling BeginExecute and before calling EndExecute.");
            }

            if (_inputStream != null)
            {
                throw new InvalidOperationException($"The input stream already exists.");
            }

            _inputStream = new ChannelInputStream(_channel);
            return _inputStream;
        }

        /// <summary>
        /// Gets the command execution result.
        /// </summary>
#pragma warning disable S1133 // Deprecated code should be removed
        [Obsolete("Please read the result from the OutputStream. I.e. new StreamReader(shell.OutputStream).ReadToEnd().")]
#pragma warning disable S1133 // Deprecated code should be removed
        public string Result
        {
            get
            {
                return GetResult();
            }
        }

        internal string GetResult()
        {
            _result ??= new StringBuilder();

            if (OutputStream != null && OutputStream.Length > 0)
            {
                using (var sr = new StreamReader(OutputStream,
                                                 _encoding,
                                                 detectEncodingFromByteOrderMarks: true,
                                                 bufferSize: 1024,
                                                 leaveOpen: true))
                {
                    _ = _result.Append(sr.ReadToEnd());
                }
            }

            return _result.ToString();
        }

        /// <summary>
        /// Gets the command execution error.
        /// </summary>
#pragma warning disable S1133 // Deprecated code should be removed
        [Obsolete("Please read the error result from the ExtendedOutputStream. I.e. new StreamReader(shell.ExtendedOutputStream).ReadToEnd().")]
#pragma warning disable S1133 // Deprecated code should be removed
        public string Error
        {
            get
            {
                return GetError();
            }
        }

        internal string GetError()
        {
            if (_hasError)
            {
                _error ??= new StringBuilder();

                if (ExtendedOutputStream != null && ExtendedOutputStream.Length > 0)
                {
                    using (var sr = new StreamReader(ExtendedOutputStream,
                                                     _encoding,
                                                     detectEncodingFromByteOrderMarks: true,
                                                     bufferSize: 1024,
                                                     leaveOpen: true))
                    {
                        _ = _error.Append(sr.ReadToEnd());
                    }
                }

                return _error.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshCommand"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding to use for the results.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="session"/>, <paramref name="commandText"/> is <see langword="null"/>.</exception>
        internal SshCommand(ISession session, string commandText, Encoding encoding)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (commandText is null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            _session = session;
            CommandText = commandText;
            _encoding = encoding;
            CommandTimeout = Session.InfiniteTimeSpan;
            _sessionErrorOccuredWaitHandle = new AutoResetEvent(initialState: false);
            _commmandCancelledWaitHandle = new AutoResetEvent(initialState: false);

            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccured;
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshException">Invalid operation.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public IAsyncResult BeginExecute()
        {
            return BeginExecute(callback: null, state: null);
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshException">Invalid operation.</exception>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public IAsyncResult BeginExecute(AsyncCallback callback)
        {
            return BeginExecute(callback, state: null);
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
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        {
            // Prevent from executing BeginExecute before calling EndExecute
            if (_asyncResult != null && !_asyncResult.EndCalled)
            {
                throw new InvalidOperationException("Asynchronous operation is already in progress.");
            }

            // Create new AsyncResult object
            _asyncResult = new CommandAsyncResult
                {
                    AsyncWaitHandle = new ManualResetEvent(initialState: false),
                    IsCompleted = false,
                    AsyncState = state,
                };

            if (_channel is not null)
            {
                throw new SshException("Invalid operation.");
            }

            if (string.IsNullOrEmpty(CommandText))
            {
                throw new ArgumentException("CommandText property is empty.");
            }

            var outputStream = OutputStream;
            if (outputStream is not null)
            {
                outputStream.Dispose();
                OutputStream = null;
            }

            var extendedOutputStream = ExtendedOutputStream;
            if (extendedOutputStream is not null)
            {
                extendedOutputStream.Dispose();
                ExtendedOutputStream = null;
            }

            // Initialize output streams
            OutputStream = new PipeStream();
            ExtendedOutputStream = new PipeStream();

            _result = null;
            _error = null;
            _callback = callback;

            _channel = CreateChannel();
            _channel.Open();

            _ = _channel.SendExecRequest(CommandText);

            return _asyncResult;
        }

        /// <summary>
        /// Begins an asynchronous command execution.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the command execution is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that represents the asynchronous command execution, which could still be pending.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
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
        /// <exception cref="ArgumentException">Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        public string EndExecute(IAsyncResult asyncResult)
        {
            if (asyncResult is null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            if (asyncResult is not CommandAsyncResult commandAsyncResult || _asyncResult != commandAsyncResult)
            {
                throw new ArgumentException(string.Format("The {0} object was not returned from the corresponding asynchronous method on this class.", nameof(IAsyncResult)));
            }

            lock (_endExecuteLock)
            {
                if (commandAsyncResult.EndCalled)
                {
                    throw new ArgumentException("EndExecute can only be called once for each asynchronous operation.");
                }

                _inputStream?.Close();

                // wait for operation to complete (or time out)
                WaitOnHandle(_asyncResult.AsyncWaitHandle);

                UnsubscribeFromEventsAndDisposeChannel(_channel);
                _channel = null;

                commandAsyncResult.EndCalled = true;
#pragma warning disable CS0618
                return Result;
#pragma warning disable CS0618
            }
        }

        /// <summary>
        /// Waits for the pending asynchronous command execution to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>Command execution exit status.</returns>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand CreateCommand BeginExecute IsCompleted EndExecute" language="C#" title="Asynchronous Command Execution" />
        /// </example>
        /// <exception cref="ArgumentException">Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        public int EndExecuteWithStatus(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            var commandAsyncResult = asyncResult switch
                {
                    CommandAsyncResult result when result == _asyncResult => result,
                    _ => throw new ArgumentException(
                             $"The {nameof(IAsyncResult)} object was not returned from the corresponding asynchronous method on this class.")
                };

            lock (_endExecuteLock)
            {
                if (commandAsyncResult.EndCalled)
                {
                    throw new ArgumentException("EndExecute can only be called once for each asynchronous operation.");
                }

                // wait for operation to complete (or time out)
                WaitOnHandle(_asyncResult.AsyncWaitHandle);
                UnsubscribeFromEventsAndDisposeChannel(_channel);
                _channel = null;

                commandAsyncResult.EndCalled = true;

                return ExitStatus;
            }
        }

        /// <summary>
        /// Executes the the command asynchronously.
        /// </summary>
        /// <returns>Exit status of the operation.</returns>
        public Task<int> ExecuteAsync()
        {
            return ExecuteAsync(forceKill: false, default);
        }

        /// <summary>
        /// Executes the the command asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>Exit status of the operation.</returns>
        public Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(forceKill: false, cancellationToken);
        }

        /// <summary>
        /// Executes the the command asynchronously.
        /// </summary>
        /// <param name="forceKill">if true send SIGKILL instead of SIGTERM to cancel the command.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>Exit status of the operation.</returns>
        public async Task<int> ExecuteAsync(bool forceKill, CancellationToken cancellationToken)
        {
#if NET || NETSTANDARD2_1_OR_GREATER
            await using var ctr = cancellationToken.Register(() => CancelAsync(forceKill), useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false);
#else
            using var ctr = cancellationToken.Register(() => CancelAsync(forceKill), useSynchronizationContext: false);
#endif // NET || NETSTANDARD2_1_OR_GREATER

            try
            {
                var status = await Task<int>.Factory.FromAsync(BeginExecute(), EndExecuteWithStatus).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                return status;
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Command execution has been cancelled.", cancellationToken);
            }
        }

        /// <summary>
        /// Cancels command execution in asynchronous scenarios.
        /// </summary>
        /// <param name="forceKill">if true send SIGKILL instead of SIGTERM.</param>
        public void CancelAsync(bool forceKill = false)
        {
            var signal = forceKill ? "KILL" : "TERM";
            _ = _channel?.SendExitSignalRequest(signal, coreDumped: false, "Command execution has been cancelled.", "en");
            _ = _commmandCancelledWaitHandle.Set();
        }

        /// <summary>
        /// Executes command specified by <see cref="CommandText"/> property.
        /// </summary>
        /// <returns>
        /// Command execution result.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public string Execute()
        {
            return EndExecute(BeginExecute(callback: null, state: null));
        }

        /// <summary>
        /// Executes the specified command text.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>
        /// The result of the command execution.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
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
            // If objected is disposed or being disposed don't handle this event
            if (_isDisposed)
            {
                return;
            }

            _exception = new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);

            _ = _sessionErrorOccuredWaitHandle.Set();
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            // If objected is disposed or being disposed don't handle this event
            if (_isDisposed)
            {
                return;
            }

            _exception = e.Exception;

            _ = _sessionErrorOccuredWaitHandle.Set();
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            OutputStream?.Flush();
            ExtendedOutputStream?.Flush();

            _asyncResult.IsCompleted = true;

            if (_callback is not null)
            {
                // Execute callback on different thread
                ThreadAbstraction.ExecuteThread(() => _callback(_asyncResult));
            }

            _ = ((EventWaitHandle) _asyncResult.AsyncWaitHandle).Set();
        }

        private void Channel_RequestReceived(object sender, ChannelRequestEventArgs e)
        {
            if (e.Info is ExitStatusRequestInfo exitStatusInfo)
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
                    _commmandCancelledWaitHandle,
                    waitHandle
                };

            var signaledElement = WaitHandle.WaitAny(waitHandles, CommandTimeout);
            switch (signaledElement)
            {
                case 0:
                    ExceptionDispatchInfo.Capture(_exception).Throw();
                    break;
                case 1: // Command cancelled
                case 2:
                    // Specified waithandle was signaled
                    break;
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.CurrentCulture, "Command '{0}' has timed out.", CommandText));
                default:
                    throw new SshException($"Unexpected element '{signaledElement.ToString(CultureInfo.InvariantCulture)}' signaled.");
            }
        }

        /// <summary>
        /// Unsubscribes the current <see cref="SshCommand"/> from channel events, and disposes
        /// the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <remarks>
        /// Does nothing when <paramref name="channel"/> is <see langword="null"/>.
        /// </remarks>
        private void UnsubscribeFromEventsAndDisposeChannel(IChannel channel)
        {
            if (channel is null)
            {
                return;
            }

            // unsubscribe from events as we do not want to be signaled should these get fired
            // during the dispose of the channel
            channel.DataReceived -= Channel_DataReceived;
            channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            channel.RequestReceived -= Channel_RequestReceived;
            channel.Closed -= Channel_Closed;

            // actually dispose the channel
            channel.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

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

                var inputStream = _inputStream;
                if (inputStream != null)
                {
                    inputStream.Dispose();
                    _inputStream = null;
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

                _commmandCancelledWaitHandle?.Dispose();
                _commmandCancelledWaitHandle = null;

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SshCommand"/> class.
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SshCommand"/> is reclaimed by garbage collection.
        /// </summary>
        ~SshCommand()
        {
            Dispose(disposing: false);
        }
    }
}
