using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpReadRequestBuilder
    {
        private uint _protocolVersion;
        private uint _requestId;
        private byte[] _handle;
        private uint _offset;
        private uint _length;
        private Action<SftpDataResponse> _dataAction;
        private Action<SftpStatusResponse> _statusAction;

        public SftpReadRequestBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpReadRequestBuilder WithRequestId(uint requestId)
        {
            _requestId = requestId;
            return this;
        }

        public SftpReadRequestBuilder WithHandle(byte[] handle)
        {
            _handle = handle;
            return this;
        }

        public SftpReadRequestBuilder WithOffset(uint offset)
        {
            _offset = offset;
            return this;
        }

        public SftpReadRequestBuilder WithLength(uint length)
        {
            _length = length;
            return this;
        }

        public SftpReadRequestBuilder WithDataAction(Action<SftpDataResponse> dataAction)
        {
            _dataAction = dataAction;
            return this;
        }

        public SftpReadRequestBuilder WithStatusAction(Action<SftpStatusResponse> statusAction)
        {
            _statusAction = statusAction;
            return this;
        }

        public SftpReadRequest Build()
        {
            var dataAction = _dataAction ?? ((dataResponse) => { });
            var statusAction = _statusAction ?? ((statusResponse) => { });

            return new SftpReadRequest(_protocolVersion, _requestId, _handle, _offset, _length, dataAction, statusAction);
        }
    }
}
