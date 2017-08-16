using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpReadDirRequest : SftpRequest
    {
        private readonly Action<SftpNameResponse> _nameAction;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ReadDir; }
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

        public SftpReadDirRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpNameResponse> nameAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;

            _nameAction = nameAction;
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
            var nameResponse = response as SftpNameResponse;
            if (nameResponse != null)
            {
                _nameAction(nameResponse);
            }
            else
            {
                base.Complete(response);
            }
        }
    }
}
