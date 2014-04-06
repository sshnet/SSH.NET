 using System;
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
    internal abstract class Channel : IDisposable
    {
        private EventWaitHandle _channelClosedWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _channelServerWindowAdjustWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);
        private EventWaitHandle _disconnectedWaitHandle = new ManualResetEvent(false);
        private readonly object _serverWindowSizeLock = new object();
        private bool _closeMessageSent;
        private uint _initialWindowSize;
        private uint? _remoteWindowSize;
        private uint? _remoteChannelNumber;
        private uint? _remotePacketSize;
        private Session _session;

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        ///  Thhe session.
        /// </value>
        protected Session Session
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
        public event EventHandler<ChannelEventArgs> RequestSuccessed;

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
            get { return this._session.IsConnected; }
        }

        /// <summary>
        /// Gets the connection info.
        /// </summary>
        /// <value>The connection info.</value>
        protected ConnectionInfo ConnectionInfo
        {
            get { return this._session.ConnectionInfo; }
        }

        /// <summary>
        /// Gets the session semaphore to control number of session channels
        /// </summary>
        /// <value>The session semaphore.</value>
        protected SemaphoreLight SessionSemaphore
        {
            get { return this._session.SessionSemaphore; }
        }

        /// <summary>
        /// Initializes the channel.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        internal virtual void Initialize(Session session, uint localWindowSize, uint localPacketSize)
        {
            _session = session;
            _initialWindowSize = localWindowSize;
            LocalPacketSize = localPacketSize;
            LocalWindowSize = localWindowSize;  // Initial window size
            LocalChannelNumber = session.NextChannelNumber;

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

        protected void InitializeRemoteInfo(uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
        {
            RemoteChannelNumber = remoteChannelNumber;
            RemoteWindowSize = remoteWindowSize;
            RemotePacketSize = remotePacketSize;
        }

        /// <summary>
        /// Sends the SSH_MSG_CHANNEL_EOF message.
        /// </summary>
        internal void SendEof()
        {
            //  Send EOF message first when channel need to be closed
            this.SendMessage(new ChannelEofMessage(this.RemoteChannelNumber));
        }

        internal void SendData(byte[] buffer)
        {
            this.SendMessage(new ChannelDataMessage(this.RemoteChannelNumber, buffer));
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        public virtual void Close()
        {
            this.Close(true);
        }

        #region Channel virtual methods

        /// <summary>
        /// Called when channel window need to be adjust.
        /// </summary>
        /// <param name="bytesToAdd">The bytes to add.</param>
        protected virtual void OnWindowAdjust(uint bytesToAdd)
        {
            lock (this._serverWindowSizeLock)
            {
                this.RemoteWindowSize += bytesToAdd;
            }
            this._channelServerWindowAdjustWaitHandle.Set();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected virtual void OnData(byte[] data)
        {
            this.AdjustDataWindow(data);

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
            this.AdjustDataWindow(data);

            var extendedDataReceived = ExtendedDataReceived;
            if (extendedDataReceived != null)
                extendedDataReceived(this, new ChannelDataEventArgs(LocalChannelNumber, data, dataTypeCode));
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected virtual void OnEof()
        {
            var endOfData = EndOfData;
            if (endOfData != null)
                endOfData(this, new ChannelEventArgs(LocalChannelNumber));
        }

        /// <summary>
        /// Called when channel is closed by the server.
        /// </summary>
        protected virtual void OnClose()
        {
            this.Close(false);

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
            var requestSuccessed = RequestSuccessed;
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
        /// Sends SSH message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void SendMessage(Message message)
        {
            // send channel messages only while channel is open
            if (!this.IsOpen)
                return;

            this._session.SendMessage(message);
        }

        /// <summary>
        /// Sends close channel message to the server, and marks the channel closed.
        /// </summary>
        /// <param name="message">The message to send.</param>
        private void SendMessage(ChannelCloseMessage message)
        {
            // send channel messages only while channel is open
            if (!this.IsOpen)
                return;

            this._session.SendMessage(message);

            // when channel close message is sent channel considered to be closed
            this.IsOpen = false;
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
            if (!this.IsOpen)
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
                    this._session.SendMessage(message);
                }
                else
                {
                    // we need to send the message in multiple chunks
                    var dataToSend = new byte[dataThatCanBeSentInMessage];
                    Array.Copy(message.Data, totalDataSent, dataToSend, 0, dataThatCanBeSentInMessage);
                    this._session.SendMessage(new ChannelDataMessage(message.LocalChannelNumber, dataToSend));
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
            // end channel messages only while channel is open
            if (!this.IsOpen)
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
                    this._session.SendMessage(message);
                }
                else
                {
                    // we need to send the message in multiple chunks
                    var dataToSend = new byte[dataThatCanBeSentInMessage];
                    Array.Copy(message.Data, totalDataSent, dataToSend, 0, dataThatCanBeSentInMessage);
                    this._session.SendMessage(new ChannelExtendedDataMessage(message.LocalChannelNumber,
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
            this._session.WaitOnHandle(waitHandle);
        }

        protected virtual void Close(bool wait)
        {
            // send message to close the channel on the server
            // ignore sending close message when client not connected
            if (!_closeMessageSent && this.IsConnected)
            {
                lock (this)
                {
                    if (!_closeMessageSent)
                    {
                        this.SendMessage(new ChannelCloseMessage(this.RemoteChannelNumber));
                        this._closeMessageSent = true;
                    }
                }
            }
            else
            {
                // also mark the channel closed if the session is no longer connected
                IsOpen = false;
            }

            // wait for channel to be closed
            if (wait)
            {
                WaitOnHandle(this._channelClosedWaitHandle);
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
            this.OnDisconnected();

            //  If object is disposed or being disposed don't handle this event
            if (this._isDisposed)
                return;

            var disconnectedWaitHandle = this._disconnectedWaitHandle;
            if (disconnectedWaitHandle != null)
                disconnectedWaitHandle.Set();
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.OnErrorOccured(e.Exception);

            //  If object is disposed or being disposed don't handle this event
            if (this._isDisposed)
                return;

            var errorOccuredWaitHandle = this._errorOccuredWaitHandle;
            if (errorOccuredWaitHandle != null)
                errorOccuredWaitHandle.Set();
        }

        #region Channel message event handlers

        private void OnChannelWindowAdjust(object sender, MessageEventArgs<ChannelWindowAdjustMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnWindowAdjust(e.Message.BytesToAdd);
            }
        }

        private void OnChannelData(object sender, MessageEventArgs<ChannelDataMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnData(e.Message.Data);
            }
        }

        private void OnChannelExtendedData(object sender, MessageEventArgs<ChannelExtendedDataMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnExtendedData(e.Message.Data, e.Message.DataTypeCode);
            }
        }

        private void OnChannelEof(object sender, MessageEventArgs<ChannelEofMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnEof();
            }
        }

        private void OnChannelClose(object sender, MessageEventArgs<ChannelCloseMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnClose();

                var channelClosedWaitHandle = _channelClosedWaitHandle;
                if (channelClosedWaitHandle != null)
                    channelClosedWaitHandle.Set();
            }
        }

        private void OnChannelRequest(object sender, MessageEventArgs<ChannelRequestMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                if (this._session.ConnectionInfo.ChannelRequests.ContainsKey(e.Message.RequestName))
                {
                    //  Get request specific class
                    var requestInfo = this._session.ConnectionInfo.ChannelRequests[e.Message.RequestName];

                    //  Load request specific data
                    requestInfo.Load(e.Message.RequestData);

                    //  Raise request specific event
                    this.OnRequest(requestInfo);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Request '{0}' is not supported.", e.Message.RequestName));
                }
            }
        }

        private void OnChannelSuccess(object sender, MessageEventArgs<ChannelSuccessMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnSuccess();
            }
        }

        private void OnChannelFailure(object sender, MessageEventArgs<ChannelFailureMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnFailure();
            }
        }

        #endregion

        private void AdjustDataWindow(byte[] messageData)
        {
            this.LocalWindowSize -= (uint)messageData.Length;

            //  Adjust window if window size is too low
            if (this.LocalWindowSize < this.LocalPacketSize)
            {
                this.SendMessage(new ChannelWindowAdjustMessage(this.RemoteChannelNumber, this._initialWindowSize - this.LocalWindowSize));
                this.LocalWindowSize = this._initialWindowSize;
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
                lock (this._serverWindowSizeLock)
                {
                    var serverWindowSize = RemoteWindowSize;
                    if (serverWindowSize == 0)
                    {
                        // allow us to be signal when remote window size is adjusted
                        this._channelServerWindowAdjustWaitHandle.Reset();
                    }
                    else
                    {
                        var bytesThatCanBeSent = Math.Min(Math.Min(RemotePacketSize, (uint) messageLength),
                            serverWindowSize);
                        this.RemoteWindowSize -= bytesThatCanBeSent;
                        return (int) bytesThatCanBeSent;
                    }
                }
                // wait for remote window size to change
                this.WaitOnHandle(this._channelServerWindowAdjustWaitHandle);
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
            this.Dispose(true);
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
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    this.Close(false);

                    // Dispose managed resources.
                    if (this._channelClosedWaitHandle != null)
                    {
                        this._channelClosedWaitHandle.Dispose();
                        this._channelClosedWaitHandle = null;
                    }
                    if (this._channelServerWindowAdjustWaitHandle != null)
                    {
                        this._channelServerWindowAdjustWaitHandle.Dispose();
                        this._channelServerWindowAdjustWaitHandle = null;
                    }
                    if (this._errorOccuredWaitHandle != null)
                    {
                        this._errorOccuredWaitHandle.Dispose();
                        this._errorOccuredWaitHandle = null;
                    }
                    if (this._disconnectedWaitHandle != null)
                    {
                        this._disconnectedWaitHandle.Dispose();
                        this._disconnectedWaitHandle = null;
                    }
                }

                //  Ensure that all events are detached from current instance
                this._session.ChannelWindowAdjustReceived -= OnChannelWindowAdjust;
                this._session.ChannelDataReceived -= OnChannelData;
                this._session.ChannelExtendedDataReceived -= OnChannelExtendedData;
                this._session.ChannelEofReceived -= OnChannelEof;
                this._session.ChannelCloseReceived -= OnChannelClose;
                this._session.ChannelRequestReceived -= OnChannelRequest;
                this._session.ChannelSuccessReceived -= OnChannelSuccess;
                this._session.ChannelFailureReceived -= OnChannelFailure;
                this._session.ErrorOccured -= Session_ErrorOccured;
                this._session.Disconnected -= Session_Disconnected;

                // Note disposing has been done.
                this._isDisposed = true;
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
            this.Dispose(false);
        }

        #endregion
    }
}
