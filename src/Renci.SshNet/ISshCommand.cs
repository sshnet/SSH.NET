#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents an SSH command that can be executed.
    /// </summary>
    public interface ISshCommand : IDisposable
    {
        /// <summary>
        /// Gets the command text.
        /// </summary>
        string CommandText { get; }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        /// <value>
        /// The command timeout.
        /// </value>
        TimeSpan CommandTimeout { get; set; }

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
        int? ExitStatus { get; }

        /// <summary>
        /// Gets the name of the signal due to which the command
        /// terminated violently, if applicable, otherwise <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// The value (if it exists) is supplied by the server and is usually one of the
        /// following, as described in https://datatracker.ietf.org/doc/html/rfc4254#section-6.10:
        /// ABRT, ALRM, FPE, HUP, ILL, INT, KILL, PIPE, QUIT, SEGV, TER, USR1, USR2.
        /// </remarks>
        string? ExitSignal { get; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        Stream OutputStream { get; }

        /// <summary>
        /// Gets the extended output stream.
        /// </summary>
        Stream ExtendedOutputStream { get; }

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
        Stream CreateInputStream();

        /// <summary>
        /// Gets the standard output of the command by reading <see cref="OutputStream"/>.
        /// </summary>
        string Result { get; }

        /// <summary>
        /// Gets the standard error of the command by reading <see cref="ExtendedOutputStream"/>,
        /// when extended data has been sent which has been marked as stderr.
        /// </summary>
        string Error { get; }

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
        Task ExecuteAsync(CancellationToken cancellationToken = default);

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
        IAsyncResult BeginExecute();

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
        IAsyncResult BeginExecute(AsyncCallback? callback);

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
        IAsyncResult BeginExecute(AsyncCallback? callback, object? state);

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
        IAsyncResult BeginExecute(string commandText, AsyncCallback? callback, object? state);

        /// <summary>
        /// Waits for the pending asynchronous command execution to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see cref="Result"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="asyncResult"/> does not correspond to the currently executing command.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <see langword="null"/>.</exception>
        string EndExecute(IAsyncResult asyncResult);

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
        void CancelAsync(bool forceKill = false, int millisecondsTimeout = 500);

        /// <summary>
        /// Executes the command specified by <see cref="CommandText"/>.
        /// </summary>
        /// <returns><see cref="Result"/>.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        string Execute();

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns><see cref="Result"/>.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SshOperationTimeoutException">Operation has timed out.</exception>
        string Execute(string commandText);
    }
}
