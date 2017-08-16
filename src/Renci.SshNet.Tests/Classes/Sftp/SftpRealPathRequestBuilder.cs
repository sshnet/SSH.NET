using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpRealPathRequestBuilder
    {
        private uint _protocolVersion;
        private uint _requestId;
        private string _path;
        private Encoding _encoding;
        private Action<SftpNameResponse> _nameAction;
        private Action<SftpStatusResponse> _statusAction;

        public SftpRealPathRequestBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpRealPathRequestBuilder WithRequestId(uint requestId)
        {
            _requestId = requestId;
            return this;
        }

        public SftpRealPathRequestBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public SftpRealPathRequestBuilder WithEncoding(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public SftpRealPathRequestBuilder WithNameAction(Action<SftpNameResponse> nameAction)
        {
            _nameAction = nameAction;
            return this;
        }

        public SftpRealPathRequestBuilder WithStatusAction(Action<SftpStatusResponse> statusAction)
        {
            _statusAction = statusAction;
            return this;
        }

        public SftpRealPathRequest Build()
        {
            var nameAction = _nameAction ?? ((nameResponse) => { });
            var statusAction = _statusAction ?? ((statusResponse) => { });

            return new SftpRealPathRequest(_protocolVersion, _requestId, _path, _encoding, nameAction, statusAction);
        }
    }
}
