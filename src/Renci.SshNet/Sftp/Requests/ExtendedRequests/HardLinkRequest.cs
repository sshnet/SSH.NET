using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class HardLinkRequest : SftpExtendedRequest
    {
        private byte[] _oldPath;
        private byte[] _newPath;

        public string OldPath
        {
            get { return Utf8.GetString(_oldPath, 0, _oldPath.Length); }
            private set { _oldPath = Utf8.GetBytes(value); }
        }

        public string NewPath
        {
            get { return Utf8.GetString(_newPath, 0, _newPath.Length); }
            private set { _newPath = Utf8.GetBytes(value); }
        }

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
                capacity += 4; // OldPath length
                capacity += _oldPath.Length; // OldPath
                capacity += 4; // NewPath length
                capacity += _newPath.Length; // NewPath
                return capacity;
            }
        }

        public HardLinkRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction, "hardlink@openssh.com")
        {
            OldPath = oldPath;
            NewPath = newPath;
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_oldPath);
            WriteBinaryString(_newPath);
        }
    }
}