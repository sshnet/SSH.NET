using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpLStatRequest : SftpRequest
    {
        private byte[] _path;
        private readonly Action<SftpAttrsResponse> _attrsAction;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.LStat; }
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

        public SftpLStatRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpAttrsResponse> attrsAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            Path = path;
            _attrsAction = attrsAction;
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
