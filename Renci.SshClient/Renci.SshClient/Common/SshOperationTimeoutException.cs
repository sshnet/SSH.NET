using System;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshOperationTimeoutException : SshException
    {
        public SshOperationTimeoutException(string message)
            : base(message)
        {

        }
    }
}
