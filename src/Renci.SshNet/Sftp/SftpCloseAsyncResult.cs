using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp
{
    internal class SftpCloseAsyncResult : AsyncResult
    {
        public SftpCloseAsyncResult(AsyncCallback asyncCallback, object state) : base(asyncCallback, state)
        {
        }
    }
}
