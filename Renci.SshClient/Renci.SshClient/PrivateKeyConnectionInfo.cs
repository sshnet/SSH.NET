using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Renci.SshClient
{
    public class PrivateKeyConnectionInfo : ConnectionInfo
    {
        public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

        public PrivateKeyConnectionInfo(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, 22, username, keyFiles)
        {

        }

        public PrivateKeyConnectionInfo(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : base(host, port, username)
        {
            this.KeyFiles = new Collection<PrivateKeyFile>(keyFiles);
        }

    }
}
