using System;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal abstract class Channel : IDisposable
    {
        private EventWaitHandle _channelClosedWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelWindowAdjustWaitHandle = new AutoResetEvent(false);

        private uint _initialWindowSize = 0x100000;

        private uint _maximumPacketSize = 0x8000;

        /// <summary>
        /// Indicates weither close channel message was sent
        /// </summary>
        private bool _closeMessageSent = false;

        public abstract ChannelTypes ChannelType { get; }

        public uint LocalChannelNumber { get; private set; }

        public uint RemoteChannelNumber { get; private set; }

        public uint LocalWindowSize { get; private set; }

        public uint ServerWindowSize { get; protected set; }

        public uint PacketSize { get; private set; }

        public bool IsOpen { get; private set; }

        protected Session Session { get; private set; }

        internal Channel()
        {
        }

        internal virtual void Initialize(Session session, uint serverChannelNumber, uint windowSize, uint packetSize)
        {
            this._initialWindowSize = windowSize;
            this._maximumPacketSize = Math.Max(packetSize, 0x8000); //  Ensure minimum maximum packet size of 0x8000 bytes

            this.Session = session;
            this.LocalWindowSize = this._initialWindowSize;  // Initial window size
            this.PacketSize = this._maximumPacketSize;     // Maximum packet size

            this.LocalChannelNumber = session.NextChannelNumber;
            this.RemoteChannelNumber = serverChannelNumber;

            this.Session.RegisterMessageType<ChannelOpenConfirmationMessage>(MessageTypes.ChannelOpenConfirmation);
            this.Session.RegisterMessageType<ChannelOpenFailureMessage>(MessageTypes.ChannelOpenFailure);
            this.Session.RegisterMessageType<ChannelWindowAdjustMessage>(MessageTypes.ChannelWindowAdjust);
            this.Session.RegisterMessageType<ChannelExtendedDataMessage>(MessageTypes.ChannelExtendedData);
            this.Session.RegisterMessageType<ChannelRequestMessage>(MessageTypes.ChannelRequest);
            this.Session.RegisterMessageType<ChannelSuccessMessage>(MessageTypes.ChannelSuccess);
            this.Session.RegisterMessageType<ChannelDataMessage>(MessageTypes.ChannelData);
            this.Session.RegisterMessageType<ChannelEofMessage>(MessageTypes.ChannelEof);
            this.Session.RegisterMessageType<ChannelCloseMessage>(MessageTypes.ChannelClose);

        }

        public virtual void Close()
        {
            if (this.IsOpen)
            {
                this.SendChannelCloseMessage();

                //  Wait for channel to be closed
                this.Session.WaitHandle(this._channelClosedWaitHandle);
            }

            this.CloseCleanup();
        }

        internal void HandleChannelMessage(ChannelMessage message)
        {
            this.HandleMessage((dynamic)message);
        }

        protected virtual void OnChannelData(string data)
        {
        }

        protected virtual void OnChannelExtendedData(string data, uint dataTypeCode)
        {
        }

        protected virtual void OnChannelSuccess()
        {
        }

        protected virtual void OnChannelEof()
        {
        }

        protected virtual void OnChannelOpen()
        {
        }

        protected virtual void OnChannelClose()
        {
        }

        protected virtual void OnChannelRequest(ChannelRequestMessage message)
        {
        }

        protected virtual void OnChannelFailed(uint reasonCode, string description)
        {
        }

        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

        protected void SendMessage(ChannelDataMessage message)
        {
            if (this.ServerWindowSize < 1)
            {
                //  Wait for window to be adjust
                this.Session.WaitHandle(this._channelWindowAdjustWaitHandle);
            }

            this.ServerWindowSize -= (uint)message.Data.Length;
            this.Session.SendMessage(message);
        }

        protected void SendMessage(ChannelExtendedDataMessage message)
        {
            if (this.ServerWindowSize < 1)
            {
                //  Wait for window to be adjust
                this.Session.WaitHandle(this._channelWindowAdjustWaitHandle);
            }

            this.ServerWindowSize -= (uint)message.Data.Length;
            this.Session.SendMessage(message);
        }

        #region Message handlers

        private void HandleMessage<T>(T message) where T : Message
        {
            throw new NotSupportedException(string.Format("Message type '{0}' is not supported.", message.MessageType));
        }

        private void HandleMessage(ChannelOpenConfirmationMessage message)
        {
            this.RemoteChannelNumber = message.RemoteChannelNumber;
            this.ServerWindowSize = message.InitialWindowSize;
            this.PacketSize = message.MaximumPacketSize;

            this.IsOpen = true;

            this.OnChannelOpen();
        }

        private void HandleMessage(ChannelOpenFailureMessage message)
        {
            this.OnChannelFailed(message.ReasonCode, message.Description);
        }

        private void HandleMessage(ChannelWindowAdjustMessage message)
        {
            this.ServerWindowSize += message.BytesToAdd;
            this._channelWindowAdjustWaitHandle.Set();
        }

        private void HandleMessage(ChannelDataMessage message)
        {
            this.AdjustDataWindow(message.Data);
            this.OnChannelData(message.Data);
        }

        private void HandleMessage(ChannelExtendedDataMessage message)
        {
            this.AdjustDataWindow(message.Data);
            this.OnChannelExtendedData(message.Data, message.DataTypeCode);
        }

        private void HandleMessage(ChannelRequestMessage message)
        {
            this.OnChannelRequest(message);
        }

        private void HandleMessage(ChannelSuccessMessage message)
        {
            this.OnChannelSuccess();
        }

        private void HandleMessage(ChannelEofMessage message)
        {
            this.OnChannelEof();
        }

        private void HandleMessage(ChannelCloseMessage message)
        {
            this.OnChannelClose();

            this.CloseCleanup();

            this._channelClosedWaitHandle.Set();
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

        protected void CloseCleanup()
        {
            if (!this.IsOpen)
                return;

            this.IsOpen = false;
            this.SendChannelCloseMessage();
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
