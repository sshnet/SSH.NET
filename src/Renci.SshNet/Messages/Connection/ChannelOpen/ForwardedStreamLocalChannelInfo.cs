using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "forwarded-streamlocal@openssh.com" channel type
    /// </summary>
    internal class ForwardedStreamChannelInfo : ChannelOpenInfo
    {
        private byte[] _socketPath;
        private byte[] _reserved;

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "forwarded-streamlocal@openssh.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedTcpipChannelInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public ForwardedStreamChannelInfo(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new <see cref="ForwardedTcpipChannelInfo"/> instance with the specified connector
        /// address and port, and originator address and port.
        /// </summary>
        public ForwardedStreamChannelInfo(string socketPath, string reserved = "")
        {
            SocketPath = socketPath;
            Reserved = reserved;
        }

        /// <summary>
        /// Gets the type of the channel to open.
        /// </summary>
        /// <value>
        /// The type of the channel to open.
        /// </value>
        public override string ChannelType
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets the connected address.
        /// </summary>
        public string SocketPath
        {
            get { return Utf8.GetString(_socketPath, 0, _socketPath.Length); }
            private set { _socketPath = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
        public string Reserved
        {
            get { return Utf8.GetString(_reserved, 0, _reserved.Length); }
            private set { _reserved = Utf8.GetBytes(value); }
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
                capacity += 4; // ConnectedAddress length
                capacity += _socketPath.Length; // ConnectedAddress
                capacity += 4; // ConnectedPort
                capacity += 4; // Reserved length
                capacity += _reserved.Length; // Reserved
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _socketPath = ReadBinary();
            _reserved = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_socketPath);
            WriteBinaryString(_reserved);
        }
    }
}
