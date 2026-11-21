#nullable enable
using System;
using System.Diagnostics;
using System.IO;
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
    /// Represents an SSH command that can be executed.
    /// </summary>
    public sealed class SshCommand : IDisposable
    {
        private readonly ISession _session;
        private readonly Encoding _encoding;

        /// <summary>
        /// Guards <see cref="_channel"/>, <see cref="_channelUseCount"/>, <see cref="_channelClosed"/>
        /// and <see cref="_executionCompleted"/>.
        /// </summary>
        private readonly Lock _channelLock = new Lock();

        /// <summary>
        /// The channel of the current execution, or <see langword="null"/> before the first execution
        /// and after the channel of a completed execution has been released.
        /// </summary>
        private IChannelSession? _channel;

        /// <summary>
        /// The number of threads currently blocked in a request on <see cref="_channel"/>
        /// (<see cref="IChannelSession.SendExecRequest(string)"/> or
        /// <see cref="IChannelSession.SendSignalRequest(string)"/>). The channel is not disposed
        /// while this is non-zero, because disposing it would pull the wait handle out from under
        /// the waiting thread (see #1596).
        /// </summary>
        private int _channelUseCount;

        /// <summary>
        /// Whether <see cref="_channel"/> has been closed at both ends.
        /// </summary>
        private bool _channelClosed;

        /// <summary>
        /// Whether the execution that <see cref="_channel"/> belongs to has completed.
        /// </summary>
        private bool _executionCompleted;

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
        /// Gets the number representing the exit status of the command, if applicable,
        /// otherwise <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The value is not <see langword="null"/> when an exit status code has been returned
        /// from the server. If the command terminated due to a signal, <see cref="ExitSignal"/>
        /// may be not <see langword="null"/> instead.
        /// </remarks>
        /// <seealso cref="ExitSignal"/>
        public int? ExitStatus
        {
            get
            {
                return _haveExitStatus ? _exitStatus : null;
            }
        }

        /// <summary>
        /// Gets the name of the signal due to which the command
        /// terminated violently, if applicable, otherwise <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The value (if it exists) is supplied by the server and is usually one of the
        /// following, as described in https://datatracker.ietf.org/doc/html/rfc4254#section-6.10:
        /// ABRT, ALRM, FPE, HUP, ILL, INT, KILL, PIPE, QUIT, SEGV, TER, USR1, USR2.
        /// </remarks>
        public string? ExitSignal { get; private set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        public Stream OutputStream { get; private set; }

        /// <summary>
        /// Gets the extended output stream.
        /// </summary>
        public Stream ExtendedOutputStream { get; private set; }

        /// <summary>
        /// Creates and returns the input stream for the command.
        /// </summary>
        /// <returns>
        /// The stream that can be used to transfer data to the command's input stream.
        /// </returns>
        /// <remarks>
        /// Callers should ensure that <see cref="Stream.Dispose()"/> is called on the
        /// returned instance in order to notify the command that no more data will be sent.
        /// Failure to do so may result in the command executing indefinitely.
        /// </remarks>
        /// <example>
        /// This example shows how to stream some data to 'cat' and have the server echo it back.
        /// <code>
        /// using (SshCommand command = mySshClient.CreateCommand("cat"))
        /// {
        ///     Task executeTask = command.ExecuteAsync(CancellationToken.None);
        ///
        ///     using (Stream inputStream = command.CreateInputStream())
        ///     {
        ///         inputStream.Write("Hello World!"u8);
        ///     }
        ///
        ///     await executeTask;
        ///
        ///     Console.WriteLine(command.ExitStatus); // 0
        ///     Console.WriteLine(command.Result); // "Hello World!"
        /// }
        /// </code>
        /// </example>
        public Stream CreateInputStream()
        {
            var channel = _channel;

            if (channel is not { IsOpen: true })
            {
                throw new InvalidOperationException("The input stream can be used only during execution.");
            }

            if (_inputStream != null)
            {
                throw new InvalidOperationException("The input stream already exists.");
            }

            _inputStream = new ChannelInputStream(channel);
            return _inputStream;
        }

        /// <summary>
        /// Gets the standard output of the command by reading <see cref="OutputStream"/>.
        /// </summary>
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

        /// <summary>
        /// Gets the standard error of the command by reading <see cref="ExtendedOutputStream"/>,
        /// when extended data has been sent which has been marked as stderr.
        /// </summary>
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
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(commandText);
            ArgumentNullException.ThrowIfNull(encoding);

            _session = session;
            CommandText = commandText;
            _encoding = encoding;
            CommandTimeout = Timeout.InfiniteTimeSpan;
            OutputStream = new PipeStream();
            ExtendedOutputStream = new PipeStream();

            // Note: we deliberately do not subscribe to session events or create the channel here.
            // Both are done in ExecuteAsync and undone when the execution completes, so that an
            // SshCommand which is not disposed is not kept alive by a long-lived session (#1204).
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/>. When triggered, attempts to terminate the
        /// remote command by sending a signal.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the lifetime of the command.</returns>
        /// <exception cref="InvalidOperationException">Command is already executing. Thrown synchronously.</exception>
        /// <exception cref="ObjectDisposedException">Instance has been disposed. Thrown synchronously.</exception>
        /// <exception cref="OperationCanceledException">The <see cref="Task"/> has been cancelled.</exception>
        /// <exception cref="SshOperationTimeoutException">The command timed out according to <see cref="CommandTimeout"/>.</exception>
#pragma warning disable CA1849 // Call async methods when in an async method; PipeStream.DisposeAsync would complete synchronously anyway.
        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

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

                // The channel of the previous execution has been released when that execution
                // completed; this is just a safety net.
                ReleaseChannel(force: true);

                OutputStream.Dispose();
                ExtendedOutputStream.Dispose();

                // Initialise output streams. We already initialised them for the first
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

            var channel = _session.CreateChannelSession();

            _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _userToken = cancellationToken;

            lock (_channelLock)
            {
                _channel = channel;
                _channelClosed = false;
                _executionCompleted = false;

                // This thread is about to block in SendExecRequest; don't let the channel be
                // disposed underneath it if SSH_MSG_CHANNEL_CLOSE is processed first (#1596).
                _channelUseCount = 1;
            }

            channel.DataReceived += Channel_DataReceived;
            channel.ExtendedDataReceived += Channel_ExtendedDataReceived;
            channel.RequestReceived += Channel_RequestReceived;
            channel.Closed += Channel_Closed;

            // Subscribe after the channel has been created so that the channel's own
            // session event handlers run before ours.
            SubscribeToSessionEvents();

            try
            {
                channel.Open();

                _ = channel.SendExecRequest(CommandText);
            }
            catch (Exception ex)
            {
                // The command could not be started. Don't leave the channel and the session
                // subscriptions behind, and don't leave this instance stuck "in progress".
                _ = _tcs.TrySetException(ex);
                _ = _tcs.Task.Exception; // Observed: the exception is rethrown to the caller below.

                UnsubscribeFromSessionEvents();
                EndChannelUse(channel);
                ReleaseChannel(force: true);

                OutputStream.Dispose();
                ExtendedOutputStream.Dispose();

                throw;
            }

            EndChannelUse(channel);

            if (CommandTimeout != Timeout.InfiniteTimeSpan)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _cts.CancelAfter(CommandTimeout);
                cancellationToken = _cts.Token;
            }

            if (cancellationToken.CanBeCanceled)
            {
                _tokenRegistration = cancellationToken.Register(static cmd =>
                {
                    try
                    {
                        ((SshCommand)cmd!).CancelAsync();
                    }
                    catch
                    {
                        // Swallow exceptions which would otherwise be unhandled.
                    }
                },
                this);
            }

            return _tcs.Task;
        }
#pragma warning restore CA1849

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
        public IAsyncResult BeginExecute(AsyncCallback? callback)
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
        public IAsyncResult BeginExecute(AsyncCallback? callback, object? state)
        {
            return TaskToAsyncResult.Begin(ExecuteAsync(), callback, state);
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
        public IAsyncResult BeginExecute(string commandText, AsyncCallback? callback, object? state)
        {
            ArgumentNullException.ThrowIfNull(commandText);

            CommandText = commandText;

            return BeginExecute(callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous command execution to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see cref="Result"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="asyncResult"/> does not correspond to the currently executing command.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
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

        /// <summary>
        /// Cancels a running command by sending a signal to the remote process.
        /// </summary>
        /// <param name="forceKill">if true send SIGKILL instead of SIGTERM.</param>
        /// <param name="millisecondsTimeout">Time to wait for the server to reply.</param>
        /// <remarks>
        /// <para>
        /// This method stops the command running on the server by sending a SIGTERM
        /// (or SIGKILL, depending on <paramref name="forceKill"/>) signal to the remote
        /// process. When the server implements signals, it will send a response which
        /// populates <see cref="ExitSignal"/> with the signal with which the command terminated.
        /// </para>
        /// <para>
        /// When the server does not implement signals, it may send no response. As a fallback,
        /// this method waits up to <paramref name="millisecondsTimeout"/> for a response
        /// and then completes the <see cref="SshCommand"/> object anyway if there was none.
        /// </para>
        /// <para>
        /// If the command has already finished (with or without cancellation), this method does
        /// nothing.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Command has not been started.</exception>
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

            try
            {
                var channel = BeginChannelUse();

                if (channel is null)
                {
                    // Command has completed (in the meantime since the last check).
                    return;
                }

                try
                {
                    // Try to send the cancellation signal.
                    _ = channel.SendSignalRequest(forceKill ? "KILL" : "TERM");
                }
                finally
                {
                    EndChannelUse(channel);
                }

                // Having sent the "signal" message, we expect to receive "exit-signal"
                // and then a close message. But since a server may not implement signals,
                // we can't guarantee that, so we wait a short time for that to happen and
                // if it doesn't, just complete the task ourselves to unblock waiters.

                _ = _tcs.Task.Wait(millisecondsTimeout);
            }
            catch (AggregateException)
            {
                // We expect to be here from the call to Wait if the server implements signals.
                // But we don't want to propagate the exception on the task from here.
            }
            finally
            {
                SetAsyncComplete();
            }
        }

        /// <summary>
        /// Executes the command specified by <see cref="CommandText"/>.
        /// </summary>
        /// <returns><see cref="Result"/>.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public string Execute()
        {
            ExecuteAsync().GetAwaiter().GetResult();

            return Result;
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns><see cref="Result"/>.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public string Execute(string commandText)
        {
            CommandText = commandText;

            return Execute();
        }

        private void Session_Disconnected(object? sender, EventArgs e)
        {
            SetAsyncComplete(new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost));
        }

        private void Session_ErrorOccurred(object? sender, ExceptionEventArgs e)
        {
            SetAsyncComplete(e.Exception);
        }

        private void SetAsyncComplete(Exception? exception = null)
        {
            // Clean up before completing the task: once the task has completed, the instance may be
            // executed again (e.g. on a continuation), and this cleanup must not then affect the
            // session subscriptions or output streams of that new execution.

            // The execution is over: stop listening to the session so that the session (which
            // typically lives much longer than a command) no longer references this instance.
            UnsubscribeFromSessionEvents();

            // Release the channel unless a thread is blocked in a request on it, in which case that
            // thread releases it when the request returns (see EndChannelUse).
            lock (_channelLock)
            {
                _executionCompleted = true;
            }

            ReleaseChannel(force: false);

            OutputStream.Dispose();
            ExtendedOutputStream.Dispose();

            if (exception is not null)
            {
                _ = _tcs?.TrySetException(exception);
                return;
            }

            Interlocked.MemoryBarrier(); // ensure fresh read of _cancellationRequested (possibly unnecessary)

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

        private void Channel_Closed(object? sender, ChannelEventArgs e)
        {
            lock (_channelLock)
            {
                _channelClosed = true;
            }

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
            else if (e.Info.WantReply && sender is IChannel { RemoteChannelNumber: uint remoteChannelNumber })
            {
                var replyMessage = new ChannelFailureMessage(remoteChannelNumber);
                _session.SendMessage(replyMessage);
            }
        }

        private void Channel_ExtendedDataReceived(object? sender, ChannelExtendedDataEventArgs e)
        {
            ExtendedOutputStream.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);

            if (e.DataTypeCode == 1)
            {
                _hasError = true;
            }
        }

        private void Channel_DataReceived(object? sender, ChannelDataEventArgs e)
        {
            OutputStream.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
        }

        private void SubscribeToSessionEvents()
        {
            // Guard against a double subscription should a previous execution not have unsubscribed.
            UnsubscribeFromSessionEvents();

            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccurred;
        }

        private void UnsubscribeFromSessionEvents()
        {
            _session.Disconnected -= Session_Disconnected;
            _session.ErrorOccured -= Session_ErrorOccurred;
        }

        /// <summary>
        /// Marks the start of a blocking request on the current channel.
        /// </summary>
        /// <returns>
        /// The channel, or <see langword="null"/> if there is no channel or it has already been closed.
        /// Each non-null return must be paired with a call to <see cref="EndChannelUse(IChannelSession)"/>.
        /// </returns>
        private IChannelSession? BeginChannelUse()
        {
            lock (_channelLock)
            {
                if (_channel is null || _channelClosed)
                {
                    return null;
                }

                _channelUseCount++;
                return _channel;
            }
        }

        /// <summary>
        /// Marks the end of a blocking request on <paramref name="channel"/>, and releases the channel
        /// if the execution has completed in the meantime.
        /// </summary>
        /// <param name="channel">The channel returned by <see cref="BeginChannelUse"/>.</param>
        private void EndChannelUse(IChannelSession channel)
        {
            lock (_channelLock)
            {
                if (!ReferenceEquals(channel, _channel))
                {
                    // That channel has already been released (forcibly, by Dispose or a subsequent
                    // execution); the count now belongs to another channel (or none).
                    return;
                }

                Debug.Assert(_channelUseCount > 0, "EndChannelUse without matching use.");

                _channelUseCount--;
            }

            ReleaseChannel(force: false);
        }

        /// <summary>
        /// Unsubscribes from the events of <see cref="_channel"/>, disposes it and drops the reference to it.
        /// </summary>
        /// <param name="force">
        /// <see langword="false"/> to only release the channel when its execution has completed and no
        /// thread is blocked in a request on it; <see langword="true"/> to release it unconditionally
        /// (and synchronously).
        /// </param>
        private void ReleaseChannel(bool force)
        {
            IChannelSession? channel;
            bool channelClosed;

            lock (_channelLock)
            {
                channel = _channel;

                if (channel is null)
                {
                    return;
                }

                if (!force && (!_executionCompleted || _channelUseCount > 0))
                {
                    return;
                }

                channelClosed = _channelClosed;
                _channel = null;
            }

            // unsubscribe from events as we do not want to be signaled should these get fired
            // during the dispose of the channel
            channel.DataReceived -= Channel_DataReceived;
            channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            channel.RequestReceived -= Channel_RequestReceived;
            channel.Closed -= Channel_Closed;

            // Disposing the channel unsubscribes it from the session's events and releases its
            // wait handles. If the channel has been closed this does not send any message nor block.
            if (force || channelClosed)
            {
                channel.Dispose();
                return;
            }

            // The execution completed while the channel is still open (cancellation without a reply
            // from the server, session error/disconnect). Disposing it then sends SSH_MSG_CHANNEL_CLOSE
            // and waits up to ConnectionInfo.ChannelCloseTimeout for the server to close it, which we
            // don't want to do on the thread completing the execution (possibly the message loop).
            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    channel.Dispose();
                }
                catch
                {
                    // Swallow exceptions which would otherwise be unhandled.
                }
            });
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
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // unsubscribe from session events to ensure other objects that we're going to dispose
                // are not accessed while disposing
                UnsubscribeFromSessionEvents();

                // unsubscribe from channel events to ensure other objects that we're going to dispose
                // are not accessed while disposing
                ReleaseChannel(force: true);

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
