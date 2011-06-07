using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Messages;

namespace Renci.SshNet.Sftp
{
    internal class CloseCommand : SftpCommand
    {
        private byte[] _handle;

        public CloseCommand(SftpSession sftpSession, byte[] handle)
            : base(sftpSession)
        {
            this._handle = handle;
        }

        protected override void OnExecute()
        {
            this.SendCloseMessage(this._handle);
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
