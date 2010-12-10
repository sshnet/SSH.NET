using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient.Sftp
{
    internal class SftpSession : IDisposable
    {
        private Session _session;

        private uint _requestId;

        private ChannelSession _channel;

        private StringBuilder _packetData;

        private Exception _exception;

        private EventWaitHandle _errorOccuredWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(false);

        public int _operationTimeout;

        public event EventHandler<ErrorEventArgs> ErrorOccured;

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
        }

        public void Disconnect()
        {
            //  Close SFTP channel
            this._channel.Close();
        }

        internal void SendMessage(SftpRequestMessage sftpMessage)
        {
            sftpMessage.RequestId = this._requestId++;

            var message = new SftpDataMessage
            {
                LocalChannelNumber = this._channel.RemoteChannelNumber,
                Message = sftpMessage,
            };
            this._session.SendMessage(message);
        }

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            var packets = new Queue<string>();

            if (this._packetData == null)
            {
                var dataOffset = 0;
                while (true)
                {
                    //  Read SFTP packet length
                    var packetLength = (e.Data[dataOffset + 0] << 24 | e.Data[dataOffset + 1] << 16 | e.Data[dataOffset + 2] << 8 | e.Data[dataOffset + 3]);

                    //  Create data holder for SFTP packet
                    this._packetData = new StringBuilder(packetLength, packetLength);

                    //  Add data to the packet holder
                    this._packetData.Append(e.Data.GetSshBytes().Skip(dataOffset + 4).Take((int)packetLength).GetSshString());

                    dataOffset += (packetLength + 4);

                    if (dataOffset < e.Data.Length)
                    {
                        //  If there is another SFTP packet in current message then queue this data and read next one
                        packets.Enqueue(this._packetData.ToString());
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                //  Add message data to packet data
                this._packetData.Append(e.Data);
            }

            if (this._packetData.Length < this._packetData.MaxCapacity)
            {
                //  Wait for more packet data
                return;
            }

            //  Add last packet to the queue of packet data that need to be proccessed
            packets.Enqueue(this._packetData.ToString());

            this._packetData = null;

            foreach (var packetData in packets)
            {
                dynamic sftpMessage = SftpMessage.Load(packetData.GetSshBytes());

                try
                {
                    //  TODO:   Check to run on different thread
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

            //if (message.StatusCode == StatusCodes.NoSuchFile ||
            //    message.StatusCode == StatusCodes.PermissionDenied ||
            //    message.StatusCode == StatusCodes.Failure ||
            //    message.StatusCode == StatusCodes.BadMessage ||
            //    message.StatusCode == StatusCodes.NoConnection ||
            //    message.StatusCode == StatusCodes.ConnectionLost ||
            //    message.StatusCode == StatusCodes.OperationUnsupported
            //    )
            //{
            //    //  Throw an exception if it was not handled by the command
            //    throw new SshException(message.ErrorMessage);
            //}

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
            var message = new SftpDataMessage
            {
                LocalChannelNumber = this._channel.RemoteChannelNumber,
                Message = sftpMessage,
            };

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
                disposed = true;
            }
        }

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
