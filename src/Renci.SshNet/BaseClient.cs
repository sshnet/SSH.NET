using System;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Abstractions;
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

                return IsSessionConnected();
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
                    if (_keepAliveTimer != null)
                    {
                        // change the due time and interval of the timer if has already
                        // been created (which means the client is connected)

                        _keepAliveTimer.Change(value, value);
                    }
                    else if (IsSessionConnected())
                    {
                        // if timer has not yet been created and the client is already connected,
                        // then we need to create the timer now
                        //
                        // this means that - before connecting - the keep-alive interval was set to
                        // negative one (-1) and as such we did not create the timer
                        _keepAliveTimer = CreateKeepAliveTimer(value, value);
                    }

                    // note that if the client is not yet connected, then the timer will be created with the 
                    // new interval when Connect() is invoked
                }
                _keepAliveInterval = value;
            }
        }

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        /// <example>
        ///   <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect ErrorOccurred" language="C#" title="Handle ErrorOccurred event" />
        /// </example>
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Occurs when host key received.
        /// </summary>
        /// <example>
        ///   <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect HostKeyReceived" language="C#" title="Handle HostKeyReceived event" />
        /// </example>
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <c>null</c>.</exception>
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
            if (IsSessionConnected())
                throw new InvalidOperationException("The client is already connected.");

            OnConnecting();

            Session = CreateAndConnectSession();
            try
            {
                // Even though the method we invoke makes you believe otherwise, at this point only
                // the SSH session itself is connected.
                OnConnected();
            }
            catch
            {
                // Only dispose the session as Disconnect() would have side-effects (such as remove forwarded
                // ports in SshClient).
                DisposeSession();
                throw;
            }
            StartKeepAliveTimer();
        }

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void Disconnect()
        {
            DiagnosticAbstraction.Log("Disconnecting client.");

            CheckDisposed();

            OnDisconnecting();

            // stop sending keep-alive messages before we close the session
            StopKeepAliveTimer();

            // dispose the SSH session
            DisposeSession();

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
            var session = Session;
            if (session != null)
            {
                session.OnDisconnecting();
            }
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DiagnosticAbstraction.Log("Disposing client.");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

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

            _keepAliveTimer.Dispose();
            _keepAliveTimer = null;
        }

        private void SendKeepAliveMessage()
        {
            var session = Session;

            // do nothing if we have disposed or disconnected
            if (session == null)
                return;

            // do not send multiple keep-alive messages concurrently
            if (Monitor.TryEnter(_keepAliveLock))
            {
                try
                {
                    session.TrySendMessage(new IgnoreMessage());
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

            if (_keepAliveTimer != null)
                // timer is already started
                return;

            _keepAliveTimer = CreateKeepAliveTimer(_keepAliveInterval, _keepAliveInterval);
        }

        /// <summary>
        /// Creates a <see cref="Timer"/> with the specified due time and interval.
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before the keep-alive message is first sent. Specify negative one (-1) milliseconds to prevent the timer from starting. Specify zero (0) to start the timer immediately.</param>
        /// <param name="period">The time interval between attempts to send a keep-alive message. Specify negative one (-1) milliseconds to disable periodic signaling.</param>
        /// <returns>
        /// A <see cref="Timer"/> with the specified due time and interval.
        /// </returns>
        private Timer CreateKeepAliveTimer(TimeSpan dueTime, TimeSpan period)
        {
            return new Timer(state => SendKeepAliveMessage(), Session, dueTime, period);
        }

        private ISession CreateAndConnectSession()
        {
            var session = _serviceFactory.CreateSession(ConnectionInfo, _serviceFactory.CreateSocketFactory());
            session.HostKeyReceived += Session_HostKeyReceived;
            session.ErrorOccured += Session_ErrorOccured;

            try
            {
                session.Connect();
                return session;
            }
            catch
            {
                DisposeSession(session);
                throw;
            }
        }

        private void DisposeSession(ISession session)
        {
            session.ErrorOccured -= Session_ErrorOccured;
            session.HostKeyReceived -= Session_HostKeyReceived;
            session.Dispose();
        }

        /// <summary>
        /// Disposes the SSH session, and assigns <c>null</c> to <see cref="Session"/>.
        /// </summary>
        private void DisposeSession()
        {
            var session = Session;
            if (session != null)
            {
                Session = null;
                DisposeSession(session);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the SSH session is established.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the SSH session is established; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSessionConnected()
        {
            var session = Session;
            return session != null && session.IsConnected;
        }
    }
}
