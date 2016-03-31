﻿using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRealPathRequest : SftpRequest
    {
#if true //old TUNING
        private byte[] _path;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.RealPath; }
        }

#if true //old TUNING
        public string Path
        {
            get { return Encoding.GetString(_path, 0, _path.Length); }
            private set { _path = Encoding.GetBytes(value); }
        }
#else
        public string Path { get; private set; }
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
                capacity += 4; // Path length
                capacity += _path.Length; // Path
                return capacity;
            }
        }
#endif

        public SftpRealPathRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpNameResponse> nameAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            if (nameAction == null)
                throw new ArgumentNullException("nameAction");
            if (statusAction == null)
                throw new ArgumentNullException("statusAction");

            this.Encoding = encoding;
            this.Path = path;
            this.SetAction(nameAction);
            
        }

        protected override void SaveData()
        {
            base.SaveData();
#if true //old TUNING
            WriteBinaryString(_path);
#else
            this.Write(this.Path, this.Encoding);
#endif
        }
    }
}
