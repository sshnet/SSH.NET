using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class RemoveFileCommand : SftpCommand
    {
        private string _path;

        public RemoveFileCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendRemoveMessage(this._path);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.Ok)
            {
                this.CompleteExecution();
            }
        }
    }
}
