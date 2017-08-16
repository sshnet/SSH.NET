using System.Collections.Generic;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpVersionResponseBuilder
    {
        private uint _version;
        private IDictionary<string, string> _extensions;

        public SftpVersionResponseBuilder()
        {
            _extensions = new Dictionary<string, string>();
        }

        public SftpVersionResponseBuilder WithVersion(uint version)
        {
            _version = version;
            return this;
        }

        public SftpVersionResponseBuilder WithExtension(string name, string data)
        {
            _extensions.Add(name, data);
            return this;
        }

        public SftpVersionResponse Build()
        {
            var sftpVersionResponse = new SftpVersionResponse()
                {
                    Version = _version,
                    Extentions = _extensions
                };
            return sftpVersionResponse;
        }
    }
}
