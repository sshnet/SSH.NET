using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    internal sealed class SftpOpenAsyncResult : AsyncResult<byte[]>
    {
        public SftpOpenAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }
    }
}
