using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Sftp.Messages;

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

        protected override void OnName(IDictionary<string, SftpFileAttributes> files)
        {
            base.OnName(files);

            this.Files = from f in files
                         select new SftpFile(this.SftpSession, f.Key, f.Value);

            this.CompleteExecution();
        }
    }
}
