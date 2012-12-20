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
        private EventWaitHandle _channelClosedWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelWindowAdjustWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);

        private EventWaitHandle _disconnectedWaitHandle = new ManualResetEvent(false);

        private bool _closeMessageSent = false;

        private uint _initialWindowSize = 0x100000;

        private uint _maximumPacketSize = 0x8000;

        private Session _session;

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
        /// Gets the remote channel number assigned by the server.
        /// </summary>
        public uint RemoteChannelNumber { get; private set; }

        /// <summary>
        /// Gets the size of the local window.
        /// </summary>
        /// <value>
        /// The size of the local window.
        /// </value>
        public uint LocalWindowSize { get; private set; }

        /// <summary>
        /// Gets or sets the size of the server window.
        /// </summary>
        /// <value>
        /// The size of the server window.
        /// </value>
        public uint ServerWindowSize { get; protected set; }

        /// <summary>
        /// Gets the size of the packet.
        /// </summary>
        /// <value>
        /// The size of the packet.
        /// </value>
        public uint PacketSize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this channel is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this channel is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen { get; private set; }

        #region Message events

        /// <summary>
        /// Occurs when <see cref="ChannelOpenFailureMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelOpenFailedEventArgs> OpenFailed;

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
        /// 	<c>true</c> if the session is connected; otherwise, <c>false</c>.
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
            get
            {
                return this._session.ConnectionInfo;
            }
        }

        /// <summary>
        /// Gets the session semaphore to control number of session channels
        /// </summary>
        /// <value>The session semaphore.</value>
        protected SemaphoreLight SessionSemaphore
        {
            get
            {
                return this._session.SessionSemaphore;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        internal Channel()
        {
        }

        /// <summary>
        /// Initializes the channel.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="serverChannelNumber">The server channel number.</param>
        /// <param name="windowSize">Size of the window.</param>
        /// <param name="packetSize">Size of the packet.</param>
        internal virtual void Initialize(Session session, uint serverChannelNumber, uint windowSize, uint packetSize)
        {
            this._initialWindowSize = windowSize;
            this._maximumPacketSize = Math.Max(packetSize, 0x8000); //  Ensure minimum maximum packet size of 0x8000 bytes

            this._session = session;
            this.LocalWindowSize = this._initialWindowSize;  // Initial window size
            this.PacketSize = this._maximumPacketSize;     // Maximum packet size

            this.LocalChannelNumber = session.NextChannelNumber;
            this.RemoteChannelNumber = serverChannelNumber;

            this._session.ChannelOpenReceived += OnChannelOpen;
            this._session.ChannelOpenConfirmationReceived += OnChannelOpenConfirmation;
            this._session.ChannelOpenFailureReceived += OnChannelOpenFailure;
            this._session.ChannelWindowAdjustReceived += OnChannelWindowAdjust;
            this._session.ChannelDataReceived += OnChannelData;
            this._session.ChannelExtendedDataReceived += OnChannelExtendedData;
            this._session.ChannelEofReceived += OnChannelEof;
            this._session.ChannelCloseReceived += OnChannelClose;
            this._session.ChannelRequestReceived += OnChannelRequest;
            this._session.ChannelSuccessReceived += OnChannelSuccess;
            this._session.ChannelFailureReceived += OnChannelFailure;
            this._session.ErrorOccured += Session_ErrorOccured;
            this._session.Disconnected += Session_Disconnected;
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
        /// Called when channel need to be open on the client.
        /// </summary>
        /// <param name="info">Channel open information.</param>
        protected virtual void OnOpen(ChannelOpenInfo info)
        {
        }

        /// <summary>
        /// Called when channel is opened by the server.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected virtual void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            this.RemoteChannelNumber = remoteChannelNumber;
            this.ServerWindowSize = initialWindowSize;
            this.PacketSize = maximumPacketSize;

            //  Chanel consider to be open when confirmation message was received
            this.IsOpen = true;
        }

        /// <summary>
        /// Called when channel failed to open.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="description">The description.</param>
        /// <param name="language">The language.</param>
        protected virtual void OnOpenFailure(uint reasonCode, string description, string language)
        {
            if (this.OpenFailed != null)
            {
                this.OpenFailed(this, new ChannelOpenFailedEventArgs(this.LocalChannelNumber, reasonCode, description, language));
            }
        }

        /// <summary>
        /// Called when channel window need to be adjust.
        /// </summary>
        /// <param name="bytesToAdd">The bytes to add.</param>
        protected virtual void OnWindowAdjust(uint bytesToAdd)
        {
            this.ServerWindowSize += bytesToAdd;
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected virtual void OnData(byte[] data)
        {
            this.AdjustDataWindow(data);

            if (this.DataReceived != null)
            {
                this.DataReceived(this, new ChannelDataEventArgs(this.LocalChannelNumber, data));
            }
        }

        /// <summary>
        /// Called when channel extended data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="dataTypeCode">The data type code.</param>
        protected virtual void OnExtendedData(byte[] data, uint dataTypeCode)
        {
            this.AdjustDataWindow(data);

            if (this.ExtendedDataReceived != null)
            {
                this.ExtendedDataReceived(this, new ChannelDataEventArgs(this.LocalChannelNumber, data, dataTypeCode));
            }
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected virtual void OnEof()
        {
            if (this.EndOfData != null)
            {
                this.EndOfData(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        /// <summary>
        /// Called when channel is closed by the server.
        /// </summary>
        protected virtual void OnClose()
        {
            this.Close(false);

            if (this.Closed != null)
            {
                this.Closed(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        /// <summary>
        /// Called when channel request received.
        /// </summary>
        /// <param name="info">Channel request information.</param>
        protected virtual void OnRequest(RequestInfo info)
        {
            if (this.RequestReceived != null)
            {
                this.RequestReceived(this, new ChannelRequestEventArgs(info));
            }
        }

        /// <summary>
        /// Called when channel request was successful
        /// </summary>
        protected virtual void OnSuccess()
        {
            if (this.RequestSuccessed != null)
            {
                this.RequestSuccessed(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        /// <summary>
        /// Called when channel request failed.
        /// </summary>
        protected virtual void OnFailure()
        {
            if (this.RequestFailed != null)
            {
                this.RequestFailed(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        #endregion

        /// <summary>
        /// Sends SSH message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void SendMessage(Message message)
        {
            //  Send channel messages only while channel is open
            if (!this.IsOpen)
                return;

            this._session.SendMessage(message);
        }

        protected void SendMessage(ChannelOpenConfirmationMessage message)
        {
            //  No need to check whether channel is open when trying to open a channel
            this._session.SendMessage(message);

            //  Chanel consider to be open when confirmation message is sent
            this.IsOpen = true;
        }

        /// <summary>
        /// Send message to open a channel.
        /// </summary>
        /// <param name="message">Message to send</param>
        protected void SendMessage(ChannelOpenMessage message)
        {
            //  No need to check whether channel is open when trying to open a channel
            this._session.SendMessage(message);
        }

        /// <summary>
        /// Sends close channel message to the server
        /// </summary>
        /// <param name="message">Message to send.</param>
        protected void SendMessage(ChannelCloseMessage message)
        {
            //  Send channel messages only while channel is open
            if (!this.IsOpen)
                return;

            this._session.SendMessage(message);

            //  When channel close message is sent channel considred to be closed
            this.IsOpen = false;
        }

        /// <summary>
        /// Sends channel data message to the servers.
        /// </summary>
        /// <remarks>This method takes care of managing the window size.</remarks>
        /// <param name="message">Channel data message.</param>
        protected void SendMessage(ChannelDataMessage message)
        {
            //  Send channel messages only while channel is open
            if (!this.IsOpen)
                return;

            if (this.ServerWindowSize < 1)
            {
                //  Wait for window to be adjust
                this._session.WaitHandle(this._channelWindowAdjustWaitHandle);
            }

            this.ServerWindowSize -= (uint)message.Data.Length;
            this._session.SendMessage(message);
        }

        /// <summary>
        /// Sends channel extended data message to the servers.
        /// </summary>
        /// <remarks>This method takes care of managing the window size.</remarks>
        /// <param name="message">Channel data message.</param>
        protected void SendMessage(ChannelExtendedDataMessage message)
        {
            //  Send channel messages only while channel is open
            if (!this.IsOpen)
                return;

            if (this.ServerWindowSize < 1)
            {
                //  Wait for window to be adjust
                this._session.WaitHandle(this._channelWindowAdjustWaitHandle);
            }

            this.ServerWindowSize -= (uint)message.Data.Length;
            this._session.SendMessage(message);
        }

        /// <summary>
        /// Waits for the handle to be signaled or for an error to occurs.
        /// </summary>
        /// <param name="waitHandle">The wait handle.</param>
        protected void WaitHandle(WaitHandle waitHandle)
        {
            this._session.WaitHandle(waitHandle);
        }

        protected virtual void Close(bool wait)
        {
            if (!wait)
            {
                this._session.ChannelOpenReceived -= OnChannelOpen;
                this._session.ChannelOpenConfirmationReceived -= OnChannelOpenConfirmation;
                this._session.ChannelOpenFailureReceived -= OnChannelOpenFailure;
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
            }

            //  Send message to close the channel on the server
            if (!_closeMessageSent)
            {
                this.SendMessage(new ChannelCloseMessage(this.RemoteChannelNumber));
                this._closeMessageSent = true;
            }

            //  Wait for channel to be closed
            if (wait)
            {
                this._session.WaitHandle(this._channelClosedWaitHandle);
            }
        }


        private void Session_Disconnected(object sender, EventArgs e)
        {
            //  If objected is disposed or being disposed don't handle this event
            if (this._isDisposed)
                return;

            this._disconnectedWaitHandle.Set();
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            //  If objected is disposed or being disposed don't handle this event
            if (this._isDisposed)
                return;

            this._errorOccuredWaitHandle.Set();
        }

        #region Channel message event handlers

        private void OnChannelOpen(object sender, MessageEventArgs<ChannelOpenMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnOpen(e.Message.Info);
            }
        }

        private void OnChannelOpenConfirmation(object sender, MessageEventArgs<ChannelOpenConfirmationMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnOpenConfirmation(e.Message.RemoteChannelNumber, e.Message.InitialWindowSize, e.Message.MaximumPacketSize);
            }
        }

        private void OnChannelOpenFailure(object sender, MessageEventArgs<ChannelOpenFailureMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnOpenFailure(e.Message.ReasonCode, e.Message.Description, e.Message.Language);
            }
        }

        private void OnChannelWindowAdjust(object sender, MessageEventArgs<ChannelWindowAdjustMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnWindowAdjust(e.Message.BytesToAdd);

                this._channelWindowAdjustWaitHandle.Set();
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

                this._channelClosedWaitHandle.Set();
            }
        }

        private void OnChannelRequest(object sender, MessageEventArgs<ChannelRequestMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                if (this._session.ConnectionInfo.ChannelRequests.ContainsKey(e.Message.RequestName))
                {
                    //  Get request specific class
                    RequestInfo requestInfo = this._session.ConnectionInfo.ChannelRequests[e.Message.RequestName];

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
            if (this.LocalWindowSize < this.PacketSize)
            {
                this.SendMessage(new ChannelWindowAdjustMessage(this.RemoteChannelNumber, this._initialWindowSize - this.LocalWindowSize));
                this.LocalWindowSize = this._initialWindowSize;
            }
        }

        #region IDisposable Members

        private bool _isDisposed = false;

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
                    if (this._channelWindowAdjustWaitHandle != null)
                    {
                        this._channelWindowAdjustWaitHandle.Dispose();
                        this._channelWindowAdjustWaitHandle = null;
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
                this._session.ChannelOpenReceived -= OnChannelOpen;
                this._session.ChannelOpenConfirmationReceived -= OnChannelOpenConfirmation;
                this._session.ChannelOpenFailureReceived -= OnChannelOpenFailure;
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
            this.Dispose(false);
        }

        #endregion
    }
}
