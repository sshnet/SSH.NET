using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Messages;

namespace Renci.SshNet.Sftp
{
    internal class FileExistsCommand : SftpCommand
    {
        private string _path;

        private Flags _flags;

        public bool Exists { get; private set; }

        public FileExistsCommand(SftpSession sftpSession, string path, Flags flags)
            : base(sftpSession)
        {
            this._path = path;
            this._flags = flags;
        }

        protected override void OnExecute()
        {
            this.SendOpenMessage(this._path, this._flags);
        }

        protected override void OnHandle(byte[] handle)
        {
            base.OnHandle(handle);

            this.Exists = true;

            this.SendCloseMessage(handle);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.NoSuchFile)
            {
                this.Exists = false;

                this.IsStatusHandled = true;

                this.CompleteExecution();
            }
            else if (statusCode == StatusCodes.Ok)
            {
                this.CompleteExecution();
            }

        }
    }
}
