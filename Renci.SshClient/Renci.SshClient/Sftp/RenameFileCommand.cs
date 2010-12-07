using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient.Sftp
{
    internal class RenameFileCommand : SftpCommand
    {
        private string _oldPath;
        private string _newPath;

        public RenameFileCommand(SftpSession sftpSession, string oldPath, string newPath)
            : base(sftpSession)
        {
            this._oldPath = oldPath;
            this._newPath = newPath;
        }

        protected override void OnExecute()
        {
            this.SendRenameMessage(this._oldPath, this._newPath);
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
