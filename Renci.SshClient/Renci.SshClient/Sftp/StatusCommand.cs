using System.Collections.Generic;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class StatusCommand : SftpCommand
    {
        private string _path;

        public SftpFile File { get; private set; }

        public StatusCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendStatMessage(this._path);
        }

        protected override void OnAttributes(SftpFileAttributes attributes)
        {
            base.OnAttributes(attributes);

            this.File = new SftpFile(this.SftpSession, this._path, attributes);

            this.CompleteExecution();
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.NoSuchFile)
            {
                this.CompleteExecution();

                this.IsStatusHandled = true;
            }
        }
    }
}
