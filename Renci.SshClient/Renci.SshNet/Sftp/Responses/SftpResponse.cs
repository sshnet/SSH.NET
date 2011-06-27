using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class SftpResponse : SftpMessage
    {
        public uint ResponseId { get; private set; }

        protected override void LoadData()
        {
            base.LoadData();
            
            this.ResponseId = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            throw new InvalidOperationException("Response cannot be saved.");
        }
    }
}
