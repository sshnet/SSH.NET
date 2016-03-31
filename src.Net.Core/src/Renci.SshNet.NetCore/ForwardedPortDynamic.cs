using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for dynamic port forwarding
    /// </summary>
    public partial class ForwardedPortDynamic : ForwardedPort
    {
        private EventWaitHandle _listenerCompleted;

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public string BoundHost { get; private set; }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
        /// </value>
        public override bool IsStarted
        {
            get { return _listenerCompleted != null && !_listenerCompleted.WaitOne(0); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortDynamic"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public ForwardedPortDynamic(uint port)
            : this(string.Empty, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortDynamic"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public ForwardedPortDynamic(string host, uint port)
        {
            BoundHost = host;
            BoundPort = port;
        }

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            InternalStart();
        }

        /// <summary>
        /// Stops local port forwarding, and waits for the specified timeout until all pending
        /// requests are processed.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for pending requests to finish processing.</param>
        protected override void StopPort(TimeSpan timeout)
        {
            if (IsStarted)
            {
                // prevent new requests from getting processed before we signal existing
                // channels that the port is closing
                StopListener();
                // signal existing channels that the port is closing
                base.StopPort(timeout);
            }
            // wait for open channels to close
            InternalStop(timeout);
        }

        /// <summary>
        /// Ensures the current instance is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance is disposed.</exception>
        protected override void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        partial void InternalStart();

        /// <summary>
        /// Stops the listener.
        /// </summary>
        partial void StopListener();

        /// <summary>
        /// Waits for pending requests to finish, and channels to close.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the forwarded port to stop.</param>
        partial void InternalStop(TimeSpan timeout);

        /// <summary>
        /// Executes the specified action in a separate thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        partial void ExecuteThread(Action action);

        #region IDisposable Members

        /// <summary>
        /// Holds a value indicating whether the current instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current instance is disposed; otherwise, <c>false</c>.
        /// </value>
        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        partial void InternalDispose(bool disposing);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (_listenerCompleted != null)
                    {
                        _listenerCompleted.Dispose();
                        _listenerCompleted = null;
                    }
                }

                InternalDispose(disposing);

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortLocal"/> is reclaimed by garbage collection.
        /// </summary>
        ~ForwardedPortDynamic()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
