using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRmDirRequest : SftpRequest
    {
        private byte[] _path;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.RmDir; }
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

        public SftpRmDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            Path = path;
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
    }
}
