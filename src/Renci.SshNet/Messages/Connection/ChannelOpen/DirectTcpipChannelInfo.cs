using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "direct-tcpip" channel type
    /// </summary>
    internal class DirectTcpipChannelInfo : ChannelOpenInfo
    {
        private byte[] _hostToConnect;
        private byte[] _originatorAddress;

        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "direct-tcpip";

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
        public string HostToConnect
        {
            get { return Utf8.GetString(_hostToConnect, 0, _hostToConnect.Length); }
            private set { _hostToConnect = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets the port to connect.
        /// </summary>
        public uint PortToConnect { get; private set; }

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
                capacity += 4; // HostToConnect length
                capacity += _hostToConnect.Length; // HostToConnect
                capacity += 4; // PortToConnect
                capacity += 4; // OriginatorAddress length
                capacity += _originatorAddress.Length; // OriginatorAddress
                capacity += 4; // OriginatorPort
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectTcpipChannelInfo"/> class from the
        /// specified data.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public DirectTcpipChannelInfo(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectTcpipChannelInfo"/> class.
        /// </summary>
        /// <param name="hostToConnect">The host to connect.</param>
        /// <param name="portToConnect">The port to connect.</param>
        /// <param name="originatorAddress">The originator address.</param>
        /// <param name="originatorPort">The originator port.</param>
        public DirectTcpipChannelInfo(string hostToConnect, uint portToConnect, string originatorAddress, uint originatorPort)
        {
            HostToConnect = hostToConnect;
            PortToConnect = portToConnect;
            OriginatorAddress = originatorAddress;
            OriginatorPort = originatorPort;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _hostToConnect = ReadBinary();
            PortToConnect = ReadUInt32();
            _originatorAddress = ReadBinary();
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_hostToConnect);
            Write(PortToConnect);
            WriteBinaryString(_originatorAddress);
            Write(OriginatorPort);
        }
    }
}
