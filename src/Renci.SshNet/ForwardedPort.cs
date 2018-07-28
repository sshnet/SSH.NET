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
        /// The <see cref="Closing"/> event occurs as the forwarded port is being stopped.
        /// </summary>
        internal event EventHandler Closing;

        /// <summary>
        /// The <see cref="IForwardedPort.Closing"/> event occurs as the forwarded port is being stopped.
        /// </summary>
        event EventHandler IForwardedPort.Closing
        {
            add { Closing += value; }
            remove { Closing -= value; }
        }

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsStarted { get; }

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
        public virtual void Start()
        {
            CheckDisposed();

            if (IsStarted)
                throw new InvalidOperationException("Forwarded port is already started.");
            if (Session == null)
                throw new InvalidOperationException("Forwarded port is not added to a client.");
            if (!Session.IsConnected)
                throw new SshConnectionException("Client not connected.");

            Session.ErrorOccured += Session_ErrorOccured;
            StartPort();
        }

        /// <summary>
        /// Stops port forwarding.
        /// </summary>
        public virtual void Stop()
        {
            if (IsStarted)
            {
                StopPort(Session.ConnectionInfo.Timeout);
            }
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
            if (session != null)
            {
                session.ErrorOccured -= Session_ErrorOccured;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var session = Session;
                if (session != null)
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
            var handlers = Exception;
            if (handlers != null)
            {
                handlers(this, new ExceptionEventArgs(exception));
            }
        }

        /// <summary>
        /// Raises <see cref="RequestReceived"/> event.
        /// </summary>
        /// <param name="host">Request originator host.</param>
        /// <param name="port">Request originator port.</param>
        protected void RaiseRequestReceived(string host, uint port)
        {
            var handlers = RequestReceived;
            if (handlers != null)
            {
                handlers(this, new PortForwardEventArgs(host, port));
            }
        }

        /// <summary>
        /// Raises the <see cref="IForwardedPort.Closing"/> event.
        /// </summary>
        private void RaiseClosing()
        {
            var handlers = Closing;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
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
