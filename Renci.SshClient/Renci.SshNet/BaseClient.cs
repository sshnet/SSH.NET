using System;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    /// <summary>
    /// Serves as base class for client implementations, provides common client functionality.
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        /// <summary>
        /// Holds value indicating whether the connection info is owned by this client.
        /// </summary>
        private readonly bool _ownsConnectionInfo;

        private readonly IServiceFactory _serviceFactory;
        private readonly object _keepAliveLock = new object();
        private TimeSpan _keepAliveInterval;
        private Timer _keepAliveTimer;
        private ConnectionInfo _connectionInfo;

        /// <summary>
        /// Gets the current session.
        /// </summary>
        /// <value>
        /// The current session.
        /// </value>
        internal ISession Session { get; private set; }

        /// <summary>
        /// Gets the factory for creating new services.
        /// </summary>
        /// <value>
        /// The factory for creating new services.
        /// </value>
        internal IServiceFactory ServiceFactory
        {
            get { return _serviceFactory; }
        }

        /// <summary>
        /// Gets the connection info.
        /// </summary>
        /// <value>
        /// The connection info.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public ConnectionInfo ConnectionInfo
        {
            get
            {
                CheckDisposed();
                return _connectionInfo;
            }
            private set
            {
                _connectionInfo = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this client is connected to the server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this client is connected; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public bool IsConnected
        {
            get
            {
                CheckDisposed();
                return Session != null && Session.IsConnected;
            }
        }

        /// <summary>
        /// Gets or sets the keep-alive interval.
        /// </summary>
        /// <value>
        /// The keep-alive interval. Specify negative one (-1) milliseconds to disable the
        /// keep-alive. This is the default value.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public TimeSpan KeepAliveInterval
        {
            get
            {
                CheckDisposed();
                return _keepAliveInterval;
            }
            set
            {
                CheckDisposed();

                if (value == _keepAliveInterval)
                    return;

                if (value == SshNet.Session.InfiniteTimeSpan)
                {
                    // stop the timer when the value is -1 milliseconds
                    StopKeepAliveTimer();
                }
                else
                {
                    // change the due time and interval of the timer if has already
                    // been created (which means the client is connected)
                    // 
                    // if the client is not yet connected, then the timer will be
                    // created with the new interval when Connect() is invoked
                    if (_keepAliveTimer != null)
                        _keepAliveTimer.Change(value, value);
                }
                _keepAliveInterval = value;
            }
        }

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        /// <example>
        ///   <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect ErrorOccurred" language="C#" title="Handle ErrorOccurred event" />
        /// </example>
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Occurs when host key received.
        /// </summary>
        /// <example>
        ///   <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect HostKeyReceived" language="C#" title="Handle HostKeyReceived event" />
        /// </example>
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        protected BaseClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
            : this(connectionInfo, ownsConnectionInfo, new ServiceFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="serviceFactory">The factory to use for creating new services.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is null.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal BaseClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (serviceFactory == null)
                throw new ArgumentNullException("serviceFactory");

            ConnectionInfo = connectionInfo;
            _ownsConnectionInfo = ownsConnectionInfo;
            _serviceFactory = serviceFactory;
            _keepAliveInterval = SshNet.Session.InfiniteTimeSpan;
        }

        /// <summary>
        /// Connects client to the server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <exception cref="SocketException">Socket connection to the SSH server or proxy server could not be established, or an error occurred while resolving the hostname.</exception>
        /// <exception cref="SshConnectionException">SSH session could not be established.</exception>
        /// <exception cref="SshAuthenticationException">Authentication of SSH session failed.</exception>
        /// <exception cref="ProxyException">Failed to establish proxy connection.</exception>
        public void Connect()
        {
            CheckDisposed();

            // TODO (see issue #1758):
            // we're not stopping the keep-alive timer and disposing the session here
            // 
            // we could do this but there would still be side effects as concrete
            // implementations may still hang on to the original session
            // 
            // therefore it would be better to actually invoke the Disconnect method
            // (and then the Dispose on the session) but even that would have side effects
            // eg. it would remove all forwarded ports from SshClient
            // 
            // I think we should modify our concrete clients to better deal with a
            // disconnect. In case of SshClient this would mean not removing the 
            // forwarded ports on disconnect (but only on dispose ?) and link a
            // forwarded port with a client instead of with a session
            //
            // To be discussed with Oleg (or whoever is interested)
            if (Session != null && Session.IsConnected)
                throw new InvalidOperationException("The client is already connected.");

            OnConnecting();
            Session = _serviceFactory.CreateSession(ConnectionInfo);
            Session.HostKeyReceived += Session_HostKeyReceived;
            Session.ErrorOccured += Session_ErrorOccured;
            Session.Connect();
            StartKeepAliveTimer();
            OnConnected();
        }

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void Disconnect()
        {
            CheckDisposed();

            OnDisconnecting();

            // stop sending keep-alive messages before we close the
            // session
            StopKeepAliveTimer();

            // disconnect and dispose the SSH session
            if (Session != null)
            {
                // a new session is created in Connect(), so we should dispose and
                // dereference the current session here
                Session.ErrorOccured -= Session_ErrorOccured;
                Session.HostKeyReceived -= Session_HostKeyReceived;
                Session.Disconnect();
                Session.Dispose();
                Session = null;
            }

            OnDisconnected();
        }

        /// <summary>
        /// Sends a keep-alive message to the server.
        /// </summary>
        /// <remarks>
        /// Use <see cref="KeepAliveInterval"/> to configure the client to send a keep-alive at regular
        /// intervals.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        [Obsolete("Use KeepAliveInterval to send a keep-alive message at regular intervals.")]
        public void SendKeepAlive()
        {
            CheckDisposed();

            SendKeepAliveMessage();
        }

        /// <summary>
        /// Called when client is connecting to the server.
        /// </summary>
        protected virtual void OnConnecting()
        {
        }

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected virtual void OnDisconnecting()
        {
            if (Session != null)
                Session.OnDisconnecting();
        }

        /// <summary>
        /// Called when client is disconnected from the server.
        /// </summary>
        protected virtual void OnDisconnected()
        {
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            var handler = ErrorOccurred;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Session_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            var handler = HostKeyReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
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
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Disconnect();

                    if (_ownsConnectionInfo && _connectionInfo != null)
                    {
                        var connectionInfoDisposable = _connectionInfo as IDisposable;
                        if (connectionInfoDisposable != null)
                            connectionInfoDisposable.Dispose();
                        _connectionInfo = null;
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Check if the current instance is disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">THe current instance is disposed.</exception>
        protected void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="BaseClient"/> is reclaimed by garbage collection.
        /// </summary>
        ~BaseClient()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// Stops the keep-alive timer, and waits until all timer callbacks have been
        /// executed.
        /// </summary>
        private void StopKeepAliveTimer()
        {
            if (_keepAliveTimer == null)
                return;

            var timerDisposed = new ManualResetEvent(false);
            _keepAliveTimer.Dispose(timerDisposed);
            timerDisposed.WaitOne();
            timerDisposed.Dispose();
            _keepAliveTimer = null;
        }

        private void SendKeepAliveMessage()
        {
            // do nothing if we have disposed or disconnected
            if (Session == null)
                return;

            // do not send multiple keep-alive messages concurrently
            if (Monitor.TryEnter(_keepAliveLock))
            {
                try
                {
                    Session.TrySendMessage(new IgnoreMessage());
                }
                finally
                {
                    Monitor.Exit(_keepAliveLock);
                }
            }
        }

        /// <summary>
        /// Starts the keep-alive timer.
        /// </summary>
        /// <remarks>
        /// When <see cref="KeepAliveInterval"/> is negative one (-1) milliseconds, then
        /// the timer will not be started.
        /// </remarks>
        private void StartKeepAliveTimer()
        {
            if (_keepAliveInterval == SshNet.Session.InfiniteTimeSpan)
                return;

            if (_keepAliveTimer == null)
                _keepAliveTimer = new Timer(state => SendKeepAliveMessage());
            _keepAliveTimer.Change(_keepAliveInterval, _keepAliveInterval);
        }
    }
}
