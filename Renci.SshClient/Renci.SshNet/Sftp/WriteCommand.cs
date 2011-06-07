using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Messages;

namespace Renci.SshNet.Sftp
{
    internal class WriteCommand : SftpCommand
    {
        private byte[] _handle;
        private ulong _offset;
        private byte[] _data;

        public WriteCommand(SftpSession sftpSession, byte[] handle, ulong offset, byte[] data)
            : base(sftpSession)
        {
            this._handle = handle;
            this._offset = offset;
            this._data = data;
        }

        protected override void OnExecute()
        {
            this.SendWriteMessage(this._handle, this._offset, this._data);
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
