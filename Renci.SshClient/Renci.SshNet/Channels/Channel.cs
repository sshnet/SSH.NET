using System;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System.Globalization;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Represents base class for SSH channel implementations.
    /// </summary>
    internal abstract class Channel : IChannel
    {
        private const int Initial = 0;
        private const int Considered = 1;
        private const int Sent = 2;

        private EventWaitHandle _channelClosedWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _channelServerWindowAdjustWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);
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
        /// <c>0</c> when the SSH_MSG_CHANNEL_CLOSE message has not been sent or considered
        /// <c>1</c> when sending a SSH_MSG_CHANNEL_CLOSE message to the remote party is under consideration
        /// <c>2</c> when this message has been sent to the remote party
        /// </value>
        private int _closeMessageSent;

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
        /// <c>0</c> when the SSH_MSG_CHANNEL_EOF message has not been sent or considered
        /// <c>1</c> when sending a SSH_MSG_CHANNEL_EOF message to the remote party is under consideration
        /// <c>2</c> when this message has been sent to the remote party
        /// </value>
        private int _eofMessageSent;

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

            _session.ChannelWindowAdjustReceived += OnChannelWindowAdjust;
            _session.ChannelDataReceived += OnChannelData;
            _session.ChannelExtendedDataReceived += OnChannelExtendedData;
            _session.ChannelEofReceived += OnChannelEof;
            _session.ChannelCloseReceived += OnChannelClose;
            _session.ChannelRequestReceived += OnChannelRequest;
            _session.ChannelSuccessReceived += OnChannelSuccess;
            _session.ChannelFailureReceived += OnChannelFailure;
            _session.ErrorOccured += Session_ErrorOccured;
            _session.Disconnected += Session_Disconnected;
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
        /// Gets the maximum size of a packet.
        /// </summary>
        /// <value>
        /// The maximum size of a packet.
        /// </value>
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
        /// Occurs when <see cref="ChannelDataMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelExtendedDataMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelDataEventArgs> ExtendedDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelEofMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelEventArgs> EndOfData;

        /// <summary>
        /// Occurs when <see cref="ChannelCloseMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelEventArgs> Closed;

        /// <summary>
        /// Occurs when <see cref="ChannelRequestMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelRequestEventArgs> RequestReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelSuccessMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelEventArgs> RequestSucceeded;

        /// <summary>
        /// Occurs when <see cref="ChannelFailureMessage"/> message received
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
        /// Gets the session semaphore to control number of session channels
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
            SendMessage(new ChannelDataMessage(RemoteChannelNumber, data));
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        public void Close()
        {
            Close(true);
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
                extendedDataReceived(this, new ChannelDataEventArgs(LocalChannelNumber, data, dataTypeCode));
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

            // close the channel
            Close(false);

            var closed = Closed;
            if (closed != null)
                closed(this, new ChannelEventArgs(LocalChannelNumber));
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

        #endregion

        /// <summary>
        /// Raises <see cref="Channel.Exception"/> event.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected void RaiseExceptionEvent(Exception exception)
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
        /// Sends channel data message to the servers.
        /// </summary>
        /// <param name="message">Channel data message.</param>
        /// <remarks>
        /// <para>
        /// When the data of the message exceeds the maximum packet size or the remote window
        /// size does not allow the full message to be sent, then this method will send the
        /// data in multiple chunks and will only wait for the remote window size to be adjusted
        /// when its zero.
        /// </para>
        /// <para>
        /// This is done to support SSH servers will a small window size that do not agressively
        /// increase their window size. We need to take into account that there may be SSH
        /// servers that only increase their window size when it has reached zero.
        /// </para>
        /// </remarks>
        protected void SendMessage(ChannelDataMessage message)
        {
            // send channel messages only while channel is open
            if (!IsOpen)
                return;

            var totalDataLength = message.Data.Length;
            var totalDataSent = 0;

            var totalBytesToSend = totalDataLength;
            while (totalBytesToSend > 0)
            {
                var dataThatCanBeSentInMessage = GetDataLengthThatCanBeSentInMessage(totalBytesToSend);
                if (dataThatCanBeSentInMessage == totalDataLength)
                {
                    // we can send the message in one chunk
                    _session.SendMessage(message);
                }
                else
                {
                    // we need to send the message in multiple chunks
                    var dataToSend = new byte[dataThatCanBeSentInMessage];
                    Array.Copy(message.Data, totalDataSent, dataToSend, 0, dataThatCanBeSentInMessage);
                    _session.SendMessage(new ChannelDataMessage(message.LocalChannelNumber, dataToSend));
                }
                totalDataSent += dataThatCanBeSentInMessage;
                totalBytesToSend -= dataThatCanBeSentInMessage;
            }
        }

        /// <summary>
        /// Sends channel extended data message to the servers.
        /// </summary>
        /// <param name="message">Channel data message.</param>
        /// <remarks>
        /// <para>
        /// When the data of the message exceeds the maximum packet size or the remote window
        /// size does not allow the full message to be sent, then this method will send the
        /// data in multiple chunks and will only wait for the remote window size to be adjusted
        /// when its zero.
        /// </para>
        /// <para>
        /// This is done to support SSH servers will a small window size that do not agressively
        /// increase their window size. We need to take into account that there may be SSH
        /// servers that only increase their window size when it has reached zero.
        /// </para>
        /// </remarks>
        protected void SendMessage(ChannelExtendedDataMessage message)
        {
            // send channel messages only while channel is open
            if (!IsOpen)
                return;

            var totalDataLength = message.Data.Length;
            var totalDataSent = 0;

            var totalBytesToSend = totalDataLength;
            while (totalBytesToSend > 0)
            {
                var dataThatCanBeSentInMessage = GetDataLengthThatCanBeSentInMessage(totalBytesToSend);
                if (dataThatCanBeSentInMessage == totalDataLength)
                {
                    // we can send the message in one chunk
                    _session.SendMessage(message);
                }
                else
                {
                    // we need to send the message in multiple chunks
                    var dataToSend = new byte[dataThatCanBeSentInMessage];
                    Array.Copy(message.Data, totalDataSent, dataToSend, 0, dataThatCanBeSentInMessage);
                    _session.SendMessage(new ChannelExtendedDataMessage(message.LocalChannelNumber,
                        message.DataTypeCode, dataToSend));
                }
                totalDataSent += dataThatCanBeSentInMessage;
                totalBytesToSend -= dataThatCanBeSentInMessage;
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
        /// Closes the channel, optionally waiting for the SSH_MSG_CHANNEL_CLOSE message to
        /// be received from the server.
        /// </summary>
        /// <param name="wait"><c>true</c> to wait for the SSH_MSG_CHANNEL_CLOSE message to be received from the server; otherwise, <c>false</c>.</param>
        protected virtual void Close(bool wait)
        {
            // send EOF message first when channel need to be closed, and the remote party has not already sent
            // a SSH_MSG_CHANNEL_EOF or SSH_MSG_CHANNEL_CLOSE message
            //
            // note that we might have had a race condition here when the remote party sends a SSH_MSG_CHANNEL_CLOSE
            // immediately after it has sent a SSH_MSG_CHANNEL_EOF message
            //
            // in that case, we would risk sending a SSH_MSG_CHANNEL_EOF message after the remote party has
            // closed its end of the channel
            //
            // as a solution for this issue we only send a SSH_MSG_CHANNEL_EOF message if we haven't received a
            // SSH_MSG_CHANNEL_EOF or SSH_MSG_CHANNEL_CLOSE message from the remote party
            if (Interlocked.CompareExchange(ref _eofMessageSent, Considered, Initial) == Initial)
            {
                if (!_closeMessageReceived && !_eofMessageReceived && IsOpen && IsConnected)
                {
                    if (TrySendMessage(new ChannelEofMessage(RemoteChannelNumber)))
                        _eofMessageSent = Sent;
                }
            }

            // send message to close the channel on the server
            if (Interlocked.CompareExchange(ref _closeMessageSent, Considered, Initial) == Initial)
            {
                // ignore sending close message when client is not connected or the channel is closed
                if (IsOpen && IsConnected)
                {
                    if (TrySendMessage(new ChannelCloseMessage(RemoteChannelNumber)))
                        _closeMessageSent = Sent;
                }
            }

            // mark the channel closed
            IsOpen = false;

            // wait for channel to be closed if we actually sent a close message (either to initiate closing
            // the channel, or as response to a SSH_MSG_CHANNEL_CLOSE message sent by the server
            if (wait && _closeMessageSent == Sent)
            {
                WaitOnHandle(_channelClosedWaitHandle);
            }

            // reset indicators in case we want to reopen the channel; these are safe to reset
            // since the channel is marked closed by now
            _eofMessageSent = Initial;
            _eofMessageReceived = false;
            _closeMessageReceived = false;
            _closeMessageSent = Initial;
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

                var errorOccuredWaitHandle = _errorOccuredWaitHandle;
                if (errorOccuredWaitHandle != null)
                    errorOccuredWaitHandle.Set();
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

                var channelClosedWaitHandle = _channelClosedWaitHandle;
                if (channelClosedWaitHandle != null)
                    channelClosedWaitHandle.Set();
            }
        }

        private void OnChannelRequest(object sender, MessageEventArgs<ChannelRequestMessage> e)
        {
            if (e.Message.LocalChannelNumber == LocalChannelNumber)
            {
                try
                {
                    if (_session.ConnectionInfo.ChannelRequests.ContainsKey(e.Message.RequestName))
                    {
                        //  Get request specific class
                        var requestInfo = _session.ConnectionInfo.ChannelRequests[e.Message.RequestName];

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

        #endregion

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
                    if (serverWindowSize == 0)
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

        private InvalidOperationException CreateRemoteChannelInfoNotAvailableException()
        {
            throw new InvalidOperationException("The channel has not been opened, or the open has not yet been confirmed.");
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
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Close(false);

                    if (_session != null)
                    {
                        _session.ChannelWindowAdjustReceived -= OnChannelWindowAdjust;
                        _session.ChannelDataReceived -= OnChannelData;
                        _session.ChannelExtendedDataReceived -= OnChannelExtendedData;
                        _session.ChannelEofReceived -= OnChannelEof;
                        _session.ChannelCloseReceived -= OnChannelClose;
                        _session.ChannelRequestReceived -= OnChannelRequest;
                        _session.ChannelSuccessReceived -= OnChannelSuccess;
                        _session.ChannelFailureReceived -= OnChannelFailure;
                        _session.ErrorOccured -= Session_ErrorOccured;
                        _session.Disconnected -= Session_Disconnected;
                        _session = null;
                    }

                    if (_channelClosedWaitHandle != null)
                    {
                        _channelClosedWaitHandle.Dispose();
                        _channelClosedWaitHandle = null;
                    }
                    if (_channelServerWindowAdjustWaitHandle != null)
                    {
                        _channelServerWindowAdjustWaitHandle.Dispose();
                        _channelServerWindowAdjustWaitHandle = null;
                    }
                    if (_errorOccuredWaitHandle != null)
                    {
                        _errorOccuredWaitHandle.Dispose();
                        _errorOccuredWaitHandle = null;
                    }
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
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
