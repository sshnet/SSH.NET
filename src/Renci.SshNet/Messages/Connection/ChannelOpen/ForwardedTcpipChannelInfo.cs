using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "forwarded-tcpip" channel type
    /// </summary>
    internal class ForwardedTcpipChannelInfo : ChannelOpenInfo
    {
        private byte[] _connectedAddress;
        private byte[] _originatorAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedTcpipChannelInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public ForwardedTcpipChannelInfo(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new <see cref="ForwardedTcpipChannelInfo"/> instance with the specified connector
        /// address and port, and originator address and port.
        /// </summary>
        public ForwardedTcpipChannelInfo(string connectedAddress, uint connectedPort, string originatorAddress, uint originatorPort)
        {
            ConnectedAddress = connectedAddress;
            ConnectedPort = connectedPort;
            OriginatorAddress = originatorAddress;
            OriginatorPort = originatorPort;
        }

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "forwarded-tcpip";

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
        public string ConnectedAddress
        {
            get { return Utf8.GetString(_connectedAddress, 0, _connectedAddress.Length); }
            private set { _connectedAddress = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets the connected port.
        /// </summary>
        public uint ConnectedPort { get; private set; }

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
                capacity += 4; // ConnectedAddress length
                capacity += _connectedAddress.Length; // ConnectedAddress
                capacity += 4; // ConnectedPort
                capacity += 4; // OriginatorAddress length
                capacity += _originatorAddress.Length; // OriginatorAddress
                capacity += 4; // OriginatorPort
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _connectedAddress = ReadBinary();
            ConnectedPort = ReadUInt32();
            _originatorAddress = ReadBinary();
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_connectedAddress);
            Write(ConnectedPort);
            WriteBinaryString(_originatorAddress);
            Write(OriginatorPort);
        }
    }
}
