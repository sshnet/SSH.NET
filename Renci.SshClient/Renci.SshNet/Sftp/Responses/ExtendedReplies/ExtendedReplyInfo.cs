using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class ExtendedReplyInfo : SshData
    {
        protected override void LoadData()
        {
            //  Read Message Type
            var messageType = this.ReadByte();

            //  Read Response ID
            var responseId = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
