using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Common
{
    public abstract class AuthenticationEventArgs : EventArgs
    {
        public string Username { get; private set; }

        public AuthenticationEventArgs(string username)
        {
            this.Username = username;
        }
    }
}
