using System.IO;
using System.Linq;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class UploadFileCommand : SftpCommand
    {
        private string _path;
        private Stream _input;
        private string _handle;
        private byte[] _buffer;
        private ulong _offset;
        private bool _hasMoreData;

        public UploadFileCommand(SftpSession sftpSession, uint bufferSize, string path, Stream input)
            : base(sftpSession)
        {
            this._path = path;
            this._input = input;
            this._buffer = new byte[bufferSize];
        }

        protected override void OnExecute()
        {
            this.SendOpenMessage(this._path, Flags.Write | Flags.CreateNewOrOpen | Flags.Truncate);
        }

        protected override void OnHandle(string handle)
        {
            base.OnHandle(handle);

            this._handle = handle;

            this.SendData();
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.Ok)
            {
                if (this._hasMoreData)
                {
                    this.SendData();
                }
            }
        }

        protected override void OnHandleClosed()
        {
            base.OnHandleClosed();

            this.CompleteExecution();
        }

        private void SendData()
        {
            var bytesRead = this._input.Read(this._buffer, 0, this._buffer.Length);

            this._hasMoreData = (bytesRead > 0);

            if (this._hasMoreData)
            {
                this.SendWriteMessage(this._handle, this._offset, this._buffer.Take(bytesRead).GetSshString());

                this._offset += (ulong)bytesRead;

                this.AsyncResult.UploadedBytes = this._offset;
            }
            else
            {
                this.SendCloseMessage(this._handle);
            }
        }
    }
}
