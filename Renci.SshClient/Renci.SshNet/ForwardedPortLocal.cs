using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal : ForwardedPort, IDisposable
    {
        private EventWaitHandle _listenerTaskCompleted;

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public string BoundHost { get; private set; }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; private set; }

        /// <summary>
        /// Gets the forwarded host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the forwarded port.
        /// </summary>
        public uint Port { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <c>true</c> if port forwarding is started; otherwise, <c>false</c>.
        /// </value>
        public override bool IsStarted
        {
            get { return _listenerTaskCompleted != null && !_listenerTaskCompleted.WaitOne(0); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\ForwardedPortLocalTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortLocal" language="C#" title="Local port forwarding" />
        /// </example>
        public ForwardedPortLocal(uint boundPort, string host, uint port)
            : this(string.Empty, boundPort, host, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundHost">The bound host.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="boundHost"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortLocal(string boundHost, string host, uint port)
            : this(boundHost, 0, host, port) 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundHost">The bound host.</param>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="boundHost"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        public ForwardedPortLocal(string boundHost, uint boundPort, string host, uint port)
        {
            if (boundHost == null)
                throw new ArgumentNullException("boundHost");

            if (host == null)
                throw new ArgumentNullException("host");

            boundPort.ValidatePort("boundPort");
            port.ValidatePort("port");

            this.BoundHost = boundHost;
            this.BoundPort = boundPort;
            this.Host = host;
            this.Port = port;
        }

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            this.InternalStart();
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
        /// Interrupts the listener, and waits for the listener loop to finish.
        /// </summary>
        /// <remarks>
        /// When the forwarded port is stopped, then any further action is skipped.
        /// </remarks>
        partial void StopListener();

        partial void InternalStop(TimeSpan timeout);

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
                    if (_listenerTaskCompleted != null)
                    {
                        _listenerTaskCompleted.Dispose();
                        _listenerTaskCompleted = null;
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ForwardedPortLocal"/> is reclaimed by garbage collection.
        /// </summary>
        ~ForwardedPortLocal()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
