
using System;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Connection;
namespace Renci.SshClient.Channels
{
    internal abstract class Channel
    {
        private static uint _channelCounter = 0;

        private static object _lock = new object();

        private EventWaitHandle _channelOpenWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _channelClosedWaitHandle = new AutoResetEvent(false);

        private uint _initialWindowSize = 0x100000;

        private uint _maximumPacketSize = 0x4000;

        public abstract ChannelTypes ChannelType { get; }

        public uint ClientChannelNumber { get; set; }

        public uint ServerChannelNumber { get; set; }

        public uint WindowSize { get; set; }

        public uint PacketSize { get; set; }

        public bool IsOpen { get; protected set; }

        protected Session Session { get; private set; }

        public Channel(Session session, uint windowSize, uint packetSize)
        {
            this._initialWindowSize = windowSize;
            this._maximumPacketSize = Math.Max(packetSize, 0x8000); //  Ensure minimum maximum packet size of 0x8000 bytes

            lock (_lock)
            {
                //  TODO:   Refactor to make channel number to come from the session, to avoid situation where new session will be open and first channel number will not be 0
                this.ClientChannelNumber = _channelCounter++;
            }

            this.Session = session;
            this.WindowSize = this._initialWindowSize;  // Initial window size
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

        public Channel(Session session)
            : this(session, 0x100000, 0x8000)
        {
        }

        public virtual void Open()
        {
            this.Session.MessageReceived += SessionInfo_MessageReceived;

            //  Open session channel
            if (!this.IsOpen)
            {
                this.SendMessage(new ChannelOpenMessage
                {
                    ChannelName = "session",
                    ChannelNumber = this.ClientChannelNumber,
                    InitialWindowSize = this.WindowSize,
                    MaximumPacketSize = this.PacketSize,
                });

                this.Session.WaitHandle(this._channelOpenWaitHandle);
            }
        }

        public virtual void Close()
        {
            if (this.IsOpen)
            {
                this.SendMessage(new ChannelCloseMessage
                {
                    ChannelNumber = this.ServerChannelNumber,
                });

                //  Wait for channel to be closed
                this.Session.WaitHandle(this._channelClosedWaitHandle);
            }

            this.CloseCleanup();
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

        private void SessionInfo_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ChannelMessage message = e.Message as ChannelMessage;

            //  Handle only messages belong to this channel or channel open confirmation
            if (message.ChannelNumber == this.ClientChannelNumber || e.Message is ChannelOpenConfirmationMessage)
            {
                this.HandleMessage((dynamic)e.Message);
            }
        }

        #region Message handlers

        private void HandleMessage<T>(T message) where T : Message
        {
            throw new NotSupportedException(string.Format("Message type '{0}' is not supported.", message.MessageType));
        }

        private void HandleMessage(ChannelOpenConfirmationMessage message)
        {
            //  Make sure we open channel only for requested channel number
            if (this.ClientChannelNumber != message.ChannelNumber)
                return;

            this.ServerChannelNumber = message.ServerChannelNumber;
            this.IsOpen = true;
            this.WindowSize = message.InitialWindowSize;
            this.PacketSize = message.MaximumPacketSize;
            this._channelOpenWaitHandle.Set();
        }

        private void HandleMessage(ChannelOpenFailureMessage message)
        {
            this.IsOpen = false;
            this._channelOpenWaitHandle.Set();
            this.OnChannelFailed(message.ReasonCode, message.Description);
        }

        private void HandleMessage(ChannelWindowAdjustMessage message)
        {
            this.WindowSize += message.BytesToAdd;
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
            this.CloseCleanup();

            this._channelClosedWaitHandle.Set();
        }

        private void AdjustDataWindow(string messageData)
        {
            this.WindowSize -= (uint)messageData.Length;

            //  Adjust window if window size is too low
            if (this.WindowSize < this._initialWindowSize / 2)
            {
                this.SendMessage(new ChannelWindowAdjustMessage
                {
                    ChannelNumber = this.ServerChannelNumber,
                    BytesToAdd = this._initialWindowSize - this.WindowSize,
                });
                this.WindowSize = this._initialWindowSize;
            }
        }

        #endregion

        private void CloseCleanup()
        {
            this.IsOpen = false;

            this.Session.MessageReceived -= SessionInfo_MessageReceived;
        }
    }
}
