using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    internal sealed class SFtpStatAsyncResult : AsyncResult<SftpFileAttributes>
    {
        public SFtpStatAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }
    }
}
