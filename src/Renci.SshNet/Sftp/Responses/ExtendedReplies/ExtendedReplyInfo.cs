using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class ExtendedReplyInfo
    {
        public abstract void LoadData(SshDataStream stream);
    }
}
