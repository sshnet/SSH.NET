using System;
using System.Collections.Generic;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient.Sftp
{
    internal abstract class SftpCommand
    {
        private SftpSession _sftpSession;

        private uint _requestId;

        private SftpAsyncResult _asyncResult;

        public SftpCommand(SftpSession sftpSession)
        {
            this._sftpSession = sftpSession;
            this._sftpSession.AttributesMessageReceived += SftpSession_AttributesMessageReceived;
            this._sftpSession.DataMessageReceived += SftpSession_DataMessageReceived;
            this._sftpSession.HandleMessageReceived += SftpSession_HandleMessageReceived;
            this._sftpSession.NameMessageReceived += SftpSession_NameMessageReceived;
            this._sftpSession.StatusMessageReceived += SftpSession_StatusMessageReceived;
        }

        public SftpAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            this._asyncResult = new SftpAsyncResult(this, callback, state);

            this.OnExecute();

            return this._asyncResult;
        }

        public void EndExecute(SftpAsyncResult result)
        {
            //  TODO:   Add timeout info here
            this._sftpSession.WaitHandle(result.AsyncWaitHandle);
        }

        public void Execute()
        {
            this.EndExecute(this.BeginExecute(null, null));
        }

        protected abstract void OnExecute();

        protected virtual void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
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
            this._asyncResult.IsCompleted = true;

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
            }

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

        private void SendMessage(SftpRequestMessage message)
        {
            this._sftpSession.SendMessage(message);

            //  Remeber command request id that was sent
            this._requestId = message.RequestId;
        }
    }
}
