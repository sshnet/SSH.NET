using System.Collections.Generic;
using System.Linq;
using Renci.SshNet.Sftp.Messages;
using System.Threading;

namespace Renci.SshNet.Sftp
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
