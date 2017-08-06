using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpOpenRequestBuilder
    {
        private uint _protocolVersion;
        private uint _requestId;
        private string _fileName;
        private Encoding _encoding;
        private Flags _flags;
        private Action<SftpHandleResponse> _handleAction;
        private Action<SftpStatusResponse> _statusAction;

        public SftpOpenRequestBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpOpenRequestBuilder WithRequestId(uint requestId)
        {
            _requestId = requestId;
            return this;
        }

        public SftpOpenRequestBuilder WithFileName(string fileName)
        {
            _fileName = fileName;
            return this;
        }

        public SftpOpenRequestBuilder WithEncoding(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public SftpOpenRequestBuilder WithFlags(Flags flags)
        {
            _flags = flags;
            return this;
        }

        public SftpOpenRequestBuilder WithDataAction(Action<SftpHandleResponse> handleAction)
        {
            _handleAction = handleAction;
            return this;
        }

        public SftpOpenRequestBuilder WithStatusAction(Action<SftpStatusResponse> statusAction)
        {
            _statusAction = statusAction;
            return this;
        }

        public SftpOpenRequest Build()
        {
            var handleAction = _handleAction ?? ((handleResponse) => { });
            var statusAction = _statusAction ?? ((statusResponse) => { });

            return new SftpOpenRequest(_protocolVersion, _requestId, _fileName, _encoding, _flags, handleAction, statusAction);
        }
    }
}
