using System;
using System.Text;

using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal sealed class SftpRealPathRequest : SftpRequest
    {
        private readonly Action<SftpNameResponse> _nameAction;
        private byte[] _path;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.RealPath; }
        }

        public string Path
        {
            get { return Encoding.GetString(_path, 0, _path.Length); }
            private set { _path = Encoding.GetBytes(value); }
        }

        public Encoding Encoding { get; }

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

        public SftpRealPathRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpNameResponse> nameAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            if (nameAction == null)
            {
                throw new ArgumentNullException(nameof(nameAction));
            }

            Encoding = encoding;
            Path = path;
            _nameAction = nameAction;
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_path);
        }

        public override void Complete(SftpResponse response)
        {
            if (response is SftpNameResponse nameResponse)
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
