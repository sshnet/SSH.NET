using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpStatVfsRequestBuilder
    {
        private uint _protocolVersion;
        private uint _requestId;
        private string _path;
        private Encoding _encoding;
        private Action<SftpExtendedReplyResponse> _extendedAction;
        private Action<SftpStatusResponse> _statusAction;

        public SftpStatVfsRequestBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpStatVfsRequestBuilder WithRequestId(uint requestId)
        {
            _requestId = requestId;
            return this;
        }

        public SftpStatVfsRequestBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public SftpStatVfsRequestBuilder WithEncoding(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public SftpStatVfsRequestBuilder WithExtendedAction(Action<SftpExtendedReplyResponse> extendedAction)
        {
            _extendedAction = extendedAction;
            return this;
        }
        
        public SftpStatVfsRequestBuilder WithStatusAction(Action<SftpStatusResponse> statusAction)
        {
            _statusAction = statusAction;
            return this;
        }

        public StatVfsRequest Build()
        {
            var extendedAction = _extendedAction ?? ((extendedReplyResponse) => { });
            var statusAction = _statusAction ?? ((statusResponse) => { });

            return new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, extendedAction, statusAction);
        }
    }
}
