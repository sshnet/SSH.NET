#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    /// <inheritdoc />
    public class SshCommand : ISshCommand
    {
        private readonly ISession _session;
        private readonly Encoding _encoding;

        private IChannelSession? _channel;
        private TaskCompletionSource<object>? _tcs;
        private CancellationTokenSource? _cts;
        private CancellationTokenRegistration _tokenRegistration;
        private string? _stdOut;
        private string? _stdErr;
        private bool _hasError;
        private bool _isDisposed;
        private ChannelInputStream? _inputStream;
        private TimeSpan _commandTimeout;

        /// <summary>
        /// The token supplied as an argument to <see cref="ExecuteAsync(CancellationToken)"/>.
        /// </summary>
        private CancellationToken _userToken;

        /// <summary>
        /// Whether <see cref="CancelAsync(bool, int)"/> has been called
        /// (either by a token or manually).
        /// </summary>
        private bool _cancellationRequested;

        private int _exitStatus;
        private volatile bool _haveExitStatus; // volatile to prevent re-ordering of reads/writes of _exitStatus.

        /// <inheritdoc />
        public string CommandText { get; private set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int? ExitStatus
        {
            get
            {
                return _haveExitStatus ? _exitStatus : null;
            }
        }

        /// <inheritdoc />
        public string? ExitSignal { get; private set; }

        /// <inheritdoc />
        public Stream OutputStream { get; private set; }

        /// <inheritdoc />
        public Stream ExtendedOutputStream { get; private set; }

        /// <inheritdoc />
        public Stream CreateInputStream()
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

        /// <inheritdoc />
        public string Result
        {
            get
            {
                if (_stdOut is not null)
                {
                    return _stdOut;
                }

                if (_tcs is null)
                {
                    return string.Empty;
                }

                using (var sr = new StreamReader(OutputStream, _encoding))
                {
                    return _stdOut = sr.ReadToEnd();
                }
            }
        }

        /// <inheritdoc />
        public string Error
        {
            get
            {
                if (_stdErr is not null)
                {
                    return _stdErr;
                }

                if (_tcs is null || !_hasError)
                {
                    return string.Empty;
                }

                using (var sr = new StreamReader(ExtendedOutputStream, _encoding))
                {
                    return _stdErr = sr.ReadToEnd();
                }
            }
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
            ThrowHelper.ThrowIfNull(session);
            ThrowHelper.ThrowIfNull(commandText);
            ThrowHelper.ThrowIfNull(encoding);

            _session = session;
            CommandText = commandText;
            _encoding = encoding;
            CommandTimeout = Timeout.InfiniteTimeSpan;
            OutputStream = new PipeStream();
            ExtendedOutputStream = new PipeStream();
            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccured;
        }

        /// <inheritdoc />
#pragma warning disable CA1849 // Call async methods when in an async method; PipeStream.DisposeAsync would complete synchronously anyway.
        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowObjectDisposedIf(_isDisposed, this);

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (_tcs is not null)
            {
                if (!_tcs.Task.IsCompleted)
                {
                    throw new InvalidOperationException("Asynchronous operation is already in progress.");
                }

                OutputStream.Dispose();
                ExtendedOutputStream.Dispose();

                // Initialize output streams. We already initialised them for the first
                // execution in the constructor (to allow passing them around before execution)
                // so we just need to reinitialise them for subsequent executions.
                OutputStream = new PipeStream();
                ExtendedOutputStream = new PipeStream();
            }

            _exitStatus = default;
            _haveExitStatus = false;
            ExitSignal = null;
            _stdOut = null;
            _stdErr = null;
            _hasError = false;
            _tokenRegistration.Dispose();
            _tokenRegistration = default;
            _cts?.Dispose();
            _cts = null;
            _cancellationRequested = false;

            _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _userToken = cancellationToken;

            _channel = _session.CreateChannelSession();
            _channel.DataReceived += Channel_DataReceived;
            _channel.ExtendedDataReceived += Channel_ExtendedDataReceived;
            _channel.RequestReceived += Channel_RequestReceived;
            _channel.Closed += Channel_Closed;
            _channel.Open();

            _ = _channel.SendExecRequest(CommandText);

            if (CommandTimeout != Timeout.InfiniteTimeSpan)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _cts.CancelAfter(CommandTimeout);
                cancellationToken = _cts.Token;
            }

            if (cancellationToken.CanBeCanceled)
            {
                _tokenRegistration = cancellationToken.Register(static cmd => ((SshCommand)cmd!).CancelAsync(), this);
            }

            return _tcs.Task;
        }
#pragma warning restore CA1849

        /// <inheritdoc />
        public IAsyncResult BeginExecute()
        {
            return BeginExecute(callback: null, state: null);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(AsyncCallback? callback)
        {
            return BeginExecute(callback, state: null);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(AsyncCallback? callback, object? state)
        {
            return TaskToAsyncResult.Begin(ExecuteAsync(), callback, state);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(string commandText, AsyncCallback? callback, object? state)
        {
            ThrowHelper.ThrowIfNull(commandText);

            CommandText = commandText;

            return BeginExecute(callback, state);
        }

        /// <inheritdoc />
        public string EndExecute(IAsyncResult asyncResult)
        {
            var executeTask = TaskToAsyncResult.Unwrap(asyncResult);

            if (executeTask != _tcs?.Task)
            {
                throw new ArgumentException("Argument does not correspond to the currently executing command.", nameof(asyncResult));
            }

            executeTask.GetAwaiter().GetResult();

            return Result;
        }

        /// <inheritdoc />
        public void CancelAsync(bool forceKill = false, int millisecondsTimeout = 500)
        {
            if (_tcs is null)
            {
                throw new InvalidOperationException("Command has not been started.");
            }

            if (_tcs.Task.IsCompleted)
            {
                return;
            }

            _cancellationRequested = true;
            Interlocked.MemoryBarrier(); // ensure fresh read in SetAsyncComplete (possibly unnecessary)

            // Try to send the cancellation signal.
            if (_channel?.SendSignalRequest(forceKill ? "KILL" : "TERM") is null)
            {
                // Command has completed (in the meantime since the last check).
                return;
            }

            // Having sent the "signal" message, we expect to receive "exit-signal"
            // and then a close message. But since a server may not implement signals,
            // we can't guarantee that, so we wait a short time for that to happen and
            // if it doesn't, just complete the task ourselves to unblock waiters.

            try
            {
                if (_tcs.Task.Wait(millisecondsTimeout))
                {
                    return;
                }
            }
            catch (AggregateException)
            {
                // We expect to be here if the server implements signals.
                // But we don't want to propagate the exception on the task from here.
                return;
            }

            SetAsyncComplete();
        }

        /// <inheritdoc />
        public string Execute()
        {
            ExecuteAsync().GetAwaiter().GetResult();

            return Result;
        }

        /// <inheritdoc />
        public string Execute(string commandText)
        {
            CommandText = commandText;

            return Execute();
        }

        private void Session_Disconnected(object? sender, EventArgs e)
        {
            _ = _tcs?.TrySetException(new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost));

            SetAsyncComplete(setResult: false);
        }

        private void Session_ErrorOccured(object? sender, ExceptionEventArgs e)
        {
            _ = _tcs?.TrySetException(e.Exception);

            SetAsyncComplete(setResult: false);
        }

        private void SetAsyncComplete(bool setResult = true)
        {
            Interlocked.MemoryBarrier(); // ensure fresh read of _cancellationRequested (possibly unnecessary)

            if (setResult)
            {
                Debug.Assert(_tcs is not null, "Should only be completing the task if we've started one.");

                if (_userToken.IsCancellationRequested)
                {
                    _ = _tcs.TrySetCanceled(_userToken);
                }
                else if (_cts?.Token.IsCancellationRequested == true)
                {
                    _ = _tcs.TrySetException(new SshOperationTimeoutException($"Command '{CommandText}' timed out. ({nameof(CommandTimeout)}: {CommandTimeout})."));
                }
                else if (_cancellationRequested)
                {
                    _ = _tcs.TrySetCanceled();
                }
                else
                {
                    _ = _tcs.TrySetResult(null!);
                }
            }

            UnsubscribeFromEventsAndDisposeChannel();

            OutputStream.Dispose();
            ExtendedOutputStream.Dispose();
        }

        private void Channel_Closed(object? sender, ChannelEventArgs e)
        {
            SetAsyncComplete();
        }

        private void Channel_RequestReceived(object? sender, ChannelRequestEventArgs e)
        {
            if (e.Info is ExitStatusRequestInfo exitStatusInfo)
            {
                _exitStatus = (int)exitStatusInfo.ExitStatus;
                _haveExitStatus = true;

                Debug.Assert(!exitStatusInfo.WantReply, "exit-status is want_reply := false by definition.");
            }
            else if (e.Info is ExitSignalRequestInfo exitSignalInfo)
            {
                ExitSignal = exitSignalInfo.SignalName;

                Debug.Assert(!exitSignalInfo.WantReply, "exit-signal is want_reply := false by definition.");
            }
            else if (e.Info.WantReply && _channel?.RemoteChannelNumber is uint remoteChannelNumber)
            {
                var replyMessage = new ChannelFailureMessage(remoteChannelNumber);
                _session.SendMessage(replyMessage);
            }
        }

        private void Channel_ExtendedDataReceived(object? sender, ChannelExtendedDataEventArgs e)
        {
            ExtendedOutputStream.Write(e.Data, 0, e.Data.Length);

            if (e.DataTypeCode == 1)
            {
                _hasError = true;
            }
        }

        private void Channel_DataReceived(object? sender, ChannelDataEventArgs e)
        {
            OutputStream.Write(e.Data, 0, e.Data.Length);
        }

        /// <summary>
        /// Unsubscribes the current <see cref="SshCommand"/> from channel events, and disposes
        /// the <see cref="_channel"/>.
        /// </summary>
        private void UnsubscribeFromEventsAndDisposeChannel()
        {
            var channel = _channel;

            if (channel is null)
            {
                return;
            }

            _channel = null;

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
                _session.Disconnected -= Session_Disconnected;
                _session.ErrorOccured -= Session_ErrorOccured;

                // unsubscribe from channel events to ensure other objects that we're going to dispose
                // are not accessed while disposing
                UnsubscribeFromEventsAndDisposeChannel();

                _inputStream?.Dispose();
                _inputStream = null;

                OutputStream.Dispose();
                ExtendedOutputStream.Dispose();

                _tokenRegistration.Dispose();
                _tokenRegistration = default;
                _cts?.Dispose();
                _cts = null;

                if (_tcs is { Task.IsCompleted: false } tcs)
                {
                    // In case an operation is still running, try to complete it with an ObjectDisposedException.
                    _ = tcs.TrySetException(new ObjectDisposedException(GetType().FullName));
                }

                _isDisposed = true;
            }
        }
    }
}
