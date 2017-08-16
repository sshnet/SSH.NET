using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpHandleResponseBuilder
    {
        private uint _protocolVersion;
        private uint _responseId;
        private byte[] _handle;

        public SftpHandleResponseBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpHandleResponseBuilder WithResponseId(uint responseId)
        {
            _responseId = responseId;
            return this;
        }

        public SftpHandleResponseBuilder WithHandle(byte[] handle)
        {
            _handle = handle;
            return this;
        }

        public SftpHandleResponse Build()
        {
            var sftpHandleResponse = new SftpHandleResponse(_protocolVersion)
            {
                ResponseId = _responseId,
                Handle = _handle
            };
            return sftpHandleResponse;
        }
    }
}
