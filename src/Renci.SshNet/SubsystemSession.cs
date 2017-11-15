using System;
using System.Globalization;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for SSH subsystem implementations
    /// </summary>
    internal abstract class SubsystemSession : ISubsystemSession
    {
        /// <summary>
        /// Holds the number of system wait handles that are returned as the leading entries in the array returned
        /// in <see cref="CreateWaitHandleArray(WaitHandle[])"/>.
        /// </summary>
        private const int SystemWaitHandleCount = 3;

        private ISession _session;
        private readonly string _subsystemName;
        private IChannelSession _channel;
        private Exception _exception;
        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _sessionDisconnectedWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _channelClosedWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Gets or set the number of seconds to wait for an operation to complete.
        /// </summary>
        /// <value>
        /// The number of seconds to wait for an operation to complete, or -1 to wait indefinitely.
        /// </value>
        public int OperationTimeout { get; private set; }

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
        /// Initializes a new instance of the SubsystemSession class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="subsystemName">Name of the subsystem.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for a given operation to complete, or -1 to wait indefinitely.</param>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> or <paramref name="subsystemName" /> is <c>null</c>.</exception>
        protected SubsystemSession(ISession session, string subsystemName, int operationTimeout)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (subsystemName == null)
                throw new ArgumentNullException("subsystemName");

            _session = session;
            _subsystemName = subsystemName;
            OperationTimeout = operationTimeout;
        }

        /// <summary>
        /// Connects the subsystem using a new SSH channel session.
        /// </summary>
        /// <exception cref="InvalidOperationException">The session is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the session was disposed.</exception>
        /// <exception cref="SshException">The channel session could not be opened, or the subsystem could not be executed.</exception>
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

            if (!_channel.SendSubsystemRequest(_subsystemName))
            {
                // close channel session
                Disconnect();
                // signal subsystem failure
                throw new SshException(string.Format(CultureInfo.InvariantCulture,
                                                     "Subsystem '{0}' could not be executed.",
                                                     _subsystemName));
            }

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
                _channel = null;
                channel.DataReceived -= Channel_DataReceived;
                channel.Exception -= Channel_Exception;
                channel.Closed -= Channel_Closed;
                channel.Dispose();
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

            DiagnosticAbstraction.Log("Raised exception: " + error);

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
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for <paramref name="waitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified timeout.</exception>
        public void WaitOnHandle(WaitHandle waitHandle, int millisecondsTimeout)
        {
            var waitHandles = new[]
                {
                    _errorOccuredWaitHandle,
                    _sessionDisconnectedWaitHandle,
                    _channelClosedWaitHandle,
                    waitHandle
                };

            var result = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
            switch (result)
            {
                case 0:
                    throw _exception;
                case 1:
                    throw new SshException("Connection was closed by the server.");
                case 2:
                    throw new SshException("Channel was closed.");
                case 3:
                    break;
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException("Operation has timed out.");
                default:
                    throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", result));
            }
        }

        /// <summary>
        /// Blocks the current thread until the specified <see cref="WaitHandle"/> gets signaled, using a
        /// 32-bit signed integer to specify the time interval in milliseconds.
        /// </summary>
        /// <param name="waitHandle">The handle to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for <paramref name="waitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="waitHandle"/> received a signal within the specified timeout;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <remarks>
        /// The blocking wait is also interrupted when either the established channel is closed, the current
        /// session is disconnected or an unexpected <see cref="Exception"/> occurred while processing a channel
        /// or session event.
        /// </remarks>
        public bool WaitOne(WaitHandle waitHandle, int millisecondsTimeout)
        {
            var waitHandles = new[]
                {
                    _errorOccuredWaitHandle,
                    _sessionDisconnectedWaitHandle,
                    _channelClosedWaitHandle,
                    waitHandle
                };

            var result = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
            switch (result)
            {
                case 0:
                    throw _exception;
                case 1:
                    throw new SshException("Connection was closed by the server.");
                case 2:
                    throw new SshException("Channel was closed.");
                case 3:
                    return true;
                case WaitHandle.WaitTimeout:
                    return false;
                default:
                    throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", result));
            }
        }

        /// <summary>
        /// Blocks the current thread until the specified <see cref="WaitHandle"/> gets signaled, using a
        /// 32-bit signed integer to specify the time interval in milliseconds.
        /// </summary>
        /// <param name="waitHandle1">The first handle to wait for.</param>
        /// <param name="waitHandle2">The second handle to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for a <see cref="WaitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <returns>
        /// <c>0</c> if <paramref name="waitHandle1"/> received a signal within the specified timeout, and <c>1</c>
        /// if <paramref name="waitHandle2"/> received a signal within the specified timeout.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The handle did not get signaled within the specified timeout.</exception>
        /// <remarks>
        /// <para>
        /// The blocking wait is also interrupted when either the established channel is closed, the current
        /// session is disconnected or an unexpected <see cref="Exception"/> occurred while processing a channel
        /// or session event.
        /// </para>
        /// <para>
        /// When both <paramref name="waitHandle1"/> and <paramref name="waitHandle2"/> are signaled during the call,
        /// then <c>0</c> is returned.
        /// </para>
        /// </remarks>
        public int WaitAny(WaitHandle waitHandle1, WaitHandle waitHandle2, int millisecondsTimeout)
        {
            var waitHandles = new[]
                {
                    _errorOccuredWaitHandle,
                    _sessionDisconnectedWaitHandle,
                    _channelClosedWaitHandle,
                    waitHandle1,
                    waitHandle2
                };

            var result = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
            switch (result)
            {
                case 0:
                    throw _exception;
                case 1:
                    throw new SshException("Connection was closed by the server.");
                case 2:
                    throw new SshException("Channel was closed.");
                case 3:
                    return 0;
                case 4:
                    return 1;
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException("Operation has timed out.");
                default:
                    throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", result));
            }
        }

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a signal, using a 32-bit signed
        /// integer to specify the time interval.
        /// </summary>
        /// <param name="waitHandles">A <see cref="WaitHandle"/> array - constructed using <see cref="CreateWaitHandleArray(WaitHandle[])"/> - containing the objects to wait for.</param>
        /// <param name="millisecondsTimeout">To number of milliseconds to wait for a <see cref="WaitHandle"/> to get signaled, or -1 to wait indefinitely.</param>
        /// <returns>
        /// The array index of the first non-system object that satisfied the wait.
        /// </returns>
        /// <exception cref="SshException">The connection was closed by the server.</exception>
        /// <exception cref="SshException">The channel was closed.</exception>
        /// <exception cref="SshOperationTimeoutException">No object satified the wait and a time interval equivalent to <paramref name="millisecondsTimeout"/> has passed.</exception>
        /// <remarks>
        /// For the return value, the index of the first non-system object is considered to be zero.
        /// </remarks>
        public int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            var result = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
            switch (result)
            {
                case 0:
                    throw _exception;
                case 1:
                    throw new SshException("Connection was closed by the server.");
                case 2:
                    throw new SshException("Channel was closed.");
                case WaitHandle.WaitTimeout:
                    throw new SshOperationTimeoutException("Operation has timed out.");
                default:
                    return result - SystemWaitHandleCount;
            }
        }

        /// <summary>
        /// Creates a <see cref="WaitHandle"/> array that is composed of system objects and the specified
        /// elements.
        /// </summary>
        /// <param name="waitHandle1">The first <see cref="WaitHandle"/> to wait for.</param>
        /// <param name="waitHandle2">The second <see cref="WaitHandle"/> to wait for.</param>
        /// <returns>
        /// A <see cref="WaitHandle"/> array that is composed of system objects and the specified elements.
        /// </returns>
        public WaitHandle[] CreateWaitHandleArray(WaitHandle waitHandle1, WaitHandle waitHandle2)
        {
            return new WaitHandle[]
                {
                    _errorOccuredWaitHandle,
                    _sessionDisconnectedWaitHandle,
                    _channelClosedWaitHandle,
                    waitHandle1,
                    waitHandle2
                };
        }

        /// <summary>
        /// Creates a <see cref="WaitHandle"/> array that is composed of system objects and the specified
        /// elements.
        /// </summary>
        /// <param name="waitHandles">A <see cref="WaitHandle"/> array containing the objects to wait for.</param>
        /// <returns>
        /// A <see cref="WaitHandle"/> array that is composed of system objects and the specified elements.
        /// </returns>
        public WaitHandle[] CreateWaitHandleArray(params WaitHandle[] waitHandles)
        {
            var array = new WaitHandle[waitHandles.Length + SystemWaitHandleCount];
            array[0] = _errorOccuredWaitHandle;
            array[1] = _sessionDisconnectedWaitHandle;
            array[2] = _channelClosedWaitHandle;

            for (var i = 0; i < waitHandles.Length; i++)
            {
                array[i + SystemWaitHandleCount] = waitHandles[i];
            }

            return array;
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
                    _errorOccuredWaitHandle = null;
                    errorOccuredWaitHandle.Dispose();
                }

                var sessionDisconnectedWaitHandle = _sessionDisconnectedWaitHandle;
                if (sessionDisconnectedWaitHandle != null)
                {
                    _sessionDisconnectedWaitHandle = null;
                    sessionDisconnectedWaitHandle.Dispose();
                }

                var channelClosedWaitHandle = _channelClosedWaitHandle;
                if (channelClosedWaitHandle != null)
                {
                    _channelClosedWaitHandle = null;
                    channelClosedWaitHandle.Dispose();
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
