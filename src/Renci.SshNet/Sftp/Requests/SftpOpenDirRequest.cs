using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpOpenDirRequest : SftpRequest
    {
        private byte[] _path;
        private readonly Action<SftpHandleResponse> _handleAction;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.OpenDir; }
        }

        public string Path
        {
            get { return Encoding.GetString(_path, 0, _path.Length); }
            private set { _path = Encoding.GetBytes(value); }
        }

        public Encoding Encoding { get; private set; }

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
                capacity += 4; // Path length
                capacity += _path.Length; // Path
                return capacity;
            }
        }

        public SftpOpenDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            Path = path;

            _handleAction = handleAction;
        }

        protected override void LoadData()
        {
            base.LoadData();

            _path = ReadBinary();
        }

        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_path);
        }

        public override void Complete(SftpResponse response)
        {
            var handleResponse = response as SftpHandleResponse;
            if (handleResponse != null)
            {
                _handleAction(handleResponse);
            }
            else
            {
                base.Complete(response);
            }
        }
    }
}
