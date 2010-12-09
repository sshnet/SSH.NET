using System.Collections.Generic;

namespace Renci.SshClient.Sftp
{
    internal class RealPathCommand : SftpCommand
    {
        private string _path;

        public IEnumerable<SftpFile> Files { get; private set; }

        public RealPathCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendRealPathMessage(this._path);
        }

        protected override void OnName(System.Collections.Generic.IEnumerable<SftpFile> files)
        {
            base.OnName(files);

            this.Files = files;

            this.CompleteExecution();
        }
    }
}
