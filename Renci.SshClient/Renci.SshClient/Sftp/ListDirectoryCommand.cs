using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Common;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class ListDirectoryCommand : SftpCommand
    {
        private string _path;

        private byte[] _handle;

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

        protected override void OnHandle(byte[] handle)
        {
            base.OnHandle(handle);

            this._handle = handle;

            this.SendReadDirMessage(this._handle);
        }

        protected override void OnName(IDictionary<string, SftpFileAttributes> files)
        {
            base.OnName(files);

            var seperator = "/";
            if (this._path.EndsWith(seperator))
                seperator = string.Empty;

            var sftpFiles = from f in files
                            select new SftpFile(this.SftpSession, string.Format("{0}{1}{2}", this._path, seperator, f.Key), f.Value);

            this._files.AddRange(sftpFiles);

            this.SendReadDirMessage(this._handle);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.NoSuchFile)
            {
                throw new SshFileNotFoundException(string.Format("Path '{0}' is not found.", this._path));
            }

            if (statusCode == StatusCodes.Eof)
            {
                this.CompleteExecution();

                this.SendCloseMessage(this._handle);
            }
        }
    }
}
