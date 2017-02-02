using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpFStatRequest : SftpRequest
    {
        private readonly Action<SftpAttrsResponse> _attrsAction;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FStat; }
        }

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

        public SftpFStatRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpAttrsResponse> attrsAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;
            _attrsAction = attrsAction;
        }

        protected override void LoadData()
        {
            base.LoadData();
            Handle = ReadBinary();
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
        }

        public override void Complete(SftpResponse response)
        {
            var attrsResponse = response as SftpAttrsResponse;
            if (attrsResponse != null)
            {
                _attrsAction(attrsResponse);
            }
            else
            {
                base.Complete(response);
            }
        }
    }
}
