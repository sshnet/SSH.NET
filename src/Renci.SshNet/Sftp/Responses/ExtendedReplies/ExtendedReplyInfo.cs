using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class ExtendedReplyInfo : SshData
    {
        protected override void LoadData()
        {
            // skip packet length
            ReadUInt32();

            //  skip message type
            ReadByte();

            //  skip response id
            ReadUInt32();
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
