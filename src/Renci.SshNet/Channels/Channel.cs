using System;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System.Globalization;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Represents base class for SSH channel implementations.
    /// </summary>
    internal abstract class Channel : IChannel
    {
        private EventWaitHandle _channelClosedWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _channelServerWindowAdjustWaitHandle = new ManualResetEvent(false);
        private readonly object _serverWindowSizeLock = new object();
        private readonly uint _initialWindowSize;
        private uint? _remoteWindowSize;
        private uint? _remoteChannelNumber;
        private uint? _remotePacketSize;
        private ISession _session;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_CLOSE has been sent to the remote party.
        /// </summary>
        /// <value>
        /// <c>true</c> when a SSH_MSG_CHANNEL_CLOSE message has been sent to the other party;
        /// otherwise, <c>false</c>.
        /// </value>
        private bool _closeMessageSent;

        /// <summary>
        /// Holds a value indicating whether a SSH_MSG_CHANNEL_CLOSE has been received from the other
        /// party.
        /// </summary>
        /// <value>
        /// <c>true</c> when a SSH_MSG_CHANNEL_CLOSE message has been received from the other party;
        /// otherwise, <c>false</c>.
        /// </value>
        private bool _closeMessageReceived;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_EOF has been received from the other party.
        /// </summary>
        /// <value>
        /// <c>true</c> when a SSH_MSG_CHANNEL_EOF message has been received from the other party;
        /// otherwise, <c>false</c>.
        /// </value>
        private bool _eofMessageReceived;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_EOF has been sent to the remote party.
        /// </summary>
        /// <value>
        /// <c>true</c> when a SSH_MSG_CHANNEL_EOF message has been sent to the remote party;
        /// otherwise, <c>false</c>.
        /// </value>
        private bool _eofMessageSent;

        /// <summary>
        /// Occurs when an exception is thrown when processing channel messages.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Initializes a new <see cref="Channel"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        protected Channel(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
        {
            _session = session;
            _initialWindowSize = localWindowSize;
            LocalChannelNumber = localChannelNumber;
            LocalPacketSize = localPacketSize;
            LocalWindowSize = localWindowSize;

            session.ChannelWindowAdjustReceived += OnChannelWindowAdjust;
            session.ChannelDataReceived += OnChannelData;
            session.ChannelExtendedDataReceived += OnChannelExtendedData;
            session.ChannelEofReceived += OnChannelEof;
            session.ChannelCloseReceived += OnChannelClose;
            session.ChannelRequestReceived += OnChannelRequest;
            session.ChannelSuccessReceived += OnChannelSuccess;
            session.ChannelFailureReceived += OnChannelFailure;
            session.ErrorOccured += Session_ErrorOccured;
            session.Disconnected += Session_Disconnected;
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        ///  Thhe session.
        /// </value>
        protected ISession Session
        {
            get { return _session; }
        }

        /// <summary>
        /// Gets the type of the channel.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public abstract ChannelTypes ChannelType { get; }

        /// <summary>
        /// Gets the local channel number.
        /// </summary>
        /// <value>
        /// The local channel number.
        /// </value>
        public uint LocalChannelNumber { get; private set; }

        /// <summary>
        /// Gets the maximum size of a data packet that we can receive using the channel.
        /// </summary>
        /// <value>
        /// The maximum size of a packet.
        /// </value>
        /// <remarks>
        /// <para>
        /// This is the maximum size (in bytes) we support for the data (payload) of a
        /// <c>SSH_MSG_CHANNEL_DATA</c> message we receive.
        /// </para>
        /// <para>
        /// We currently do not enforce this limit.
        /// </para>
        /// </remarks>
        public uint LocalPacketSize { get; private set; }

        /// <summary>
        /// Gets the size of the local window.
        /// </summary>
        /// <value>
        /// The size of the local window.
        /// </value>
        public uint LocalWindowSize { get; private set; }

        /// <summary>
        /// Gets the remote channel number.
        /// </summary>
        /// <value>
        /// The remote channel number.
        /// </value>
        public uint RemoteChannelNumber
        {
            get
            {
                if (!_remoteChannelNumber.HasValue)
                    throw CreateRemoteChannelInfoNotAvailableException();
                return _remoteChannelNumber.Value;
            }
            private set
            {
                _remoteChannelNumber = value;
            }
        }

        /// <summary>
        /// Gets the maximum size of a data packet that we can send using the channel.
        /// </summary>
        /// <value>
        /// The maximum size of data that can be sent using a <see cref="ChannelDataMessage"/>
        /// on the current channel.
        /// </value>
        /// <exception cref="InvalidOperationException">The channel has not been opened, or the open has not yet been confirmed.</exception>
        public uint RemotePacketSize
        {
            get
            {
                if (!_remotePacketSize.HasValue)
                    throw CreateRemoteChannelInfoNotAvailableException();
                return _remotePacketSize.Value;
            }
            private set
            {
                _remotePacketSize = value;
            }
        }

        /// <summary>
        /// Gets the window size of the remote server.
        /// </summary>
        /// <value>
        /// The size of the server window.
        /// </value>
        public uint RemoteWindowSize
        {
            get
            {
                if (!_remoteWindowSize.HasValue)
                    throw CreateRemoteChannelInfoNotAvailableException();
                return _remoteWindowSize.Value;
            }
            private set
            {
                _remoteWindowSize = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this channel is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this channel is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen { get; protected set; }

        #region Message events

        /// <summary>
        /// Occurs when <see cref="ChannelDataMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelExtendedDataMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelExtendedDataEventArgs> ExtendedDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelEofMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelEventArgs> EndOfData;

        /// <summary>
        /// Occurs when <see cref="ChannelCloseMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelEventArgs> Closed;

        /// <summary>
        /// Occurs when <see cref="ChannelRequestMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelRequestEventArgs> RequestReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelSuccessMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelEventArgs> RequestSucceeded;

        /// <summary>
        /// Occurs when <see cref="ChannelFailureMessage"/> is received.
        /// </summary>
        public event EventHandler<ChannelEventArgs> RequestFailed;

        #endregion

        /// <summary>
        /// Gets a value indicating whether the session is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if the session is connected; otherwise, <c>false</c>.
        /// </value>
        protected bool IsConnected
        {
            get { return _session.IsConnected; }
        }

        /// <summary>
        /// Gets the connection info.
        /// </summary>
        /// <value>The connection info.</value>
        protected IConnectionInfo ConnectionInfo
        {
            get { return _session.ConnectionInfo; }
        }

        /// <summary>
        /// Gets the session semaphore to control number of session channels.
        /// </summary>
        /// <value>The session semaphore.</value>
        protected SemaphoreLight SessionSemaphore
        {
            get { return _session.SessionSemaphore; }
        }

        protected void InitializeRemoteInfo(uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
        {
            RemoteChannelNumber = remoteChannelNumber;
            RemoteWindowSize = remoteWindowSize;
            RemotePacketSize = remotePacketSize;
        }

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_DATA message with the specified payload.
        /// </summary>
        /// <param name="data">The payload to send.</param>
        public void SendData(byte[] data)
        {
            SendData(data, 0, data.Length);
        }

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_DATA message with the specified payload.
        /// </summary>
        /// <param name="data">An array of <see cref="byte"/> containing the payload to send.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin taking data from.</param>
        /// <param name="size">The number of bytes of <paramref name="data"/> to send.</param>
        /// <remarks>
        /// <para>
        /// When the size of the data to send exceeds the maximum packet size or the remote window
        /// size does not allow the full data to be sent, then this method will send the data in
        /// multiple chunks and will wait for the remote window size to be adjusted when it's zero.
        /// </para>
        /// <para>
        /// This is done to support SSH servers will a small window size that do not agressively
        /// increase their window size. We need to take into account that there may be SSH servers
        /// that only increase their window size when it has reached zero.
        /// </para>
        /// </remarks>
        public void SendData(byte[] data, int offset, int size)
        {
            // send channel messages only while channel is open
            if (!IsOpen)
                return;

            var totalBytesToSend = size;
            while (totalBytesToSend > 0)
            {
                var sizeOfCurrentMessage = GetDataLengthThatCanBeSentInMessage(totalBytesToSend);

                var channelDataMessage = new ChannelDataMessage(RemoteChannelNumber,
                                                                data,
                                                                offset,
                                                                sizeOfCurrentMessage);
                _session.SendMessage(channelDataMessage);

                totalBytesToSend -= sizeOfCurrentMessage;
                offset += sizeOfCurrentMessage;
            }
        }

        #region Channel virtual methods

        /// <summary>
        /// Called when channel window need to be adjust.
        /// </summary>
        /// <param name="bytesToAdd">The bytes to add.</param>
        protected virtual void OnWindowAdjust(uint bytesToAdd)
        {
            lock (_serverWindowSizeLock)
            {
                RemoteWindowSize += bytesToAdd;
            }
            _channelServerWindowAdjustWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected virtual void OnData(byte[] data)
        {
            AdjustDataWindow(data);

            var dataReceived = DataReceived;
            if (dataReceived != null)
                dataReceived(this, new ChannelDataEventArgs(LocalChannelNumber, data));
        }

        /// <summary>
        /// Called when channel extended data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="dataTypeCode">The data type code.</param>
        protected virtual void OnExtendedData(byte[] data, uint dataTypeCode)
        {
            AdjustDataWindow(data);

            var extendedDataReceived = ExtendedDataReceived;
            if (extendedDataReceived != null)
                extendedDataReceived(this, new ChannelExtendedDataEventArgs(LocalChannelNumber, data, dataTypeCode));
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected virtual void OnEof()
        {
            _eofMessageReceived = true;

            var endOfData = EndOfData;
            if (endOfData != null)
                endOfData(this, new ChannelEventArgs(LocalChannelNumber));
        }

        /// <summary>
        /// Called when channel is closed by the server.
        /// </summary>
        protected virtual void OnClose()
        {
            _closeMessageReceived = true;

            // Signal that SSH_MSG_CHANNEL_CLOSE message was received from server.
            // We need to signal this before invoking Close() as it may very well
            // be blocked waiting for this signal.
            var channelClosedWaitHandle = _channelClosedWaitHandle;
            if (channelClosedWaitHandle != null)
                channelClosedWaitHandle.Set();

            // close the channel
            Close();
        }

        /// <summary>
        /// Called when channel request received.
        /// </summary>
        /// <param name="info">Channel request information.</param>
        protected virtual void OnRequest(RequestInfo info)
        {
            var requestReceived = RequestReceived;
            if (requestReceived != null)
                requestReceived(this, new ChannelRequestEventArgs(info));
        }

        /// <summary>
        /// Called when channel request was successful
        /// </summary>
        protected virtual void OnSuccess()
        {
            var requestSuccessed = RequestSucceeded;
            if (requestSuccessed != null)
                requestSuccessed(this, new ChannelEventArgs(LocalChannelNumber));
        }

        /// <summary>
        /// Called when channel request failed.
        /// </summary>
        protected virtual void OnFailure()
        {
            var requestFailed = RequestFailed;
            if (requestFailed != null)
                requestFailed(this, new ChannelEventArgs(LocalChannelNumber));
        }

        #endregion // Channel virtual methods

        /// <summary>
        /// Raises <see cref="Exception"/> event.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void RaiseExceptionEvent(Exception exception)
        {
            var handlers = Exception;
            if (handlers != null)
            {
                handlers(this, new ExceptionEventArgs(exception));
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>
        /// <c>true</c> if the message was sent to the server; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">The size of the packet exceeds the maximum size defined by the protocol.</exception>
        /// <remarks>
        /// This methods returns <c>false</c> when the attempt to send the message results in a
        /// <see cref="SocketException"/> or a <see cref="SshException"/>.
        /// </remarks>
        private bool TrySendMessage(Message message)
        {
            return _session.TrySendMessage(message);
        }

        /// <summary>
        /// Sends SSH message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void SendMessage(Message message)
        {
            // send channel messages only while channel is open
            if (!IsOpen)
                return;

            _session.SendMessage(message);
        }

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_EOF message to the remote server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The channel is closed.</exception>
        public void SendEof()
        {
            if (!IsOpen)
                throw CreateChannelClosedException();

            lock (this)
            {
                _session.SendMessage(new ChannelEofMessage(RemoteChannelNumber));
                _eofMessageSent = true;
            }
        }

        /// <summary>
        /// Waits for the handle to be signaled or for an error to occurs.
        /// </summary>
        /// <param name="waitHandle">The wait handle.</param>
        protected void WaitOnHandle(WaitHandle waitHandle)
        {
            _session.WaitOnHandle(waitHandle);
        }

        /// <summary>
        /// Closes the channel, waiting for the SSH_MSG_CHANNEL_CLOSE message to be received from the server.
        /// </summary>
        protected virtual void Close()
        {
            // synchronize sending SSH_MSG_CHANNEL_EOF and SSH_MSG_CHANNEL_CLOSE to ensure that these messages
            // are sent in that other; when both the client and the server attempt to close the channel at the
            // same time we would otherwise risk sending the SSH_MSG_CHANNEL_EOF after the SSH_MSG_CHANNEL_CLOSE
            // message causing the server to disconnect the session.

            lock (this)
            {
                // send EOF message first the following conditions are met:
                // * we have not sent a SSH_MSG_CHANNEL_EOF message
                // * remote party has not already sent a SSH_MSG_CHANNEL_EOF message
                // * remote party has not already sent a SSH_MSG_CHANNEL_CLOSE message
                // * the channel is open
                // * the session is connected
                if (!_eofMessageSent && !_closeMessageReceived && !_eofMessageReceived && IsOpen && IsConnected)
                {
                    if (TrySendMessage(new ChannelEofMessage(RemoteChannelNumber)))
                    {
                        _eofMessageSent = true;
                    }
                }

                // send message to close the channel on the server when it has not already been sent
                // and the channel is open and the session is connected
                if (!_closeMessageSent && IsOpen && IsConnected)
                {
                    if (TrySendMessage(new ChannelCloseMessage(RemoteChannelNumber)))
                    {
                        _closeMessageSent = true;

                        // only wait for the channel to be closed by the server if we didn't send a
                        // SSH_MSG_CHANNEL_CLOSE as response to a SSH_MSG_CHANNEL_CLOSE sent by the
                        // server
                        var closeWaitResult = _session.TryWait(_channelClosedWaitHandle, ConnectionInfo.ChannelCloseTimeout);
                        if (closeWaitResult != WaitResult.Success)
                        {
                            DiagnosticAbstraction.Log(string.Format("Wait for channel close not successful: {0:G}.", closeWaitResult));
                        }
                    }
                }

                if (IsOpen)
                {
                    // mark sure the channel is marked closed before we raise the Closed event
                    // this also ensures don't raise the Closed event more than once
                    IsOpen = false;

                    if (_closeMessageReceived)
                    {
                        // raise event signaling that both ends of the channel have been closed
                        var closed = Closed;
                        if (closed != null)
                        {
                            closed(this, new ChannelEventArgs(LocalChannelNumber));
                        }
                    }
                }
            }
        }

        protected virtual void OnDisconnected()
        {
        }

        protected virtual void OnErrorOccured(Exception exp)
        {
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            IsOpen = false;

            try
            {
                OnDisconnected();
            }
            catch (Exception ex)
            {
                OnChannelException(ex);
            }
        }

        /// <summary>
        /// Called when an <see cref="Exception"/> occurs while processing a channel message.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        /// <remarks>
        /// This method will in turn invoke <see cref="OnErrorOccured(System.Exception)"/>, and
        /// raise the <see cref="Exception"/> event.
        /// </remarks>
        protected void OnChannelException(Exception ex)
        {
            OnErrorOccured(ex);
            RaiseExceptionEvent(ex);
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            try
            {
                OnErrorOccured(e.Exception);
            }
            catch (Exception ex)
            {
                RaiseExceptionEvent(ex);
            }
        }

        #region Channel message event handlers

        private void OnChannelWindowAdjust(object sender, MessageEventArgs<ChannelWindowAdjustMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnWindowAdjust(e.Message.BytesToAdd);
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelData(object sender, MessageEventArgs<ChannelDataMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnData(e.Message.Data);
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelExtendedData(object sender, MessageEventArgs<ChannelExtendedDataMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnExtendedData(e.Message.Data, e.Message.DataTypeCode);
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelEof(object sender, MessageEventArgs<ChannelEofMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnEof();
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelClose(object sender, MessageEventArgs<ChannelCloseMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnClose();
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelRequest(object sender, MessageEventArgs<ChannelRequestMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    RequestInfo requestInfo;

                    if (_session.ConnectionInfo.ChannelRequests.TryGetValue(e.Message.RequestName, out requestInfo))
                    {
                        //  Load request specific data
                        requestInfo.Load(e.Message.RequestData);

                        //  Raise request specific event
                        OnRequest(requestInfo);
                    }
                    else
                    {
                        // TODO: we should also send a SSH_MSG_CHANNEL_FAILURE message
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Request '{0}' is not supported.", e.Message.RequestName));
                    }
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelSuccess(object sender, MessageEventArgs<ChannelSuccessMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnSuccess();
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        private void OnChannelFailure(object sender, MessageEventArgs<ChannelFailureMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    OnFailure();
                }
                catch (Exception ex)
                {
                    OnChannelException(ex);
                }
            }
        }

        #endregion // Channel message event handlers

        private void AdjustDataWindow(byte[] messageData)
        {
            LocalWindowSize -= (uint)messageData.Length;

            //  Adjust window if window size is too low
            if (LocalWindowSize < LocalPacketSize)
            {
                SendMessage(new ChannelWindowAdjustMessage(RemoteChannelNumber, _initialWindowSize - LocalWindowSize));
                LocalWindowSize = _initialWindowSize;
            }
        }

        /// <summary>
        /// Determines the length of data that currently can be sent in a single message.
        /// </summary>
        /// <param name="messageLength">The length of the message that must be sent.</param>
        /// <returns>
        /// The actual data length that currently can be sent.
        /// </returns>
        private int GetDataLengthThatCanBeSentInMessage(int messageLength)
        {
            do
            {
                lock (_serverWindowSizeLock)
                {
                    var serverWindowSize = RemoteWindowSize;
                    if (serverWindowSize == 0U)
                    {
                        // allow us to be signal when remote window size is adjusted
                        _channelServerWindowAdjustWaitHandle.Reset();
                    }
                    else
                    {
                        var bytesThatCanBeSent = Math.Min(Math.Min(RemotePacketSize, (uint) messageLength),
                            serverWindowSize);
                        RemoteWindowSize -= bytesThatCanBeSent;
                        return (int) bytesThatCanBeSent;
                    }
                }
                // wait for remote window size to change
                WaitOnHandle(_channelServerWindowAdjustWaitHandle);
            } while (true);
        }

        private static InvalidOperationException CreateRemoteChannelInfoNotAvailableException()
        {
            throw new InvalidOperationException("The channel has not been opened, or the open has not yet been confirmed.");
        }

        private static InvalidOperationException CreateChannelClosedException()
        {
            throw new InvalidOperationException("The channel is closed.");
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
                Close();

                var session = _session;
                if (session != null)
                {
                    _session = null;
                    session.ChannelWindowAdjustReceived -= OnChannelWindowAdjust;
                    session.ChannelDataReceived -= OnChannelData;
                    session.ChannelExtendedDataReceived -= OnChannelExtendedData;
                    session.ChannelEofReceived -= OnChannelEof;
                    session.ChannelCloseReceived -= OnChannelClose;
                    session.ChannelRequestReceived -= OnChannelRequest;
                    session.ChannelSuccessReceived -= OnChannelSuccess;
                    session.ChannelFailureReceived -= OnChannelFailure;
                    session.ErrorOccured -= Session_ErrorOccured;
                    session.Disconnected -= Session_Disconnected;
                }

                var channelClosedWaitHandle = _channelClosedWaitHandle;
                if (channelClosedWaitHandle != null)
                {
                    _channelClosedWaitHandle = null;
                    channelClosedWaitHandle.Dispose();
                }

                var channelServerWindowAdjustWaitHandle = _channelServerWindowAdjustWaitHandle;
                if (channelServerWindowAdjustWaitHandle != null)
                {
                    _channelServerWindowAdjustWaitHandle = null;
                    channelServerWindowAdjustWaitHandle.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Channel"/> is reclaimed by garbage collection.
        /// </summary>
        ~Channel()
        {
            Dispose(false);
        }

        #endregion // IDisposable Members
    }
}
