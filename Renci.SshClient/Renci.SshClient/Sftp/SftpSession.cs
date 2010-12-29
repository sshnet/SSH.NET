using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class SftpSession : IDisposable
    {
        private Session _session;

        private uint _requestId;

        private ChannelSession _channel;

        private StringBuilder _data = new StringBuilder(32 * 1024, 32 * 1024);

        private Exception _exception;

        private EventWaitHandle _errorOccuredWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(false);

        private int _operationTimeout;

        public event EventHandler<ErrorEventArgs> ErrorOccured;

        public int ProtocolVersion { get; private set; }

        #region SFTP messages

        internal event EventHandler<MessageEventArgs<StatusMessage>> StatusMessageReceived;

        internal event EventHandler<MessageEventArgs<DataMessage>> DataMessageReceived;

        internal event EventHandler<MessageEventArgs<HandleMessage>> HandleMessageReceived;

        internal event EventHandler<MessageEventArgs<NameMessage>> NameMessageReceived;

        internal event EventHandler<MessageEventArgs<AttributesMessage>> AttributesMessageReceived;

        #endregion

        public SftpSession(Session session, int operationTimeout)
        {
            this._session = session;
            this._operationTimeout = operationTimeout;
        }

        public void Connect()
        {
            this._channel = this._session.CreateChannel<ChannelSession>();

            this._session.ErrorOccured += Session_ErrorOccured;
            this._session.Disconnected += Session_Disconnected;
            this._channel.DataReceived += Channel_DataReceived;

            this._channel.Open();

            this._channel.SendSubsystemRequest("sftp");

            this.SendMessage(new InitMessage
            {
                Version = 3,
            });

            this.WaitHandle(this._sftpVersionConfirmed, this._operationTimeout);

            this.ProtocolVersion = 3;
        }

        public void Disconnect()
        {
            //  Close SFTP channel
            this._channel.Close();
        }

        internal void SendMessage(SftpRequestMessage sftpMessage)
        {
            sftpMessage.RequestId = this._requestId++;

            this._session.SendMessage(new SftpDataMessage(this._channel.RemoteChannelNumber, sftpMessage));
        }

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            //  Add channel data to internal data holder
            this._data.Append(e.Data);

            while (this._data.Length > 4 + 1)
            {
                //  Extract packet length
                var packetLength = (this._data[0] << 24 | this._data[1] << 16 | this._data[2] << 8 | this._data[3]);

                //  Check if complete packet data is available
                if (this._data.Length < packetLength + 4)
                {
                    //  Wait for complete message to arrive first
                    break;
                }
                this._data.Remove(0, 4);

                //  Create buffer to hold packet data
                var packetData = new char[packetLength];

                //  Cope packet data to array
                this._data.CopyTo(0, packetData, 0, packetLength);

                //  Remove loaded data from _data holder
                this._data.Remove(0, packetLength);

                //  Load SFTP Message and handle it
                dynamic sftpMessage = SftpMessage.Load(packetData.Select((c) => (byte)c));

                try
                {
                    this.HandleMessage(sftpMessage);
                }
                catch (Exception exp)
                {
                    this.RaiseError(exp);
                    break;
                }
            }
        }

        #region Handle SFTP incoming messages and raise appropriate events

        private void HandleMessage(InitMessage message)
        {
            throw new InvalidOperationException("Init message should not be received by client.");
        }

        private void HandleMessage(VersionMessage message)
        {
            if (message.Version == 3)
            {
                this._sftpVersionConfirmed.Set();
            }
            else
            {
                throw new NotSupportedException(string.Format("Server SFTP version {0} is not supported.", message.Version));
            }
        }

        private void HandleMessage(StatusMessage message)
        {
            if (this.StatusMessageReceived != null)
            {
                this.StatusMessageReceived(this, new MessageEventArgs<StatusMessage>(message));
            }
        }

        private void HandleMessage(DataMessage message)
        {
            if (this.DataMessageReceived != null)
            {
                this.DataMessageReceived(this, new MessageEventArgs<DataMessage>(message));
            }
        }

        private void HandleMessage(HandleMessage message)
        {
            if (this.HandleMessageReceived != null)
            {
                this.HandleMessageReceived(this, new MessageEventArgs<HandleMessage>(message));
            }
        }

        private void HandleMessage(NameMessage message)
        {
            if (this.NameMessageReceived != null)
            {
                this.NameMessageReceived(this, new MessageEventArgs<NameMessage>(message));
            }
        }

        private void HandleMessage(AttributesMessage message)
        {
            if (this.AttributesMessageReceived != null)
            {
                this.AttributesMessageReceived(this, new MessageEventArgs<AttributesMessage>(message));
            }
        }

        #endregion

        private void Session_Disconnected(object sender, EventArgs e)
        {
            this.RaiseError(new SshException("Connection was lost"));
        }

        private void Session_ErrorOccured(object sender, ErrorEventArgs e)
        {
            this.RaiseError(e.GetException());
        }

        internal void WaitHandle(WaitHandle waitHandle, int operationTimeout)
        {
            var waitHandles = new WaitHandle[]
                {
                    this._errorOccuredWaitHandle,
                    waitHandle,
                };

            var index = EventWaitHandle.WaitAny(waitHandles, operationTimeout);

            if (index < 1)
            {
                throw this._exception;
            }
            else if (index > 1)
            {
                //  throw time out error
                throw new SshOperationTimeoutException(string.Format("Sftp operation has timed out."));
            }
        }

        private void SendMessage(SftpMessage sftpMessage)
        {
            var message = new SftpDataMessage(this._channel.RemoteChannelNumber, sftpMessage);

            this._session.SendMessage(message);
        }

        private void RaiseError(Exception error)
        {
            this._exception = error;

            this._errorOccuredWaitHandle.Set();

            if (this.ErrorOccured != null)
            {
                this.ErrorOccured(this, new ErrorEventArgs(error));
            }
        }

        #region IDisposable Members

        private bool _isDisposed = false;

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
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._channel != null)
                    {
                        this._channel.Dispose();
                    }
                    if (this._errorOccuredWaitHandle != null)
                    {
                        this._errorOccuredWaitHandle.Dispose();
                    }
                    if (this._sftpVersionConfirmed != null)
                    {
                        this._sftpVersionConfirmed.Dispose();
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SftpSession"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpSession()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
