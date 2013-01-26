using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Base class for SSH subsystem implementations
    /// </summary>
    public abstract class SubsystemSession : IDisposable
    {
        private Session _session;

        private string _subsystemName;

        private ChannelSession _channel;

        private Exception _exception;

        private EventWaitHandle _errorOccuredWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelClosedWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Specifies a timeout to wait for operation to complete
        /// </summary>
        protected TimeSpan _operationTimeout;

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Occurs when session has been disconnected form the server.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Gets the channel number.
        /// </summary>
        protected uint ChannelNumber { get; private set; }

        protected Encoding Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the SubsystemSession class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="subsystemName">Name of the subsystem.</param>
        /// <param name="operationTimeout">The operation timeout.</param>
        /// <exception cref="System.ArgumentNullException">session</exception>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> or <paramref name="subsystemName" /> is null.</exception>
        public SubsystemSession(Session session, string subsystemName, TimeSpan operationTimeout, Encoding encoding)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            if (subsystemName == null)
                throw new ArgumentNullException("subsystemName");

            this._session = session;
            this._subsystemName = subsystemName;
            this._operationTimeout = operationTimeout;
            this.Encoding = encoding;
        }

        /// <summary>
        /// Connects subsystem on SSH channel.
        /// </summary>
        public void Connect()
        {
            this._channel = this._session.CreateChannel<ChannelSession>();

            this._session.ErrorOccured += Session_ErrorOccured;
            this._session.Disconnected += Session_Disconnected;
            this._channel.DataReceived += Channel_DataReceived;
            this._channel.Closed += Channel_Closed;

            this._channel.Open();

            this.ChannelNumber = this._channel.RemoteChannelNumber;

            this._channel.SendSubsystemRequest(_subsystemName);

            this.OnChannelOpen();
        }

        /// <summary>
        /// Disconnects subsystem channel.
        /// </summary>
        public void Disconnect()
        {
            this._channel.SendEof();

            this._channel.Close();
        }

        /// <summary>
        /// Sends data to the subsystem.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void SendData(byte[] data)
        {
            this._channel.SendData(data);
        }

        /// <summary>
        /// Called when channel is open.
        /// </summary>
        protected abstract void OnChannelOpen();

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="dataTypeCode">The data type code.</param>
        /// <param name="data">The data.</param>
        protected abstract void OnDataReceived(uint dataTypeCode, byte[] data);

        /// <summary>
        /// Raises the error.
        /// </summary>
        /// <param name="error">The error.</param>
        protected void RaiseError(Exception error)
        {
            this._exception = error;

            this._errorOccuredWaitHandle.Set();

            if (this.ErrorOccurred != null)
            {
                this.ErrorOccurred(this, new ExceptionEventArgs(error));
            }
        }

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            this.OnDataReceived(e.DataTypeCode, e.Data);
        }

        private void Channel_Closed(object sender, Common.ChannelEventArgs e)
        {
            this._channelClosedWaitHandle.Set();
        }

        internal void WaitHandle(WaitHandle waitHandle, TimeSpan operationTimeout)
        {
            var waitHandles = new WaitHandle[]
                {
                    this._errorOccuredWaitHandle,
                    this._channelClosedWaitHandle,
                    waitHandle,
                };

            switch (EventWaitHandle.WaitAny(waitHandles, operationTimeout))
            {
                case 0:
                    {
                        var exception = this._exception;
                        this._exception = null;
                        throw exception;
                    }
                case 1:
                    throw new SshException("Channel was closed.");
                case System.Threading.WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.CurrentCulture, "Operation has timed out."));
                default:
                    break;
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            if (this.Disconnected != null)
            {
                this.Disconnected(this, new EventArgs());
            }

            this.RaiseError(new SshException("Connection was lost"));
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.RaiseError(e.Exception);
        }

        #region IDisposable Members

        private bool _isDisposed = false;
        
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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                if (this._channel != null)
                {
                    this._channel.DataReceived -= Channel_DataReceived;

                    this._channel.Dispose();
                    this._channel = null;
                }

                this._session.ErrorOccured -= Session_ErrorOccured;
                this._session.Disconnected -= Session_Disconnected;

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._errorOccuredWaitHandle != null)
                    {
                        this._errorOccuredWaitHandle.Dispose();
                        this._errorOccuredWaitHandle = null;
                    }

                    if (this._channelClosedWaitHandle != null)
                    {
                        this._channelClosedWaitHandle.Dispose();
                        this._channelClosedWaitHandle = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SubsystemSession" /> class.
        /// </summary>
        ~SubsystemSession()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
