using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class SetFileStatusCommand : SftpCommand
    {
        private string _path;

        private Attributes _attributes;

        public SftpFile SftpFile { get; private set; }

        public SetFileStatusCommand(SftpSession sftpSession, string path, Attributes attributes)
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
            if (statusCode == StatusCodes.Ok)
            {
                this.CompleteExecution();
            }
        }
    }
}
