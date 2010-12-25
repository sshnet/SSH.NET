using System.IO;
using System.Linq;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class DownloadFileCommand : SftpCommand
    {
        private string _path;
        private Stream _output;
        private string _handle;
        private ulong _offset = 0;
        private uint _bufferSize = 1024;

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

        protected override void OnHandle(string handle)
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
                this.CompleteExecution();
            }
        }

        protected override void OnData(string data, bool isEof)
        {
            base.OnData(data, isEof);

            var fileData = data.GetSshBytes().ToArray();
            this._output.Write(fileData, 0, fileData.Length);
            this._output.Flush();
            this._offset += (ulong)fileData.Length;
            this.AsyncResult.DownloadedBytes = this._offset;

            this.SendReadMessage(this._handle, this._offset, this._bufferSize);
        }
    }
}
