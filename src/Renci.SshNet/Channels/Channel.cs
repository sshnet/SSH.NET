using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
#if NET6_0_OR_GREATER
using System.Threading.Tasks;
#endif

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Represents base class for SSH channel implementations.
    /// </summary>
    internal abstract class Channel : IChannel
    {
        private readonly object _serverWindowSizeLock = new object();
        private readonly object _messagingLock = new object();
        private readonly uint _initialWindowSize;
        private readonly ISession _session;
        private EventWaitHandle _channelClosedWaitHandle = new ManualResetEvent(initialState: false);
        private EventWaitHandle _channelServerWindowAdjustWaitHandle = new ManualResetEvent(initialState: false);
        private uint? _remoteWindowSize;
        private uint? _remoteChannelNumber;
        private uint? _remotePacketSize;
        private bool _isDisposed;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_CLOSE has been sent to the remote party.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when a SSH_MSG_CHANNEL_CLOSE message has been sent to the other party;
        /// otherwise, <see langword="false"/>.
        /// </value>
        private bool _closeMessageSent;

        /// <summary>
        /// Holds a value indicating whether a SSH_MSG_CHANNEL_CLOSE has been received from the other
        /// party.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when a SSH_MSG_CHANNEL_CLOSE message has been received from the other party;
        /// otherwise, <see langword="false"/>.
        /// </value>
        private bool _closeMessageReceived;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_EOF has been received from the other party.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when a SSH_MSG_CHANNEL_EOF message has been received from the other party;
        /// otherwise, <see langword="false"/>.
        /// </value>
        private bool _eofMessageReceived;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_EOF has been sent to the remote party.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when a SSH_MSG_CHANNEL_EOF message has been sent to the remote party;
        /// otherwise, <see langword="false"/>.
        /// </value>
        private bool _eofMessageSent;

        /// <summary>
        /// Occurs when an exception is thrown when processing channel messages.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
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
                {
                    throw CreateRemoteChannelInfoNotAvailableException();
                }

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
                {
                    throw CreateRemoteChannelInfoNotAvailableException();
                }

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
                {
                    throw CreateRemoteChannelInfoNotAvailableException();
                }

                return _remoteWindowSize.Value;
            }
            private set
            {
                _remoteWindowSize = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this channel is open.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this channel is open; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsOpen { get; protected set; }

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

        /// <summary>
        /// Gets a value indicating whether the session is connected.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the session is connected; otherwise, <see langword="false"/>.
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
        protected SemaphoreSlim SessionSemaphore
        {
            get { return _session.SessionSemaphore; }
        }

        /// <summary>
        /// Initializes the information on the remote channel.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="remoteWindowSize">The remote window size.</param>
        /// <param name="remotePacketSize">The remote packet size.</param>
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
            {
                return;
            }

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

#if NET6_0_OR_GREATER
        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_DATA message with the specified payload.
        /// </summary>
        /// <param name="data">An array of <see cref="byte"/> containing the payload to send.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin taking data from.</param>
        /// <param name="size">The number of bytes of <paramref name="data"/> to send.</param>
        /// <param name="token">The cancellation token.</param>
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
        public async Task SendDataAsync(byte[] data, int offset, int size, CancellationToken token)
        {
            // send channel messages only while channel is open
            if (!IsOpen)
            {
                return;
            }

            var totalBytesToSend = size;
            while (totalBytesToSend > 0)
            {
                var sizeOfCurrentMessage = GetDataLengthThatCanBeSentInMessage(totalBytesToSend);

                var channelDataMessage = new ChannelDataMessage(RemoteChannelNumber,
                                                                data,
                                                                offset,
                                                                sizeOfCurrentMessage);
                await _session.SendMessageAsync(channelDataMessage, token).ConfigureAwait(false);

                totalBytesToSend -= sizeOfCurrentMessage;
                offset += sizeOfCurrentMessage;
            }
        }
#endif

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

            _ = _channelServerWindowAdjustWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected virtual void OnData(byte[] data)
        {
            AdjustDataWindow(data);

            DataReceived?.Invoke(this, new ChannelDataEventArgs(LocalChannelNumber, data));
        }

        /// <summary>
        /// Called when channel extended data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="dataTypeCode">The data type code.</param>
        protected virtual void OnExtendedData(byte[] data, uint dataTypeCode)
        {
            AdjustDataWindow(data);

            ExtendedDataReceived?.Invoke(this, new ChannelExtendedDataEventArgs(LocalChannelNumber, data, dataTypeCode));
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected virtual void OnEof()
        {
            _eofMessageReceived = true;

            EndOfData?.Invoke(this, new ChannelEventArgs(LocalChannelNumber));
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
            {
                _ = channelClosedWaitHandle.Set();
            }

            // close the channel
            Close();
        }

        /// <summary>
        /// Called when channel request received.
        /// </summary>
        /// <param name="info">Channel request information.</param>
        protected virtual void OnRequest(RequestInfo info)
        {
            RequestReceived?.Invoke(this, new ChannelRequestEventArgs(info));
        }

        /// <summary>
        /// Called when channel request was successful.
        /// </summary>
        protected virtual void OnSuccess()
        {
            RequestSucceeded?.Invoke(this, new ChannelEventArgs(LocalChannelNumber));
        }

        /// <summary>
        /// Called when channel request failed.
        /// </summary>
        protected virtual void OnFailure()
        {
            RequestFailed?.Invoke(this, new ChannelEventArgs(LocalChannelNumber));
        }

        /// <summary>
        /// Raises <see cref="Exception"/> event.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void RaiseExceptionEvent(Exception exception)
        {
            Exception?.Invoke(this, new ExceptionEventArgs(exception));
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>
        /// <see langword="true"/> if the message was sent to the server; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">The size of the packet exceeds the maximum size defined by the protocol.</exception>
        /// <remarks>
        /// This methods returns <see langword="false"/> when the attempt to send the message results in a
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
            // Send channel messages only while channel is open
            if (!IsOpen)
            {
                return;
            }

            _session.SendMessage(message);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Sends SSH message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous connect operation.</returns>
        protected async Task SendMessageAsync(Message message, CancellationToken token)
        {
            // Send channel messages only while channel is open
            if (!IsOpen)
            {
                return;
            }

            await _session.SendMessageAsync(message, token).ConfigureAwait(false);
        }
#endif

        /// <summary>
        /// Sends a SSH_MSG_CHANNEL_EOF message to the remote server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The channel is closed.</exception>
        public void SendEof()
        {
            if (!IsOpen)
            {
                throw CreateChannelClosedException();
            }

            lock (_messagingLock)
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
            /*
             * Synchronize sending SSH_MSG_CHANNEL_EOF and SSH_MSG_CHANNEL_CLOSE to ensure that these messages
             * are sent in that other; when both the client and the server attempt to close the channel at the
             * same time we would otherwise risk sending the SSH_MSG_CHANNEL_EOF after the SSH_MSG_CHANNEL_CLOSE
             * message causing the server to disconnect the session.
             */

            lock (_messagingLock)
            {
                // Send EOF message first the following conditions are met:
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
                        Closed?.Invoke(this, new ChannelEventArgs(LocalChannelNumber));
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
                    if (_session.ConnectionInfo.ChannelRequests.TryGetValue(e.Message.RequestName, out var requestInfo))
                    {
                        // Load request specific data
                        requestInfo.Load(e.Message.RequestData);

                        // Raise request specific event
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

        private void AdjustDataWindow(byte[] messageData)
        {
            LocalWindowSize -= (uint) messageData.Length;

            // Adjust window if window size is too low
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
                        // Allow us to be signal when remote window size is adjusted
                        _ = _channelServerWindowAdjustWaitHandle.Reset();
                    }
                    else
                    {
                        var bytesThatCanBeSent = Math.Min(Math.Min(RemotePacketSize, (uint) messageLength),
                            serverWindowSize);
                        RemoteWindowSize -= bytesThatCanBeSent;
                        return (int) bytesThatCanBeSent;
                    }
                }

                // Wait for remote window size to change
                WaitOnHandle(_channelServerWindowAdjustWaitHandle);
            }
            while (true);
        }

        private static InvalidOperationException CreateRemoteChannelInfoNotAvailableException()
        {
            throw new InvalidOperationException("The channel has not been opened, or the open has not yet been confirmed.");
        }

        private static InvalidOperationException CreateChannelClosedException()
        {
            throw new InvalidOperationException("The channel is closed.");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                Close();

                var session = _session;
                if (session is not null)
                {
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
                if (channelClosedWaitHandle is not null)
                {
                    _channelClosedWaitHandle = null;
                    channelClosedWaitHandle.Dispose();
                }

                var channelServerWindowAdjustWaitHandle = _channelServerWindowAdjustWaitHandle;
                if (channelServerWindowAdjustWaitHandle is not null)
                {
                    _channelServerWindowAdjustWaitHandle = null;
                    channelServerWindowAdjustWaitHandle.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Channel"/> class.
        /// </summary>
        ~Channel()
        {
            Dispose(disposing: false);
        }
    }
}
