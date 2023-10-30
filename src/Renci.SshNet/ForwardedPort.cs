using System;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for port forwarding functionality.
    /// </summary>
    public abstract class ForwardedPort : IForwardedPort
    {
        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        internal ISession Session { get; set; }

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if port forwarding is started; otherwise, <see langword="false"/>.
        /// </value>
        public abstract bool IsStarted { get; }

        /// <summary>
        /// The <see cref="Closing"/> event occurs as the forwarded port is being stopped.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs when an exception is thrown.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Occurs when a port forwarding request is received.
        /// </summary>
        public event EventHandler<PortForwardEventArgs> RequestReceived;

        /// <summary>
        /// Starts port forwarding.
        /// </summary>
        /// <exception cref="InvalidOperationException">The current <see cref="ForwardedPort"/> is already started -or- is not linked to a SSH session.</exception>
        /// <exception cref="SshConnectionException">The client is not connected.</exception>
        public virtual void Start()
        {
            CheckDisposed();

            if (IsStarted)
            {
                throw new InvalidOperationException("Forwarded port is already started.");
            }

            if (Session is null)
            {
                throw new InvalidOperationException("Forwarded port is not added to a client.");
            }

            if (!Session.IsConnected)
            {
                throw new SshConnectionException("Client not connected.");
            }

            Session.ErrorOccured += Session_ErrorOccured;
            StartPort();
        }

        /// <summary>
        /// Stops port forwarding.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public virtual void Stop()
#pragma warning restore CA1716 // Identifiers should not match keywords
        {
            if (IsStarted)
            {
                StopPort(Session.ConnectionInfo.Timeout);
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
        /// Starts port forwarding.
        /// </summary>
        protected abstract void StartPort();

        /// <summary>
        /// Stops port forwarding, and waits for the specified timeout until all pending
        /// requests are processed.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for pending requests to finish processing.</param>
        protected virtual void StopPort(TimeSpan timeout)
        {
            RaiseClosing();

            var session = Session;
            if (session is not null)
            {
                session.ErrorOccured -= Session_ErrorOccured;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langowrd="true"/> to release both managed and unmanaged resources; <see langowrd="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var session = Session;
                if (session is not null)
                {
                    StopPort(session.ConnectionInfo.Timeout);
                    Session = null;
                }
            }
        }

        /// <summary>
        /// Ensures the current instance is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance is disposed.</exception>
        protected abstract void CheckDisposed();

        /// <summary>
        /// Raises <see cref="Exception"/> event.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected void RaiseExceptionEvent(Exception exception)
        {
            Exception?.Invoke(this, new ExceptionEventArgs(exception));
        }

        /// <summary>
        /// Raises <see cref="RequestReceived"/> event.
        /// </summary>
        /// <param name="host">Request originator host.</param>
        /// <param name="port">Request originator port.</param>
        protected void RaiseRequestReceived(string host, uint port)
        {
            RequestReceived?.Invoke(this, new PortForwardEventArgs(host, port));
        }

        /// <summary>
        /// Raises the <see cref="IForwardedPort.Closing"/> event.
        /// </summary>
        private void RaiseClosing()
        {
            Closing?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles session ErrorOccured event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionEventArgs"/> instance containing the event data.</param>
        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            RaiseExceptionEvent(e.Exception);
        }
    }
}
