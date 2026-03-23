#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    /// <inheritdoc cref="V2.SshCommand" />
    [Obsolete($"Use {nameof(V2.ISshCommand)} instead.")]
    public sealed class SshCommand : V2.ISshCommand
    {
        private readonly V2.SshCommand _sshCommand;

        internal SshCommand(V2.SshCommand sshCommand)
        {
            _sshCommand = sshCommand;
        }

        /// <inheritdoc />
        public string CommandText
        {
            get
            {
                return _sshCommand.CommandText;
            }
        }

        /// <inheritdoc />
        public TimeSpan CommandTimeout
        {
            get
            {
                return _sshCommand.CommandTimeout;
            }

            set
            {
                _sshCommand.CommandTimeout = value;
            }
        }

        /// <inheritdoc />
        public int? ExitStatus
        {
            get
            {
                return _sshCommand.ExitStatus;
            }
        }

        /// <inheritdoc />
        public string? ExitSignal
        {
            get
            {
                return _sshCommand.ExitSignal;
            }
        }

        /// <inheritdoc />
        public Stream OutputStream
        {
            get
            {
                return _sshCommand.OutputStream;
            }
        }

        /// <inheritdoc />
        public Stream ExtendedOutputStream
        {
            get
            {
                return _sshCommand.ExtendedOutputStream;
            }
        }

        /// <inheritdoc />
        public string Result
        {
            get
            {
                return _sshCommand.Result;
            }
        }

        /// <inheritdoc />
        public string Error
        {
            get
            {
                return _sshCommand.Error;
            }
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute()
        {
            return _sshCommand.BeginExecute();
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(AsyncCallback? callback)
        {
            return _sshCommand.BeginExecute(callback);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(AsyncCallback? callback, object? state)
        {
            return _sshCommand.BeginExecute(callback, state);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(string commandText, AsyncCallback? callback, object? state)
        {
            return _sshCommand.BeginExecute(commandText, callback, state);
        }

        /// <inheritdoc />
        public void CancelAsync(bool forceKill = false, int millisecondsTimeout = 500)
        {
            _sshCommand.CancelAsync(forceKill, millisecondsTimeout);
        }

        /// <inheritdoc />
        public Stream CreateInputStream()
        {
            return _sshCommand.CreateInputStream();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _sshCommand.Dispose();
        }

        /// <inheritdoc />
        public string EndExecute(IAsyncResult asyncResult)
        {
            return _sshCommand.EndExecute(asyncResult);
        }

        /// <inheritdoc />
        public string Execute()
        {
            return _sshCommand.Execute();
        }

        /// <inheritdoc />
        public string Execute(string commandText)
        {
            return _sshCommand.Execute(commandText);
        }

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return _sshCommand.ExecuteAsync(cancellationToken);
        }
    }
}