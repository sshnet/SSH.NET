using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using System;

namespace Renci.SshNet.Common
{
    internal class SshSignatureData : SshData
    {
        private readonly RequestMessagePublicKey _message;

        private readonly byte[] _sessionId;
        private readonly byte[] _serviceName;
        private readonly byte[] _authenticationMethod;

        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // SessionId length
                capacity += _sessionId.Length; // SessionId
                capacity += 1; // Authentication Message Code
                capacity += 4; // UserName length
                capacity += _message.Username.Length; // UserName
                capacity += 4; // ServiceName length
                capacity += _serviceName.Length; // ServiceName
                capacity += 4; // AuthenticationMethod length
                capacity += _authenticationMethod.Length; // AuthenticationMethod
                capacity += 1; // TRUE
                capacity += 4; // PublicKeyAlgorithmName length
                capacity += _message.PublicKeyAlgorithmName.Length; // PublicKeyAlgorithmName
                capacity += 4; // PublicKeyData length
                capacity += _message.PublicKeyData.Length; // PublicKeyData
                return capacity;
            }
        }

        public SshSignatureData(RequestMessagePublicKey message, byte[] sessionId)
        {
            _message = message;
            _sessionId = sessionId;
            _serviceName = ServiceName.Connection.ToArray();
            _authenticationMethod = Ascii.GetBytes("publickey");
        }

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
            WriteBinaryString(_sessionId);
            Write((byte)RequestMessage.AuthenticationMessageCode);
            WriteBinaryString(_message.Username);
            WriteBinaryString(_serviceName);
            WriteBinaryString(_authenticationMethod);
            Write((byte)1); // TRUE
            WriteBinaryString(_message.PublicKeyAlgorithmName);
            WriteBinaryString(_message.PublicKeyData);
        }
    }
}
