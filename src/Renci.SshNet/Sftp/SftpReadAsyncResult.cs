using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    internal sealed class SftpReadAsyncResult : AsyncResult<byte[]>
    {
        public SftpReadAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }
    }
}
