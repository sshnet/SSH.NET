using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Messages;
using System.Threading;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Base class for all SFTP Commands
    /// </summary>
    internal abstract class SftpCommand : IDisposable
    {
        private uint _requestId;

        private bool _handleCloseMessageSent;

        protected SftpAsyncResult AsyncResult { get; private set; }

        protected SftpSession SftpSession { get; private set; }

        protected bool IsStatusHandled { get; set; }

        public TimeSpan CommandTimeout { get; set; }

        public SftpCommand(SftpSession sftpSession)
        {
            this.SftpSession = sftpSession;

            this.SftpSession.AttributesMessageReceived += SftpSession_AttributesMessageReceived;
            this.SftpSession.DataMessageReceived += SftpSession_DataMessageReceived;
            this.SftpSession.HandleMessageReceived += SftpSession_HandleMessageReceived;
            this.SftpSession.NameMessageReceived += SftpSession_NameMessageReceived;
            this.SftpSession.StatusMessageReceived += SftpSession_StatusMessageReceived;
            this.SftpSession.ErrorOccured += SftpSession_ErrorOccured;
        }

        public SftpAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            this.AsyncResult = new SftpAsyncResult(this.SftpSession, this.CommandTimeout, callback, state);

            this.OnExecute();

            return this.AsyncResult;
        }

        public void EndExecute(SftpAsyncResult result)
        {
            result.EndInvoke();
        }

        public void Execute()
        {
            this.EndExecute(this.BeginExecute(null, null));
        }

        protected abstract void OnExecute();

        protected virtual void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            if (this._handleCloseMessageSent)
            {
                this.OnHandleClosed();

                this._handleCloseMessageSent = false;
            }
        }

        protected virtual void OnName(IDictionary<string, SftpFileAttributes> files)
        {
        }

        protected virtual void OnHandle(byte[] handle)
        {
        }

        protected virtual void OnData(byte[] data, bool isEof)
        {
        }

        protected virtual void OnAttributes(SftpFileAttributes attributes)
        {
        }

        protected virtual void OnHandleClosed()
        {
        }

        protected void SendOpenMessage(string path, Flags flags)
        {
            this.SendMessage(new OpenMessage(this.SftpSession.NextRequestId, path, flags));
        }

        protected void SendCloseMessage(byte[] handle)
        {
            this.SendMessage(new CloseMessage(this.SftpSession.NextRequestId, handle));

            this._handleCloseMessageSent = true;
        }

        protected void SendReadMessage(byte[] handle, ulong offset, uint bufferSize)
        {
            this.SendMessage(new ReadMessage(this.SftpSession.NextRequestId, handle, offset, bufferSize));
        }

        protected void SendWriteMessage(byte[] handle, ulong offset, byte[] data)
        {
            this.SendMessage(new WriteMessage(this.SftpSession.NextRequestId, handle, offset, data));
        }

        protected void SendLStatMessage(string path)
        {
            this.SendMessage(new LStatMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendFStatMessage(byte[] handle)
        {
            this.SendMessage(new FStatMessage(this.SftpSession.NextRequestId, handle));
        }

        protected void SendSetStatMessage(string path, SftpFileAttributes attributes)
        {
            this.SendMessage(new SetStatMessage(this.SftpSession.NextRequestId, path, attributes));
        }

        protected void SendSetStatMessage(byte[] handle, SftpFileAttributes attributes)
        {
            this.SendMessage(new FSetStatMessage(this.SftpSession.NextRequestId, handle, attributes));
        }

        protected void SendOpenDirMessage(string path)
        {
            this.SendMessage(new OpenDirMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendReadDirMessage(byte[] handle)
        {
            this.SendMessage(new ReadDirMessage(this.SftpSession.NextRequestId, handle));
        }

        protected void SendRemoveMessage(string filename)
        {
            this.SendMessage(new RemoveMessage(this.SftpSession.NextRequestId, filename));
        }

        protected void SendMkDirMessage(string path)
        {
            this.SendMessage(new MkDirMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendRmDirMessage(string path)
        {
            this.SendMessage(new RmDirMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendRealPathMessage(string path)
        {
            this.SendMessage(new RealPathMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendStatMessage(string path)
        {
            this.SendMessage(new StatMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendStatMessage(byte[] handle)
        {
            this.SendMessage(new FStatMessage(this.SftpSession.NextRequestId, handle));
        }

        protected void SendRenameMessage(string oldPath, string newPath)
        {
            this.SendMessage(new RenameMessage(this.SftpSession.NextRequestId, oldPath, newPath));
        }

        protected void SendReadLinkMessage(string path)
        {
            this.SendMessage(new ReadLinkMessage(this.SftpSession.NextRequestId, path));
        }

        protected void SendSymLinkMessage(string linkPath, string path)
        {
            this.SendMessage(new SymLinkMessage(this.SftpSession.NextRequestId, linkPath, path));
        }

        protected void CompleteExecution()
        {
            this.SftpSession.AttributesMessageReceived -= SftpSession_AttributesMessageReceived;
            this.SftpSession.DataMessageReceived -= SftpSession_DataMessageReceived;
            this.SftpSession.HandleMessageReceived -= SftpSession_HandleMessageReceived;
            this.SftpSession.NameMessageReceived -= SftpSession_NameMessageReceived;
            this.SftpSession.StatusMessageReceived -= SftpSession_StatusMessageReceived;
            this.SftpSession.ErrorOccured -= SftpSession_ErrorOccured;

            this.AsyncResult.SetAsCompleted(null, false);
        }

        private void SftpSession_StatusMessageReceived(object sender, MessageEventArgs<StatusMessage> e)
        {
            if (this._requestId == e.Message.RequestId)
            {
                this.OnStatus(e.Message.StatusCode, e.Message.ErrorMessage, e.Message.Language);

                //  If status was handled by event handler then exit
                if (this.IsStatusHandled)
                    return;

                if (e.Message.StatusCode == StatusCodes.PermissionDenied)
                {
                    throw new SshPermissionDeniedException(e.Message.ErrorMessage);
                }
                else if (e.Message.StatusCode == StatusCodes.NoSuchFile)
                {
                    throw new SshFileNotFoundException(e.Message.ErrorMessage);
                }
                else if (e.Message.StatusCode == StatusCodes.Failure ||
                 e.Message.StatusCode == StatusCodes.BadMessage ||
                 e.Message.StatusCode == StatusCodes.NoConnection ||
                 e.Message.StatusCode == StatusCodes.ConnectionLost ||
                 e.Message.StatusCode == StatusCodes.OperationUnsupported)
                {
                    //  Throw an exception if it was not handled by the command
                    throw new SshException(e.Message.ErrorMessage);
                }
            }
        }

        private void SftpSession_NameMessageReceived(object sender, MessageEventArgs<NameMessage> e)
        {
            if (this._requestId == e.Message.RequestId)
            {
                this.OnName(e.Message.Files);
            }
        }

        private void SftpSession_HandleMessageReceived(object sender, MessageEventArgs<HandleMessage> e)
        {
            if (this._requestId == e.Message.RequestId)
            {
                this.OnHandle(e.Message.Handle);
            }
        }

        private void SftpSession_DataMessageReceived(object sender, MessageEventArgs<DataMessage> e)
        {
            if (this._requestId == e.Message.RequestId)
            {
                this.OnData(e.Message.Data, e.Message.IsEof);
            }
        }

        private void SftpSession_AttributesMessageReceived(object sender, MessageEventArgs<AttributesMessage> e)
        {
            if (this._requestId == e.Message.RequestId)
            {
                this.OnAttributes(e.Message.Attributes);
            }
        }

        private void SftpSession_ErrorOccured(object sender, ErrorEventArgs e)
        {
            this.AsyncResult.SetAsCompleted(e.GetException(), false);
        }

        private void SendMessage(SftpRequestMessage message)
        {
            //  Remembers command request id that was sent
            this._requestId = message.RequestId;
            
            this.SftpSession.SendMessage(message);
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {

                    this.SftpSession.AttributesMessageReceived -= SftpSession_AttributesMessageReceived;
                    this.SftpSession.DataMessageReceived -= SftpSession_DataMessageReceived;
                    this.SftpSession.HandleMessageReceived -= SftpSession_HandleMessageReceived;
                    this.SftpSession.NameMessageReceived -= SftpSession_NameMessageReceived;
                    this.SftpSession.StatusMessageReceived -= SftpSession_StatusMessageReceived;
                    this.SftpSession.ErrorOccured -= SftpSession_ErrorOccured;
                }

                // Note disposing has been done.
                this._isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="BaseClient"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpCommand()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}
