using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    internal sealed class SftpRealPathAsyncResult : AsyncResult<string>
    {
        public SftpRealPathAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }
    }
}
