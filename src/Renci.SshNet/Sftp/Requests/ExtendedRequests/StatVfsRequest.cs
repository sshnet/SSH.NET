using System;
using Renci.SshNet.Sftp.Responses;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
    internal class StatVfsRequest : SftpExtendedRequest
    {
        private byte[] _path;
        private readonly Action<SftpExtendedReplyResponse> _extendedReplyAction;

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

        public StatVfsRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpExtendedReplyResponse> extendedAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction, "statvfs@openssh.com")
        {
            Encoding = encoding;
            Path = path;

            _extendedReplyAction = extendedAction;
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_path);
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
