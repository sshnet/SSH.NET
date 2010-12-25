using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class CreateDirectoryCommand : SftpCommand
    {
        private string _path;

        public CreateDirectoryCommand(SftpSession sftpSession, string path)
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
