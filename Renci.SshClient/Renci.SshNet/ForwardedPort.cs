using System;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for port forwarding functionality.
    /// </summary>
    public abstract class ForwardedPort
    {
        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        internal Session Session { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether port forwarding started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if port forwarding started; otherwise, <c>false</c>.
        /// </value>
        public bool IsStarted { get; protected set; }

        /// <summary>
        /// Occurs when exception is thrown.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Occurs when port forwarding request received.
        /// </summary>
        public event EventHandler<PortForwardEventArgs> RequestReceived;

        /// <summary>
        /// Starts port forwarding.
        /// </summary>
        public virtual void Start()
        {
            if (this.Session == null)
            {
                throw new InvalidOperationException("Session property is null.");
            }

            if (!this.Session.IsConnected)
            {
                throw new SshConnectionException("Not connected.");
            }

            this.Session.ErrorOccured += Session_ErrorOccured;
        }

        /// <summary>
        /// Stops port forwarding.
        /// </summary>
        public virtual void Stop()
        {
            if (this.Session != null)
            {
                this.Session.ErrorOccured -= Session_ErrorOccured;
            }
        }

        /// <summary>
        /// Raises <see cref="Renci.SshNet.ForwardedPort.Exception"/> event.
        /// </summary>
        /// <param name="execption">The exception.</param>
        protected void RaiseExceptionEvent(Exception execption)
        {
            if (this.Exception != null)
            {
                this.Exception(this, new ExceptionEventArgs(execption));
            }
        }

        /// <summary>
        /// Raises <see cref="Renci.SshNet.ForwardedPort.RequestReceived"/> event.
        /// </summary>
        /// <param name="host">Request originator host.</param>
        /// <param name="port">Request originator port.</param>
        protected void RaiseRequestReceived(string host, uint port)
        {
            if (this.RequestReceived != null)
            {
                this.RequestReceived(this, new PortForwardEventArgs(host, port));
            }
        }

        /// <summary>
        /// Handles session ErrorOccured event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionEventArgs"/> instance containing the event data.</param>
        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.RaiseExceptionEvent(e.Exception);
        }
    }
}
