using System;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal abstract class Channel : IDisposable
    {
        private EventWaitHandle _channelOpenWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelClosedWaitHandle = new AutoResetEvent(false);

        private uint _initialWindowSize = 0x100000;

        private uint _maximumPacketSize = 0x8000;

        /// <summary>
        /// Counts faile channel open attempts
        /// </summary>
        private int _failedOpenAttempts;

        /// <summary>
        /// Indicates weither close channel message was sent
        /// </summary>
        private bool _closeMessageSent = false;

        public abstract ChannelTypes ChannelType { get; }

        public uint ClientChannelNumber { get; private set; }

        public uint ServerChannelNumber { get; private set; }

        public uint LocalWindowSize { get; private set; }

        public uint ServerWindowSize { get; private set; }

        public uint PacketSize { get; private set; }

        public bool IsOpen { get; private set; }

        public event EventHandler<ChannelEventArgs> Opened;

        public event EventHandler<ChannelEventArgs> Closed;

        public event EventHandler<ChannelEventArgs> OpenFailed;

        protected Session Session { get; private set; }

        public Channel(Session session, uint channelId, uint windowSize, uint packetSize)
        {
            this._initialWindowSize = windowSize;
            this._maximumPacketSize = Math.Max(packetSize, 0x8000); //  Ensure minimum maximum packet size of 0x8000 bytes

            this.ClientChannelNumber = channelId;

            this.Session = session;
            this.LocalWindowSize = this._initialWindowSize;  // Initial window size
            this.PacketSize = this._maximumPacketSize;     // Maximum packet size

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

        public Channel(Session session, uint channelId)
            : this(session, channelId, 0x100000, 0x8000)
        {
        }

        public virtual void Open()
        {
            //  Open session channel
            if (!this.IsOpen)
            {
                this.SendChannelOpenMessage();
                this.Session.WaitHandle(this._channelOpenWaitHandle);
                //  TODO:   Throw exception if channel could not be open, this can happend after sever failed open attempts
            }
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

        protected virtual void OnChannelClose()
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
            //  TODO:   Adjust server window size if needed
            this.ServerWindowSize -= (uint)message.Data.Length;
            this.Session.SendMessage(message);
        }

        protected void SendMessage(ChannelExtendedDataMessage message)
        {
            //  TODO:   Adjust server window size if needed
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
            this.ServerChannelNumber = message.ServerChannelNumber;
            this.ServerWindowSize = message.InitialWindowSize;
            this.PacketSize = message.MaximumPacketSize;

            this.IsOpen = true;

            this.RaiseOpened();

            this._channelOpenWaitHandle.Set();
        }

        private void HandleMessage(ChannelOpenFailureMessage message)
        {
            if (this._failedOpenAttempts < this.Session.ConnectionInfo.RetryAttempts)
            {
                this.SendChannelOpenMessage();
                this._failedOpenAttempts++;
            }
            else
            {
                this.RaiseOpenFailed();

                this.CloseCleanup();

                this.OnChannelFailed(message.ReasonCode, message.Description);

                this._channelOpenWaitHandle.Set();
            }
        }

        private void HandleMessage(ChannelWindowAdjustMessage message)
        {
            this.ServerWindowSize += message.BytesToAdd;
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
            Message replyMessage = new ChannelFailureMessage()
            {
                ChannelNumber = message.ChannelNumber,
            };

            if (message.RequestName == RequestNames.ExitStatus)
            {
                var exitStatus = message.ExitStatus;

                replyMessage = new ChannelSuccessMessage()
                {
                    ChannelNumber = message.ChannelNumber,
                };

                //  TODO:   if exitStatus is not 0 then throw an exception or notify user that command failed to execute correctly
            }
            else
            {
                throw new NotImplementedException(string.Format("Request name {0} is not implemented.", message.RequestName));
            }

            if (message.WantReply)
            {
                this.SendMessage(replyMessage);
            }
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

            this.RaiseClosed();

            this.CloseCleanup();

            this._channelClosedWaitHandle.Set();
        }

        #endregion

        private void AdjustDataWindow(string messageData)
        {
            this.LocalWindowSize -= (uint)messageData.Length;

            //  Adjust window if window size is too low
            if (this.LocalWindowSize < 1)
            {
                this.SendMessage(new ChannelWindowAdjustMessage
                {
                    ChannelNumber = this.ServerChannelNumber,
                    BytesToAdd = this._initialWindowSize,
                });
                this.LocalWindowSize = this._initialWindowSize;
            }
        }

        private void RaiseOpened()
        {
            if (this.Opened != null)
            {
                this.Opened(this, new ChannelEventArgs(this.ClientChannelNumber));
            }
        }

        private void RaiseClosed()
        {
            if (this.Closed != null)
            {
                this.Closed(this, new ChannelEventArgs(this.ClientChannelNumber));
            }
        }

        private void RaiseOpenFailed()
        {
            if (this.OpenFailed != null)
            {
                this.OpenFailed(this, new ChannelEventArgs(this.ClientChannelNumber));
            }
        }

        internal void SendChannelOpenMessage()
        {
            this.SendMessage(new ChannelOpenMessage
            {
                ChannelName = "session",
                ChannelNumber = this.ClientChannelNumber,
                InitialWindowSize = this.LocalWindowSize,
                MaximumPacketSize = this.PacketSize,
            });
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
                    ChannelNumber = this.ServerChannelNumber,
                });
                this._closeMessageSent = true;
            }
        }

        private void CloseCleanup()
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
                    if (this._channelOpenWaitHandle != null)
                    {
                        this._channelOpenWaitHandle.Dispose();
                    }
                    if (this._channelClosedWaitHandle != null)
                    {
                        this._channelClosedWaitHandle.Dispose();
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
