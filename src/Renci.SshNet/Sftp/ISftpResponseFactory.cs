using System.Text;

namespace Renci.SshNet.Sftp
{
    internal interface ISftpResponseFactory
    {
        SftpMessage Create(uint protocolVersion, byte messageType, Encoding encoding);
    }
}
