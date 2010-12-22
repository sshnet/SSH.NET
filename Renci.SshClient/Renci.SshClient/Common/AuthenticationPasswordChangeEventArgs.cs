using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    public class AuthenticationPasswordChangeEventArgs : AuthenticationEventArgs
    {
        public string NewPassword { get; set; }

        public AuthenticationPasswordChangeEventArgs(string username)
            : base(username)
        {
        }
    }
}
