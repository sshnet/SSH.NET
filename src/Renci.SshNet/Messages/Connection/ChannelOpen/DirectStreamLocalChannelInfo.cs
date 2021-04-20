using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "direct-streamlocal@openssh.com"" channel type
    /// </summary>
    internal class DirectStreamLocalChannelInfo : ChannelOpenInfo
    {
        private byte[] _socketPath;
        private byte[] _originatorAddress;

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "direct-streamlocal@openssh.com";

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
        /// Gets the host to connect.
        /// </summary>
        public string SocketPath
        {
            get { return Utf8.GetString(_socketPath, 0, _socketPath.Length); }
            private set { _socketPath = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
        public string OriginatorAddress
        {
            get { return Utf8.GetString(_originatorAddress, 0, _originatorAddress.Length); }
            private set { _originatorAddress = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets the originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

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
                capacity += 4; // SocketPath length
                capacity += _socketPath.Length; // SocketPath
                capacity += 4; // OriginatorAddress length
                capacity += _originatorAddress.Length; // OriginatorAddress
                capacity += 4; // OriginatorPort
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectStreamLocalChannelInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public DirectStreamLocalChannelInfo(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectStreamLocalChannelInfo"/> class
        /// </summary>
        /// <param name="socketPath"></param>
        /// <param name="originatorAddress"></param>
        /// <param name="originatorPort"></param>
        public DirectStreamLocalChannelInfo(string socketPath, string originatorAddress, uint originatorPort)
        {
            SocketPath = socketPath;
            OriginatorAddress = originatorAddress;
            OriginatorPort = originatorPort;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _socketPath = ReadBinary();
            _originatorAddress = ReadBinary();
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_socketPath);
            WriteBinaryString(_originatorAddress);
            Write(OriginatorPort);
        }
    }
}