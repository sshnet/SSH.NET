using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class FStatVfsRequest : SftpExtendedRequest
    {
        private readonly Action<SftpExtendedReplyResponse> _extendedReplyAction;

        public byte[] Handle { get; private set; }

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
                capacity += 4; // Handle length
                capacity += Handle.Length; // Handle
                return capacity;
            }
        }

        public FStatVfsRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpExtendedReplyResponse> extendedAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction, "fstatvfs@openssh.com")
        {
            Handle = handle;

            _extendedReplyAction = extendedAction;
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
        }

        public override void Complete(SftpResponse response)
        {
            var extendedReplyResponse = response as SftpExtendedReplyResponse;
            if (extendedReplyResponse != null)
            {
                _extendedReplyAction(extendedReplyResponse);
            }
            else
            {
                base.Complete(response);
            }
        }
    }
}
