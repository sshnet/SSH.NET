using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient.Sftp
{
    internal class CreateRemoteDirectoryCommand : SftpCommand
    {
        private string _path;

        public CreateRemoteDirectoryCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendMkDirMessage(this._path);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            if (statusCode == StatusCodes.Ok)
            {
                this.CompleteExecution();
            }
        }
    }
}
