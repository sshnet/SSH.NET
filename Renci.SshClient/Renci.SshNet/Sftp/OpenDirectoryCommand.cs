using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Messages;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
    internal class OpenDirectoryCommand : SftpCommand
    {
        private string _path;

        public byte[] Handle { get; private set; }

        public OpenDirectoryCommand(SftpSession sftpSession, string path)
            : base(sftpSession)
        {
            this._path = path;
        }

        protected override void OnExecute()
        {
            this.SendOpenDirMessage(this._path);
        }

        protected override void OnHandle(byte[] handle)
        {
            base.OnHandle(handle);

            this.Handle = handle;

            this.CompleteExecution();
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.NoSuchFile)
            {
                throw new SftpPathNotFoundException(string.Format(CultureInfo.CurrentCulture, "Path '{0}' is not found.", this._path));
            }
        }
    }
}
