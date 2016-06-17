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
    internal abstract class SubsystemSession : ISubsystemSession
    {
        private ISession _session;
        private readonly string _subsystemName;
        private IChannelSession _channel;
        private Exception _exception;
        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _sessionDisconnectedWaitHandle = new ManualResetEvent(false);
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
        /// Occurs when the server has disconnected from the session.
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
            get
            {
                EnsureNotDisposed();

                return _channel;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this session is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this session is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen
        {
            get { return _channel != null && _channel.IsOpen; }
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
        /// Connects the subsystem using a new SSH channel session.
        /// </summary>
        /// <exception cref="InvalidOperationException">The session is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the session was disposed.</exception>
        public void Connect()
        {
            EnsureNotDisposed();

            if (IsOpen)
                throw new InvalidOperationException("The session is already connected.");

            // reset waithandles in case we're reconnecting
            _errorOccuredWaitHandle.Reset();
            _sessionDisconnectedWaitHandle.Reset();
            _sessionDisconnectedWaitHandle.Reset();
            _channelClosedWaitHandle.Reset();

            _session.ErrorOccured += Session_ErrorOccured;
            _session.Disconnected += Session_Disconnected;

            _channel = _session.CreateChannelSession();
            _channel.DataReceived += Channel_DataReceived;
            _channel.Exception += Channel_Exception;
            _channel.Closed += Channel_Closed;
            _channel.Open();
            _channel.SendSubsystemRequest(_subsystemName);

            OnChannelOpen();
        }

        /// <summary>
        /// Disconnects the subsystem channel.
        /// </summary>
        public void Disconnect()
        {
            UnsubscribeFromSessionEvents(_session);

            var channel = _channel;
            if (channel != null)
            {
                channel.DataReceived -= Channel_DataReceived;
                channel.Exception -= Channel_Exception;
                channel.Closed -= Channel_Closed;
                channel.Close();
                channel.Dispose();
                _channel = null;
            }
        }

        /// <summary>
        /// Sends data to the subsystem.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void SendData(byte[] data)
        {
            EnsureNotDisposed();
            EnsureSessionIsOpen();

            _channel.SendData(data);
        }

        /// <summary>
        /// Called when channel is open.
        /// </summary>
        protected abstract void OnChannelOpen();

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected abstract void OnDataReceived(byte[] data);

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
            try
            {
                OnDataReceived(e.Data);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
            RaiseError(e.Exception);
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            var channelClosedWaitHandle = _channelClosedWaitHandle;
            if (channelClosedWaitHandle != null)
                channelClosedWaitHandle.Set();
        }

        /// <summary>
        /// Waits a specified time for a given <see cref="WaitHandle"/> to get signaled.
        /// </summary>
        /// <param name="waitHandle">The handle to wait for.</param>
        /// <param name="operationTimeout">The time to wait for <paramref name="waitHandle"/> to get signaled.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified <paramref name="operationTimeout"/>.</exception>
        public void WaitOnHandle(WaitHandle waitHandle, TimeSpan operationTimeout)
        {
            var waitHandles = new[]
                {
                    _errorOccuredWaitHandle,
                    _sessionDisconnectedWaitHandle,
                    _channelClosedWaitHandle,
                    waitHandle
                };

            switch (WaitHandle.WaitAny(waitHandles, operationTimeout))
            {
                case 0:
                    throw _exception;
                case 1:
                    throw new SshException("Connection was closed by the server.");
                case 2:
                    throw new SshException("Channel was closed.");
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.CurrentCulture, "Operation has timed out."));
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            var sessionDisconnectedWaitHandle = _sessionDisconnectedWaitHandle;
            if (sessionDisconnectedWaitHandle != null)
                sessionDisconnectedWaitHandle.Set();

            SignalDisconnected();
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

        private void EnsureSessionIsOpen()
        {
            if (!IsOpen)
                throw new InvalidOperationException("The session is not open.");
        }

        /// <summary>
        /// Unsubscribes the current <see cref="SubsystemSession"/> from session events.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <remarks>
        /// Does nothing when <paramref name="session"/> is <c>null</c>.
        /// </remarks>
        private void UnsubscribeFromSessionEvents(ISession session)
        {
            if (session == null)
                return;

            session.Disconnected -= Session_Disconnected;
            session.ErrorOccured -= Session_ErrorOccured;
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
            if (_isDisposed)
                return;

            if (disposing)
            {
                Disconnect();

                _session = null;

                var errorOccuredWaitHandle = _errorOccuredWaitHandle;
                if (errorOccuredWaitHandle != null)
                {
                    errorOccuredWaitHandle.Dispose();
                    _errorOccuredWaitHandle = null;
                }

                var sessionDisconnectedWaitHandle = _sessionDisconnectedWaitHandle;
                if (sessionDisconnectedWaitHandle != null)
                {
                    sessionDisconnectedWaitHandle.Dispose();
                    _sessionDisconnectedWaitHandle = null;
                }

                var channelClosedWaitHandle = _channelClosedWaitHandle;
                if (channelClosedWaitHandle != null)
                {
                    channelClosedWaitHandle.Dispose();
                    _channelClosedWaitHandle = null;
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SubsystemSession" /> class.
        /// </summary>
        ~SubsystemSession()
        {
            Dispose(false);
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #endregion
    }
}
