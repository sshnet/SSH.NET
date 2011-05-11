using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class FileStatusCommand : SftpCommand
    {
        private string _path;

        public SftpFile SftpFile { get; private set; }

        public FileStatusCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendStatMessage(this._path);
        }

        protected override void OnAttributes(Attributes attributes)
        {
            base.OnAttributes(attributes);

            this.SftpFile = new SftpFile(this._path, attributes);

            this.CompleteExecution();
        }
    }
}
