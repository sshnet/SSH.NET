using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp
{
    internal class SftpOpenAsyncResult : AsyncResult<byte[]>
    {
        public SftpOpenAsyncResult(AsyncCallback asyncCallback, object state) : base(asyncCallback, state)
        {
        }
    }
}
