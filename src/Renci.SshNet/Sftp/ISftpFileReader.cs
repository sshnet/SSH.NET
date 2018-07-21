using System;

namespace Renci.SshNet.Sftp
{
    internal interface ISftpFileReader : IDisposable
    {
        byte[] Read();
    }
}
