using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient
{
    public class PasswordConnectionInfo : ConnectionInfo
    {
        public string Password { get; private set; }

        public PasswordConnectionInfo(string host, string username, string password)
            : this(host, 22, username, password)
        {

        }

        public PasswordConnectionInfo(string host, int port, string username, string password)
            : base(host, port, username)
        {
            this.Password = password;
        }

    }
}
