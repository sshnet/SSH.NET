using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Messages;
using System.IO;

namespace Renci.SshNet.Sftp
{
    internal class ReadCommand : SftpCommand
    {
        private byte[] _handle;
        private ulong _offset;
        private MemoryStream _output;

        public byte[] Data { get; private set; }

        public ReadCommand(SftpSession sftpSession, byte[] handle, ulong offset, uint bufferSize)
            : base(sftpSession)
        {
            this._handle = handle;
            this._offset = offset;
            this._output = new MemoryStream((int)bufferSize);
        }

        protected override void OnExecute()
        {
            this.SendReadMessage(this._handle, this._offset, (uint)this._output.Capacity);
        }

        protected override void OnStatus(StatusCodes statusCode, string errorMessage, string language)
        {
            base.OnStatus(statusCode, errorMessage, language);

            if (statusCode == StatusCodes.Eof)
            {
                this.Data = this._output.ToArray();
                this.CompleteExecution();
            }
        }

        protected override void OnData(byte[] data, bool isEof)
        {
            base.OnData(data, isEof);

            this._output.Write(data, 0, data.Length);
            this._output.Flush();
            this._offset += (ulong)data.Length;
            this.AsyncResult.DownloadedBytes = this._offset;

            uint bytesLeft = (uint)(this._output.Capacity - this._output.Position);

            this.SendReadMessage(this._handle, this._offset, bytesLeft);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this._output != null)
                {
                    this._output.Dispose();
                    this._output = null;
                }
            }
        }
    }
}
