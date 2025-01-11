using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    internal sealed class SftpCloseAsyncResult : AsyncResult
    {
        public SftpCloseAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }
    }
}
