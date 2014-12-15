using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class ExtendedReplyInfo : SshData
    {
        protected override void LoadData()
        {
#if TUNING
            // skip packet length
            this.ReadUInt32();
#endif

            //  skip message type
            this.ReadByte();

            //  skip response id
            this.ReadUInt32();
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
