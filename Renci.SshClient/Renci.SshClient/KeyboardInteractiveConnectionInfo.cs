using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient
{
    public class KeyboardInteractiveConnectionInfo : ConnectionInfo
    {
        public KeyboardInteractiveConnectionInfo(string host, string username)
            : this(host, 22, username)
        {

        }

        public KeyboardInteractiveConnectionInfo(string host, int port, string username)
            : base(host, port, username)
        {
        }

    }
}
