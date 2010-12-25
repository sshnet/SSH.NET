using System.Collections.Generic;
using Renci.SshClient.Common;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class ListDirectoryCommand : SftpCommand
    {
        private string _path;

        private string _handle;

        private List<SftpFile> _files = new List<SftpFile>();

        public IEnumerable<SftpFile> Files
        {
            get
            {
                return this._files.AsReadOnly();
            }
        }

        public ListDirectoryCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendOpenDirMessage(this._path);
        }

        protected override void OnHandle(string handle)
        {
            base.OnHandle(handle);

            this._handle = handle;

            this.SendReadDirMessage(this._handle);
        }

        protected override void OnName(IEnumerable<SftpFile> files)
        {
            base.OnName(files);

            this._files.AddRange(files);

            this.SendReadDirMessage(this._handle);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.NoSuchFile)
            {
                throw new SshException(string.Format("Path '{0}' is not found.", this._path));
            }

            if (statusCode == StatusCodes.Eof)
            {
                this.CompleteExecution();

                this.SendCloseMessage(this._handle);
            }
        }
    }
}
