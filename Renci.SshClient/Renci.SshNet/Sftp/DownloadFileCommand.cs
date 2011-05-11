using System.IO;
using System.Linq;
using Renci.SshNet.Sftp.Messages;

namespace Renci.SshNet.Sftp
{
    internal class DownloadFileCommand : SftpCommand
    {
        private string _path;
        private Stream _output;
        private byte[] _handle;
        private ulong _offset = 0;
        private uint _bufferSize = 1024 * 16;
        private bool _closing = false;

        public DownloadFileCommand(SftpSession sftpSession, uint bufferSize, string path, Stream output)
            : base(sftpSession)
        {
            this._path = path;
            this._output = output;
            this._bufferSize = bufferSize;
        }

        protected override void OnExecute()
        {
            this.SendOpenMessage(this._path, Flags.Read);
        }

        protected override void OnHandle(byte[] handle)
        {
            base.OnHandle(handle);

            this._handle = handle;

            this.SendReadMessage(this._handle, this._offset, this._bufferSize);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.Eof)
            {
                this.SendCloseMessage(this._handle);
                this._closing = true;
            }
            else if (statusCode == StatusCodes.Ok && this._closing)
            {
                this.CompleteExecution();
            }
        }

        protected override void OnData(byte[] data, bool isEof)
        {
            base.OnData(data, isEof);

            this._output.Write(data, 0, data.Length);
            this._output.Flush();
            this._offset += (ulong)data.Length;
            this.AsyncResult.DownloadedBytes = this._offset;

            this.SendReadMessage(this._handle, this._offset, this._bufferSize);
        }
    }
}
