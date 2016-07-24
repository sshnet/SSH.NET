using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for forwarding connections from the client to destination servers via the SSH server,
    /// also known as dynamic port forwarding.
    /// </summary>
    public partial class ForwardedPortDynamic : ForwardedPort
    {
        private ForwardedPortStatus _status;

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public string BoundHost { get; private set; }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; private set; }

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
        /// </value>
        public override bool IsStarted
        {
            get { return _status == ForwardedPortStatus.Started; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortDynamic"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public ForwardedPortDynamic(uint port) : this(string.Empty, port)
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
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            if (!ForwardedPortStatus.ToStarting(ref _status))
                return;

            try
            {
                InternalStart();
            }
            catch (Exception)
            {
                _status = ForwardedPortStatus.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Stops local port forwarding, and waits for the specified timeout until all pending
        /// requests are processed.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for pending requests to finish processing.</param>
        protected override void StopPort(TimeSpan timeout)
        {
            if (!ForwardedPortStatus.ToStopping(ref _status))
                return;

            // signal existing channels that the port is closing
            base.StopPort(timeout);
            // prevent new requests from getting processed
            StopListener();
            // wait for open channels to close
            InternalStop(timeout);
            // mark port stopped
            _status = ForwardedPortStatus.Stopped;
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

        #region IDisposable Members

        /// <summary>
        /// Holds a value indicating whether the current instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current instance is disposed; otherwise, <c>false</c>.
        /// </value>
        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            base.Dispose(disposing);
            InternalDispose(disposing);

            _isDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortDynamic"/> is reclaimed by garbage collection.
        /// </summary>
        ~ForwardedPortDynamic()
        {
            Dispose(false);
        }

        #endregion
    }
}
