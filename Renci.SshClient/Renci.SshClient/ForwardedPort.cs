using System;
using Renci.SshClient.Common;

namespace Renci.SshClient
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
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; internal set; }

        /// <summary>
        /// Gets the connected host.
        /// </summary>
        public string ConnectedHost { get; internal set; }

        /// <summary>
        /// Gets the connected port.
        /// </summary>
        public uint ConnectedPort { get; internal set; }

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
        /// Initializes a new instance of the <see cref="ForwardedPort"/> class.
        /// </summary>
        internal ForwardedPort()
        {

        }

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
        }

        /// <summary>
        /// Stops port forwarding.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Raises the exception event.
        /// </summary>
        /// <param name="execption">The execption.</param>
        protected void RaiseExceptionEvent(Exception execption)
        {
            if (this.Exception != null)
            {
                this.Exception(this, new ExceptionEventArgs(execption));
            }
        }

    }
}
