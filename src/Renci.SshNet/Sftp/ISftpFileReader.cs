using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Sftp
{
    internal interface ISftpFileReader : IDisposable
    {
        byte[] Read();
    }
}
