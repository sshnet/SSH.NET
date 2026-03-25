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
    /// <summary>
    /// Represents an SSH command that can be executed.
    /// </summary>
    public sealed class SshCommandLite : IDisposable
    {
        private readonly ISession _session;
        private readonly Encoding _encoding;

        private IChannelSession _channel;
        private TaskCompletionSource<object>? _tcs;
        private CancellationTokenSource? _cts;
        private CancellationTokenRegistration _tokenRegistration;
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
        /// Gets the command input and output encoding.
        /// </summary>
        public Encoding CommandEncoding
        {
            get
            {
                return _encoding;
            }
        }

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
        /// Occurs when output is received.
        /// </summary>
        public event EventHandler<CommandOutputEventArgs>? OutputReceived;

        /// <summary>
        /// Occurs when ExtendedOutput is received.
        /// </summary>
        public event EventHandler<ExtendedCommandEventArgs>? ExtendedOutputReceived;

        /// <summary>
        /// Occurs when the command has finished executing and the channel has been closed.
        /// Returns the exit status code if it was provided by the server, or <see langword="null"/> otherwise.
        /// </summary>
        public event EventHandler<CommandExitedEventArgs>? Exited;

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
            if (!_channel.IsOpen)
            {
                throw new InvalidOperationException("The input stream can be used only during execution.");
            }

            if (_inputStream != null)
            {
                throw new InvalidOperationException("The input stream already exists.");
            }

            _inputStream = new ChannelInputStream(_channel);
            return _inputStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshCommandLite"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding to use for the results.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="session"/>, <paramref name="commandText"/> is <see langword="null"/>.</exception>
        internal SshCommandLite(ISession session, string commandText, Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(commandText);
            ArgumentNullException.ThrowIfNull(encoding);

            _session = session;
            CommandText = commandText;
            _encoding = encoding;
            CommandTimeout = Timeout.InfiniteTimeSpan;
            _session.Disconnected += Session_Disconnected;
            _session.ErrorOccured += Session_ErrorOccurred;
            _channel = _session.CreateChannelSession();
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

                UnsubscribeFromChannelEvents(dispose: true);

                _channel = _session.CreateChannelSession();
            }

            _exitStatus = default;
            _haveExitStatus = false;
            ExitSignal = null;
            _tokenRegistration.Dispose();
            _tokenRegistration = default;
            _cts?.Dispose();
            _cts = null;
            _cancellationRequested = false;

            _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _userToken = cancellationToken;

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

        private static string? GetSignalName(CommandSignal signal)
        {
#if NETCOREAPP
            return Enum.GetName(signal);
#else

            // Boxes signal, but Enum.GetName does not have a non-boxing overload prior to .NET Core.
            return Enum.GetName(typeof(CommandSignal), signal);
#endif
        }

        /// <summary>
        /// Tries to send a POSIX/ANSI signal to the remote process executing the command, such as SIGINT or SIGTERM.
        /// </summary>
        /// <param name="signal">The signal to send</param>
        /// <returns>If the signal was sent.</returns>
        public bool TrySendSignal(CommandSignal signal)
        {
            var signalName = GetSignalName(signal);
            if (signalName is null)
            {
                return false;
            }

            if (_tcs is null || _tcs.Task.IsCompleted || _channel?.IsOpen != true)
            {
                return false;
            }

            try
            {
                // Try to send the cancellation signal.
                return _channel.SendSignalRequest(signalName);
            }
            catch (Exception)
            {
                // Exception can be ignored since we are in a Try method
                // Possible exceptions here: InvalidOperationException, SshConnectionException, SshOperationTimeoutException
            }

            return false;
        }

        /// <summary>
        /// Tries to send a POSIX/ANSI signal to the remote process executing the command, such as SIGINT or SIGTERM.
        /// </summary>
        /// <param name="signal">The signal to send</param>
        /// <exception cref="ArgumentException">Signal was not a valid CommandSignal.</exception>
        /// <exception cref="SshConnectionException">The client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">The operation timed out.</exception>
        /// <exception cref="InvalidOperationException">The size of the packet exceeds the maximum size defined by the protocol.</exception>
        /// <exception cref="InvalidOperationException">Command has not been started.</exception>
        public void SendSignal(CommandSignal signal)
        {
            var signalName = GetSignalName(signal);
            if (signalName is null)
            {
                throw new ArgumentException("Signal was not a valid CommandSignal.");
            }
            if (_tcs is null || _tcs.Task.IsCompleted || _channel?.IsOpen != true)
            {
                throw new InvalidOperationException("Command has not been started.");
            }

            _ = _channel.SendSignalRequest(signalName);
        }

        /// <summary>
        /// Executes the command specified by <see cref="CommandText"/>.
        /// </summary>
        /// <returns><see cref="ExitStatus"/>.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public int? Execute()
        {
            ExecuteAsync().GetAwaiter().GetResult();
            return ExitStatus;
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns><see cref="ExitStatus"/>.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        public int? Execute(string commandText)
        {
            CommandText = commandText;

            return Execute();
        }

        private void Session_Disconnected(object? sender, EventArgs e)
        {
            _ = _tcs?.TrySetException(new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost));

            SetAsyncComplete(setResult: false);
        }

        private void Session_ErrorOccurred(object? sender, ExceptionEventArgs e)
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

            // We don't dispose the channel here to avoid a race condition
            // where SSH_MSG_CHANNEL_CLOSE arrives before _channel starts
            // waiting for a response in _channel.SendExecRequest().
            UnsubscribeFromChannelEvents(dispose: false);

            Exited?.Invoke(this, new(ExitStatus, ExitSignal));
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
            else if (e.Info.WantReply && sender is IChannel { RemoteChannelNumber: uint remoteChannelNumber })
            {
                var replyMessage = new ChannelFailureMessage(remoteChannelNumber);
                _session.SendMessage(replyMessage);
            }
        }

        private void Channel_ExtendedDataReceived(object? sender, ChannelExtendedDataEventArgs e)
        {
            ExtendedOutputReceived?.Invoke(this, new(e.Data, _encoding, e.DataTypeCode));
        }

        private void Channel_DataReceived(object? sender, ChannelDataEventArgs e)
        {
            OutputReceived?.Invoke(this, new(e.Data, _encoding));
        }

        /// <summary>
        /// Unsubscribes the current <see cref="SshCommand"/> from channel events, and optionally,
        /// disposes <see cref="_channel"/>.
        /// </summary>
        private void UnsubscribeFromChannelEvents(bool dispose)
        {
            var channel = _channel;

            // unsubscribe from events as we do not want to be signaled should these get fired
            // during the dispose of the channel
            channel.DataReceived -= Channel_DataReceived;
            channel.ExtendedDataReceived -= Channel_ExtendedDataReceived;
            channel.RequestReceived -= Channel_RequestReceived;
            channel.Closed -= Channel_Closed;

            if (dispose)
            {
                channel.Dispose();
            }
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
                _session.Disconnected -= Session_Disconnected;
                _session.ErrorOccured -= Session_ErrorOccurred;

                // unsubscribe from channel events to ensure other objects that we're going to dispose
                // are not accessed while disposing
                UnsubscribeFromChannelEvents(dispose: true);

                _inputStream?.Dispose();
                _inputStream = null;

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
