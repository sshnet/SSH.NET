using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for SSH subsystem implementations
    /// </summary>
    internal abstract class SubsystemSession : IDisposable
    {
        private readonly ISession _session;
        private readonly string _subsystemName;
        private IChannelSession _channel;
        private Exception _exception;
        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _channelClosedWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Specifies a timeout to wait for operation to complete
        /// </summary>
        protected TimeSpan OperationTimeout { get; private set; }

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Occurs when session has been disconnected form the server.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Gets the channel associated with this session.
        /// </summary>
        /// <value>
        /// The channel associated with this session.
        /// </value>
        internal IChannelSession Channel
        {
            get { return _channel; }
        }

        /// <summary>
        /// Gets the character encoding to use.
        /// </summary>
        protected Encoding Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the SubsystemSession class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="subsystemName">Name of the subsystem.</param>
        /// <param name="operationTimeout">The operation timeout.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> or <paramref name="subsystemName" /> or <paramref name="encoding"/>is null.</exception>
        protected SubsystemSession(ISession session, string subsystemName, TimeSpan operationTimeout, Encoding encoding)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (subsystemName == null)
                throw new ArgumentNullException("subsystemName");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _session = session;
            _subsystemName = subsystemName;
            OperationTimeout = operationTimeout;
            Encoding = encoding;
        }

        /// <summary>
        /// Connects subsystem on SSH channel.
        /// </summary>
        public void Connect()
        {
            _channel = _session.CreateChannelSession();

            _session.ErrorOccured += Session_ErrorOccured;
            _session.Disconnected += Session_Disconnected;
            _channel.DataReceived += Channel_DataReceived;
            _channel.Closed += Channel_Closed;
            _channel.Open();
            _channel.SendSubsystemRequest(_subsystemName);
            OnChannelOpen();
        }

        /// <summary>
        /// Disconnects subsystem channel.
        /// </summary>
        public void Disconnect()
        {
            _channel.SendEof();
            _channel.Close();
        }

        /// <summary>
        /// Sends data to the subsystem.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void SendData(byte[] data)
        {
            _channel.SendData(data);
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
            _exception = error;

            var errorOccuredWaitHandle = _errorOccuredWaitHandle;
            if (errorOccuredWaitHandle != null)
                errorOccuredWaitHandle.Set();
            SignalErrorOccurred(error);
        }

        private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
        {
            OnDataReceived(e.DataTypeCode, e.Data);
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            var channelClosedWaitHandle = _channelClosedWaitHandle;
            if (channelClosedWaitHandle != null)
                channelClosedWaitHandle.Set();
        }

        internal void WaitOnHandle(WaitHandle waitHandle, TimeSpan operationTimeout)
        {
            var waitHandles = new[]
                {
                    _errorOccuredWaitHandle,
                    _channelClosedWaitHandle,
                    waitHandle
                };

            switch (WaitHandle.WaitAny(waitHandles, operationTimeout))
            {
                case 0:
                    throw _exception;
                case 1:
                    throw new SshException("Channel was closed.");
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.CurrentCulture, "Operation has timed out."));
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            SignalDisconnected();
            RaiseError(new SshException("Connection was lost"));
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            RaiseError(e.Exception);
        }

        private void SignalErrorOccurred(Exception error)
        {
            var errorOccurred = ErrorOccurred;
            if (errorOccurred != null)
            {
                errorOccurred(this, new ExceptionEventArgs(error));
            }
        }

        private void SignalDisconnected()
        {
            var disconnected = Disconnected;
            if (disconnected != null)
            {
                disconnected(this, new EventArgs());
            }
        }

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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                if (_channel != null)
                {
                    _channel.DataReceived -= Channel_DataReceived;
                    _channel.Closed -= Channel_Closed;
                    _channel.Dispose();
                    _channel = null;
                }

                _session.ErrorOccured -= Session_ErrorOccured;
                _session.Disconnected -= Session_Disconnected;

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_errorOccuredWaitHandle != null)
                    {
                        _errorOccuredWaitHandle.Dispose();
                        _errorOccuredWaitHandle = null;
                    }

                    if (_channelClosedWaitHandle != null)
                    {
                        _channelClosedWaitHandle.Dispose();
                        _channelClosedWaitHandle = null;
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
