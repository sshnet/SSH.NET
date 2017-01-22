using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp
{
    internal class SftpReadAsyncResult : AsyncResult<byte[]>
    {
        public SftpReadAsyncResult(AsyncCallback asyncCallback, object state) : base(asyncCallback, state)
        {
        }
    }
}
