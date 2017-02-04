using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp
{
    internal class SftpOpenDirAsyncResult : AsyncResult<byte[]>
    {
        public SftpOpenDirAsyncResult(AsyncCallback asyncCallback, object state) : base(asyncCallback, state)
        {
        }
    }
}
