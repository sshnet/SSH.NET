using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Messages;
using Renci.SshNet.Common;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
    internal class OpenCommand : SftpCommand
    {
        private string _path;

        private Flags _flags;

        public byte[] Handle { get; private set; }

        public OpenCommand(SftpSession sftpSession, string path, Flags flags)
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
