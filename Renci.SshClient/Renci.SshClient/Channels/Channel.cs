using System;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal abstract class Channel : IDisposable
    {
        private EventWaitHandle _channelClosedWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelWindowAdjustWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _errorOccuredWaitHandle = new ManualResetEvent(false);

        private EventWaitHandle _disconnectedWaitHandle = new ManualResetEvent(false);

        private uint _initialWindowSize = 0x100000;

        private uint _maximumPacketSize = 0x8000;

        /// <summary>
        /// Indicates weither close channel message was sent
        /// </summary>
        private bool _closeMessageSent = false;

        private Session _session;

        public abstract ChannelTypes ChannelType { get; }

        public uint LocalChannelNumber { get; private set; }

        public uint RemoteChannelNumber { get; private set; }

        public uint LocalWindowSize { get; private set; }

        public uint ServerWindowSize { get; protected set; }

        public uint PacketSize { get; private set; }

        public bool IsOpen { get; private set; }

        #region Message events

        /// <summary>
        /// Occurs when <see cref="ChannelOpenFailureMessage"/> message received
        /// </summary>
        public event EventHandler<ChannelOpenFailedEventArgs> OpenFailed;

        ///// <summary>
        ///// Occurs when <see cref="ChannelWindowAdjustMessage"/> message received
        ///// </summary>
        //public event EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> ChannelWindowAdjustReceived;

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
        protected SemaphoreSlim SessionSemaphore
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

        public virtual void Close()
        {
            if (this.IsOpen)
            {
                this.SendChannelCloseMessage();

                //  Wait for channel to be closed
                this._session.WaitHandle(this._channelClosedWaitHandle);
            }

            this.CloseCleanup();
        }

        #region Channel virtual methods

        protected virtual void OnOpen(ChannelOpenInfo info)
        {
        }

        protected virtual void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            this.RemoteChannelNumber = remoteChannelNumber;
            this.ServerWindowSize = initialWindowSize;
            this.PacketSize = maximumPacketSize;

            this.IsOpen = true;
        }

        protected virtual void OnOpenFailure(uint reasonCode, string description, string language)
        {
            if (this.OpenFailed != null)
            {
                this.OpenFailed(this, new ChannelOpenFailedEventArgs(this.LocalChannelNumber, reasonCode, description, language));
            }
        }

        protected virtual void OnWindowAdjust(uint bytesToAdd)
        {
            this.ServerWindowSize += bytesToAdd;
            this._channelWindowAdjustWaitHandle.Set();
        }

        protected virtual void OnData(string data)
        {
            this.AdjustDataWindow(data);

            if (this.DataReceived != null)
            {
                this.DataReceived(this, new ChannelDataEventArgs(this.LocalChannelNumber, data));
            }
        }

        protected virtual void OnExtendedData(string data, uint dataTypeCode)
        {
            this.AdjustDataWindow(data);

            if (this.ExtendedDataReceived != null)
            {
                this.ExtendedDataReceived(this, new ChannelDataEventArgs(this.LocalChannelNumber, data, dataTypeCode));
            }
        }

        protected virtual void OnEof()
        {
            if (this.EndOfData != null)
            {
                this.EndOfData(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        protected virtual void OnClose()
        {
            this.CloseCleanup();

            this._channelClosedWaitHandle.Set();

            if (this.Closed != null)
            {
                this.Closed(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        protected virtual void OnRequest(RequestInfo info)
        {
            if (this.RequestReceived != null)
            {
                this.RequestReceived(this, new ChannelRequestEventArgs(info));
            }
        }

        protected virtual void OnSuccess()
        {
            if (this.RequestSuccessed != null)
            {
                this.RequestSuccessed(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        protected virtual void OnFailure()
        {
            if (this.RequestFailed != null)
            {
                this.RequestFailed(this, new ChannelEventArgs(this.LocalChannelNumber));
            }
        }

        #endregion

        protected void SendMessage(Message message)
        {
            this._session.SendMessage(message);
        }

        protected void SendMessage(ChannelDataMessage message)
        {
            if (this.ServerWindowSize < 1)
            {
                //  Wait for window to be adjust
                this._session.WaitHandle(this._channelWindowAdjustWaitHandle);
            }

            this.ServerWindowSize -= (uint)message.Data.Length;
            this._session.SendMessage(message);
        }

        protected void SendMessage(ChannelExtendedDataMessage message)
        {
            if (this.ServerWindowSize < 1)
            {
                //  Wait for window to be adjust
                this._session.WaitHandle(this._channelWindowAdjustWaitHandle);
            }

            this.ServerWindowSize -= (uint)message.Data.Length;
            this._session.SendMessage(message);
        }

        protected void CloseCleanup()
        {
            if (!this.IsOpen)
                return;

            this.IsOpen = false;

            this.SendChannelCloseMessage();

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

        protected void WaitHandle(WaitHandle waitHandle)
        {
            this._session.WaitHandle(waitHandle);
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            this._disconnectedWaitHandle.Set();
        }

        private void Session_ErrorOccured(object sender, System.IO.ErrorEventArgs e)
        {
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
            }
        }

        private void OnChannelRequest(object sender, MessageEventArgs<ChannelRequestMessage> e)
        {
            if (e.Message.LocalChannelNumber == this.LocalChannelNumber)
            {
                this.OnRequest(e.Message.Info);
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

        private void AdjustDataWindow(string messageData)
        {
            this.LocalWindowSize -= (uint)messageData.Length;

            //  Adjust window if window size is too low
            if (this.LocalWindowSize < this.PacketSize)
            {
                this.SendMessage(new ChannelWindowAdjustMessage
                {
                    LocalChannelNumber = this.RemoteChannelNumber,
                    BytesToAdd = this._initialWindowSize - this.LocalWindowSize,
                });
                this.LocalWindowSize = this._initialWindowSize;
            }
        }

        private void SendChannelCloseMessage()
        {
            if (this._closeMessageSent)
                return;

            lock (this)
            {
                if (this._closeMessageSent)
                    return;

                this.SendMessage(new ChannelCloseMessage
                {
                    LocalChannelNumber = this.RemoteChannelNumber,
                });
                this._closeMessageSent = true;
            }
        }

        #region IDisposable Members

        protected abstract void OnDisposing();

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
                    if (this._channelClosedWaitHandle != null)
                    {
                        this._channelClosedWaitHandle.Dispose();
                    }
                    if (this._channelWindowAdjustWaitHandle != null)
                    {
                        this._channelWindowAdjustWaitHandle.Dispose();
                    }
                    if (this._errorOccuredWaitHandle != null)
                    {
                        this._errorOccuredWaitHandle.Dispose();
                    }
                    if (this._disconnectedWaitHandle != null)
                    {
                        this._disconnectedWaitHandle.Dispose();
                    }

                    this.OnDisposing();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

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
