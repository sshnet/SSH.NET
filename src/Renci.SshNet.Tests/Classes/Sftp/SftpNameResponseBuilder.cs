using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpNameResponseBuilder
    {
        private uint _responseId;
        private uint _protocolVersion;
        private Encoding _encoding;
        private List<KeyValuePair<string, SftpFileAttributes>> _files;

        public SftpNameResponseBuilder()
        {
            _files = new List<KeyValuePair<string, SftpFileAttributes>>();
        }

        public SftpNameResponseBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpNameResponseBuilder WithResponseId(uint responseId)
        {
            _responseId = responseId;
            return this;
        }

        public SftpNameResponseBuilder WithFiles(params KeyValuePair<string, SftpFileAttributes>[] files)
        {
            for (var i = 0; i < files.Length; i++)
                _files.Add(files[i]);
            return this;
        }

        public SftpNameResponseBuilder WithFile(string filename, SftpFileAttributes attributes)
        {
            _files.Add(new KeyValuePair<string, SftpFileAttributes>(filename, attributes));
            return this;
        }

        public SftpNameResponseBuilder WithEncoding(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public SftpNameResponse Build()
        {
            var sftpNameResponse = new SftpNameResponse(_protocolVersion, _encoding)
                {
                    ResponseId = _responseId,
                    Files = _files.ToArray()
                };
            return sftpNameResponse;
        }
    }
}
