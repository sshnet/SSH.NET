using System;

using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal abstract class SftpRequest : SftpMessage
    {
        private readonly Action<SftpStatusResponse> _statusAction;

        public uint RequestId { get; }

        public uint ProtocolVersion { get; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // RequestId
                return capacity;
            }
        }

        protected SftpRequest(uint protocolVersion, uint requestId, Action<SftpStatusResponse> statusAction)
        {
            RequestId = requestId;
            ProtocolVersion = protocolVersion;
            _statusAction = statusAction;
        }

        public virtual void Complete(SftpResponse response)
        {
            if (response is SftpStatusResponse statusResponse)
            {
                _statusAction(statusResponse);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Response of type '{0}' is not expected.", response.GetType().Name));
            }
        }

        protected override void LoadData()
        {
            throw new InvalidOperationException("Request cannot be saved.");
        }

        protected override void SaveData()
        {
            base.SaveData();

            Write(RequestId);
        }
    }
}
