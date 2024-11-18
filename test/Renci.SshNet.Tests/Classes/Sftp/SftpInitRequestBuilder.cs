using Renci.SshNet.Sftp.Requests;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpInitRequestBuilder
    {
        private uint _version;

        public SftpInitRequestBuilder WithVersion(uint version)
        {
            _version = version;
            return this;
        }

        public SftpInitRequest Build()
        {
            return new SftpInitRequest(_version);
        }
    }
}
