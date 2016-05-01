using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRemoveRequest : SftpRequest
    {
#if TUNING
        private byte[] _fileName;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Remove; }
        }

#if TUNING
        public string Filename
        {
            get { return Encoding.GetString(_fileName, 0, _fileName.Length); }
            private set { _fileName = Encoding.GetBytes(value); }
        }
#else
        public string Filename { get; private set; }
#endif

        public Encoding Encoding { get; private set; }

#if TUNING
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
            Encoding = encoding;
            Filename = filename;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            _fileName = ReadBinary();
#else
            Filename = ReadString(Encoding);
#endif
        }

        protected override void SaveData()
        {
            base.SaveData();
#if TUNING
            WriteBinaryString(_fileName);
#else
            Write(Filename, Encoding);
#endif
        }
    }
}
