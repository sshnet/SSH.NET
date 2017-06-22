using System;
using System.Globalization;
using System.Net;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal : ForwardedPort, IDisposable
    {
        private const uint TypeSocketPath = uint.MaxValue;
        private uint _typeOrPort;
        private string _hostOrSocketPath;

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
        /// Gets the forwarded host.
        /// </summary>
        public string Host
        {
            get
            {
                return _typeOrPort == TypeSocketPath ? string.Empty : _hostOrSocketPath;
            }
            private set
            {
                _hostOrSocketPath = value;
                _typeOrPort = _typeOrPort == TypeSocketPath ? 0 : _typeOrPort;
            }
        }

        /// <summary>
        /// Gets the forwarded port.
        /// </summary>
        public uint Port
        {
            get
            {
                return _typeOrPort == TypeSocketPath ? 0 : _typeOrPort;
            }
            private set
            {
                _typeOrPort = value;
            }
        }

        /// <summary>
        /// Gets the remote address.
        /// </summary>
        public string RemoteAddress
        {
            get
            {
                if (_typeOrPort == TypeSocketPath)
                {
                    return _hostOrSocketPath;
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", _hostOrSocketPath, _typeOrPort);
                }
            }
        }

        /// <summary>
        /// Gets the bound address.
        /// </summary>
        public string LocalAddress
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", BoundHost, BoundPort);
            }
        }

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
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="boundPort" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\ForwardedPortLocalTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortLocal" language="C#" title="Local port forwarding" />
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

            BoundHost = boundHost;
            BoundPort = boundPort;
            Host = host;
            Port = port;
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortLocal"/> class.
        /// </summary>
        /// <param name="localAddress">The local address.</param>
        /// <param name="remoteAddress">The remote address.</param>
        /// <exception cref="ArgumentNullException"><paramref name="localAddress"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="remoteAddress"/> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException"><paramref name="localAddress"/> is a unix socket path.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="localAddress" /> has a port number which is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="FormatException"><paramref name="localAddress"/> port number cannot be parsed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="remoteAddress" /> has a port number which is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="FormatException"><paramref name="remoteAddress"/> port number cannot be parsed.</exception>
        public ForwardedPortLocal(string localAddress, string remoteAddress)
        {
            if (localAddress == null)
                throw new ArgumentNullException("localAddress");

            if (remoteAddress == null)
                throw new ArgumentNullException("remoteAddress");

            if (localAddress.IsUnixSocketAddress())
            {
                throw new NotSupportedException("localAddress cannot be a unix socket path");
            }

            string boundHost;            
            uint boundPort;
            ParseAddress(localAddress, out boundHost, out boundPort, "localAddress");
            BoundHost = boundHost;            
            BoundPort = boundPort;

            if (remoteAddress.IsUnixSocketAddress())
            {
                _hostOrSocketPath = remoteAddress;
                _typeOrPort = TypeSocketPath;
            }
            else
            {
                ParseAddress(remoteAddress, out _hostOrSocketPath, out _typeOrPort, "remoteAddress");
            }
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
        /// <see cref="ForwardedPortLocal"/> is reclaimed by garbage collection.
        /// </summary>
        ~ForwardedPortLocal()
        {
            Dispose(false);
        }

        #endregion

        private void ParseAddress(string address, out string host, out uint port, string argument)
        {
            if (address.Contains(":"))
            {
                var split = address.Split(new [] { ':' });
                host = split[0];
                if (split.Length > 2 || !uint.TryParse(split[1], out port))
                {
                    throw new FormatException("Cannot parse port number.");
                }
                if (port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException(argument,
                        string.Format(CultureInfo.InvariantCulture, "Specified port cannot be greater than {0}.",
                            IPEndPoint.MaxPort));
            }
            else
            {
                host = address;
                port = 0;
            }
        }
    }
}
