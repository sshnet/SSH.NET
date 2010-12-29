using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    internal class SymbolicLinkCommand : SftpCommand
    {
        private string _linkPath;

        private string _path;

        public SymbolicLinkCommand(SftpSession sftpSession, string linkPath, string path)
            : base(sftpSession)
        {
            this._linkPath = linkPath;

            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendSymLinkMessage(this._linkPath, this._path);
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
