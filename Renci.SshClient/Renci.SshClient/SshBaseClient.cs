using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renci.SshClient.Sftp;

namespace Renci.SshClient
{
    public abstract class SshBaseClient : IDisposable
    {
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
        /// Initializes a new instance of the <see cref="SshBaseClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        public SshBaseClient(ConnectionInfo connectionInfo)
        {
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
            this.Session.Connect();

            this.OnConnected();
        }

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        public void Disconnect()
        {
            this.OnDisconnecting();

            this.Session.Disconnect();

            this.OnDisconnected();
        }

        /// <summary>
        /// Sends keep-alive message to the server.
        /// </summary>
        public void KeepAlive()
        {
            this.Session.KeepAlive();
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

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.Session != null)
                    {
                        this.Session.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~SshBaseClient()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
