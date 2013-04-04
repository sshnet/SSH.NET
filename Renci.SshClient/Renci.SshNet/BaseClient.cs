using System;
using System.IO;
using System.Threading;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Serves as base class for client implementations, provides common client functionality.
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        private TimeSpan _keepAliveInterval;

        private Timer _keepAliveTimer;

        /// <summary>
        /// Gets current session.
        /// </summary>
        protected Session Session { get; private set; }

        /// <summary>
        /// Gets the connection info.
        /// </summary>
        public ConnectionInfo ConnectionInfo { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this client is connected to the server.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this client is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                if (this.Session == null)
                    return false;
                else
                    return this.Session.IsConnected;
            }
        }

        /// <summary>
        /// Gets or sets the keep alive interval in seconds.
        /// </summary>
        /// <value>
        /// The keep alive interval in seconds.
        /// </value>
        public TimeSpan KeepAliveInterval
        {
            get
            {
                return this._keepAliveInterval;
            }
            set
            {
                this._keepAliveInterval = value;

                if (this._keepAliveTimer == null)
                {
                    this._keepAliveTimer = new Timer((state) =>
                    {
                        this.SendKeepAlive();
                    });
                }

                this._keepAliveTimer.Change(this._keepAliveInterval, this._keepAliveInterval);
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        public BaseClient(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");

            this.ConnectionInfo = connectionInfo;
            this.Session = new Session(connectionInfo);
        }

        /// <summary>
        /// Connects client to the server.
        /// </summary>
        public void Connect()
        {
            this.OnConnecting();

            if (this.IsConnected)
            {
                this.Session.Disconnect();
            }

            this.Session = new Session(this.ConnectionInfo);
            this.Session.HostKeyReceived += Session_HostKeyReceived;
            this.Session.ErrorOccured += Session_ErrorOccured;
            this.Session.Connect();

            this.OnConnected();
        }

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        public void Disconnect()
        {
            if (!this.IsConnected)
                return;

            this.OnDisconnecting();

            this.Session.Disconnect();

            this.OnDisconnected();
        }

        /// <summary>
        /// Sends keep-alive message to the server.
        /// </summary>
        public void SendKeepAlive()
        {
            if (this.Session == null)
                return;

            if (!this.Session.IsConnected)
                return;

            this.Session.SendKeepAlive();
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

        private bool _isDisposed = false;

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
                    // Dispose managed ResourceMessages.
                    this.Session.ErrorOccured -= Session_ErrorOccured;
                    this.Session.HostKeyReceived -= Session_HostKeyReceived;

                    if (this.Session != null)
                    {
                        this.Session.Dispose();
                        this.Session = null;
                    }

                    if (this._keepAliveTimer != null)
                    {
                        this._keepAliveTimer.Dispose();
                        this._keepAliveTimer = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
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
    }
}
