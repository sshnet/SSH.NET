using System;
using System.Threading;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Serves as base class for client implementations, provides common client functionality.
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        private static readonly TimeSpan Infinite = new TimeSpan(0, 0, 0, 0, -1);

        /// <summary>
        /// Holds value indicating whether the connection info is owned by this client.
        /// </summary>
        private readonly bool _ownsConnectionInfo;
        private TimeSpan _keepAliveInterval;
        private Timer _keepAliveTimer;
        private ConnectionInfo _connectionInfo;

        /// <summary>
        /// Gets current session.
        /// </summary>
        protected Session Session { get; private set; }

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
                return this.Session != null && this.Session.IsConnected;
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
                return this._keepAliveInterval;
            }
            set
            {
                CheckDisposed();

                if (value == _keepAliveInterval)
                    return;

                if (value == Infinite)
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
                this._keepAliveInterval = value;
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
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");

            ConnectionInfo = connectionInfo;
            _ownsConnectionInfo = ownsConnectionInfo;
            _keepAliveInterval = Infinite;
        }

        /// <summary>
        /// Connects client to the server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void Connect()
        {
            CheckDisposed();

            if (Session != null && Session.IsConnected)
                throw new InvalidOperationException("The client is already connected.");

            OnConnecting();
            Session = new Session(ConnectionInfo);
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
            StopKeepAliveTimer();
            if (Session != null)
                Session.Disconnect();
            OnDisconnected();
        }

        /// <summary>
        /// Sends keep-alive message to the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public void SendKeepAlive()
        {
            CheckDisposed();

            if (Session != null && Session.IsConnected)
                Session.SendKeepAlive();
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
            var handler = this.ErrorOccurred;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Session_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            var handler = this.HostKeyReceived;
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
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    // stop sending keep-alive messages before we close the
                    // session
                    StopKeepAliveTimer();

                    if (this.Session != null)
                    {
                        this.Session.ErrorOccured -= Session_ErrorOccured;
                        this.Session.HostKeyReceived -= Session_HostKeyReceived;
                        this.Session.Dispose();
                        this.Session = null;
                    }

                    if (_ownsConnectionInfo && _connectionInfo != null)
                    {
                        var connectionInfoDisposable = _connectionInfo as IDisposable;
                        if (connectionInfoDisposable != null)
                            connectionInfoDisposable.Dispose();
                        _connectionInfo = null;
                    }
                }

                // Note disposing has been done.
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

        /// <summary>
        /// Starts the keep-alive timer.
        /// </summary>
        /// <remarks>
        /// When <see cref="KeepAliveInterval"/> is negative one (-1) milliseconds, then
        /// the timer will not be started.
        /// </remarks>
        private void StartKeepAliveTimer()
        {
            if (_keepAliveInterval == Infinite)
                return;

            if (_keepAliveTimer == null)
                _keepAliveTimer = new Timer(state => this.SendKeepAlive());
            _keepAliveTimer.Change(_keepAliveInterval, _keepAliveInterval);
        }
    }
}
