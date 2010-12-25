using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Renci.SshClient.Common;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    /// <summary>
    /// Base class for all SFTP Commands
    /// </summary>
    internal abstract class SftpCommand
    {
        private SftpSession _sftpSession;

        private AsyncCallback _callback;

        private uint _requestId;

        private Exception _error;

        private bool _handleCloseMessageSent;

        protected SftpAsyncResult AsyncResult { get; private set; }

        public int CommandTimeout { get; set; }

        public SftpCommand(SftpSession sftpSession)
        {
            this._sftpSession = sftpSession;
            this._sftpSession.AttributesMessageReceived += SftpSession_AttributesMessageReceived;
            this._sftpSession.DataMessageReceived += SftpSession_DataMessageReceived;
            this._sftpSession.HandleMessageReceived += SftpSession_HandleMessageReceived;
            this._sftpSession.NameMessageReceived += SftpSession_NameMessageReceived;
            this._sftpSession.StatusMessageReceived += SftpSession_StatusMessageReceived;
            this._sftpSession.ErrorOccured += SftpSession_ErrorOccured;
        }

        public SftpAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            this._callback = callback;

            this.AsyncResult = new SftpAsyncResult(this, state);

            this.OnExecute();

            return this.AsyncResult;
        }

        public void EndExecute(SftpAsyncResult result)
        {
            this._sftpSession.WaitHandle(result.AsyncWaitHandle, this.CommandTimeout);

            if (this._callback != null)
            {
                //  Execute callback on new pool thread
                Task.Factory.StartNew(() =>
                {
                    this._callback(result);
                });
            }

            if (this._error != null)
                throw this._error;

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

        protected virtual void OnName(IEnumerable<SftpFile> files)
        {
        }

        protected virtual void OnHandle(string handle)
        {
        }

        protected virtual void OnData(string data, bool isEof)
        {
        }

        protected virtual void OnAttributes(Attributes attributes)
        {
        }

        protected virtual void OnHandleClosed()
        {
        }

        protected void SendOpenMessage(string path, Flags flags)
        {
            this.SendMessage(new OpenMessage
            {
                Filename = path,
                Flags = flags,
            });
        }

        protected void SendCloseMessage(string handle)
        {
            this.SendMessage(new CloseMessage
            {
                Handle = handle,
            });

            this._handleCloseMessageSent = true;
        }

        protected void SendReadMessage(string handle, ulong offset, uint bufferSize)
        {
            this.SendMessage(new ReadMessage
            {
                Handle = handle,
                Offset = offset,
                Length = bufferSize,
            });
        }

        protected void SendWriteMessage(string handle, ulong offset, string data)
        {
            this.SendMessage(new WriteMessage
            {
                Handle = handle,
                Offset = offset,
                Data = data,
            });
        }

        protected void SendLStatMessage(string path)
        {
            this.SendMessage(new LStatMessage
            {
                Path = path,
            });
        }

        protected void SendFStatMessage(string handle)
        {
            this.SendMessage(new FStatMessage
            {
                Handle = handle,
            });
        }

        protected void SendSetStatMessage(string path, Attributes attributes)
        {
            this.SendMessage(new SetStatMessage
            {
                Path = path,
                Attributes = attributes,
            });
        }

        protected void SendFSetStatMessage(string handle, Attributes attributes)
        {
            this.SendMessage(new FSetStatMessage
            {
                Handle = handle,
                Attributes = attributes,
            });
        }

        protected void SendOpenDirMessage(string path)
        {
            this.SendMessage(new OpenDirMessage
            {
                Path = path,
            });
        }

        protected void SendReadDirMessage(string handle)
        {
            this.SendMessage(new ReadDirMessage
            {
                Handle = handle,
            });
        }

        protected void SendRemoveMessage(string filename)
        {
            this.SendMessage(new RemoveMessage
            {
                Filename = filename,
            });
        }

        protected void SendMkDirMessage(string path)
        {
            this.SendMessage(new MkDirMessage
            {
                Path = path,
            });
        }

        protected void SendRmDirMessage(string path)
        {
            this.SendMessage(new RmDirMessage
            {
                Path = path,
            });
        }

        protected void SendRealPathMessage(string path)
        {
            this.SendMessage(new RealPathMessage
            {
                Path = path,
            });
        }

        protected void SendStatMessage(string path)
        {
            this.SendMessage(new StatMessage
            {
                Path = path,
            });
        }

        protected void SendRenameMessage(string oldPath, string newPath)
        {
            this.SendMessage(new RenameMessage
            {
                OldPath = oldPath,
                NewPath = newPath,
            });
        }

        protected void SendReadLinkMessage(string path)
        {
            this.SendMessage(new ReadLinkMessage
            {
                Path = path,
            });
        }

        protected void SendSymLinkMessage(string existingPath, string newLinkPath, bool isSymLink)
        {
            this.SendMessage(new SymLinkMessage
            {
                ExistingPath = existingPath,
                NewLinkPath = newLinkPath,
                IsSymLink = isSymLink,
            });
        }

        protected void CompleteExecution()
        {
            this.AsyncResult.Complete();

            this._sftpSession.AttributesMessageReceived -= SftpSession_AttributesMessageReceived;
            this._sftpSession.DataMessageReceived -= SftpSession_DataMessageReceived;
            this._sftpSession.HandleMessageReceived -= SftpSession_HandleMessageReceived;
            this._sftpSession.NameMessageReceived -= SftpSession_NameMessageReceived;
            this._sftpSession.StatusMessageReceived -= SftpSession_StatusMessageReceived;
        }

        private void SftpSession_StatusMessageReceived(object sender, MessageEventArgs<StatusMessage> e)
        {
            if (this._requestId == e.Message.RequestId)
            {
                this.OnStatus(e.Message.StatusCode, e.Message.ErrorMessage, e.Message.Language);

                if (e.Message.StatusCode == StatusCodes.NoSuchFile ||
                    e.Message.StatusCode == StatusCodes.PermissionDenied ||
                    e.Message.StatusCode == StatusCodes.Failure ||
                    e.Message.StatusCode == StatusCodes.BadMessage ||
                    e.Message.StatusCode == StatusCodes.NoConnection ||
                    e.Message.StatusCode == StatusCodes.ConnectionLost ||
                    e.Message.StatusCode == StatusCodes.OperationUnsupported
                    )
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
            this._error = e.GetException();

            this.CompleteExecution();
        }

        private void SendMessage(SftpRequestMessage message)
        {
            this._sftpSession.SendMessage(message);

            //  Remeber command request id that was sent
            this._requestId = message.RequestId;
        }
    }
}
