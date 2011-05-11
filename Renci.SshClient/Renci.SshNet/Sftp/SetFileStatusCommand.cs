using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Messages;

namespace Renci.SshNet.Sftp
{
    internal class SetFileStatusCommand : SftpCommand
    {
        private string _path;

        private SftpFileAttributes _attributes;

        public SftpFile SftpFile { get; private set; }

        public SetFileStatusCommand(SftpSession sftpSession, string path, SftpFileAttributes attributes)
            : base(sftpSession)
        {
            this._path = path;

            this._attributes = attributes;
        }

        protected override void OnExecute()
        {
            this.SendSetStatMessage(this._path, this._attributes);
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
