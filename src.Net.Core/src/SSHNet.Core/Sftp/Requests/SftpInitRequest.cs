﻿namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpInitRequest : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Init; }
        }

        public uint Version { get; private set; }

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
                capacity += 4; // Version
                return capacity;
            }
        }
#endif

        public SftpInitRequest(uint version)
        {
            this.Version = version;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Version = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Version);
        }
    }
}
