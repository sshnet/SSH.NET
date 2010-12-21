using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshAuthenticationException : SshException
    {
        public SshAuthenticationException(string message)
            : base(message)
        {

        }
    }
}
