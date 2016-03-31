using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRemoveRequest : SftpRequest
    {
#if true //old TUNING
        private byte[] _fileName;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Remove; }
        }

#if true //old TUNING
        public string Filename
        {
            get { return Encoding.GetString(_fileName, 0, _fileName.Length); }
            private set { _fileName = Encoding.GetBytes(value); }
        }
#else
        public string Filename { get; private set; }
#endif

        public Encoding Encoding { get; private set; }

#if true //old TUNING
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
                capacity += 4; // FileName length
                capacity += _fileName.Length; // FileName
                return capacity;
            }
        }
#endif

        public SftpRemoveRequest(uint protocolVersion, uint requestId, string filename, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Encoding = encoding;
            this.Filename = filename;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if true //old TUNING
            _fileName = ReadBinary();
#else
            this.Filename = this.ReadString(this.Encoding);
#endif
        }

        protected override void SaveData()
        {
            base.SaveData();
#if true //old TUNING
            WriteBinaryString(_fileName);
#else
            this.Write(this.Filename, this.Encoding);
#endif
        }
    }
}
