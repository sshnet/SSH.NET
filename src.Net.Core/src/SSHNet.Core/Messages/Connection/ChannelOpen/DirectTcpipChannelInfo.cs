using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "direct-tcpip" channel type
    /// </summary>
    internal class DirectTcpipChannelInfo : ChannelOpenInfo
    {
#if true //old TUNING
        private byte[] _hostToConnect;
        private byte[] _originatorAddress;
#endif

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
#if true //old TUNING
        public string HostToConnect
        {
            get { return Utf8.GetString(_hostToConnect, 0, _hostToConnect.Length); }
            private set { _hostToConnect = Utf8.GetBytes(value); }
        }
#else
        public string HostToConnect { get; private set; }
#endif

        /// <summary>
        /// Gets the port to connect.
        /// </summary>
        public uint PortToConnect { get; private set; }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
#if true //old TUNING
        public string OriginatorAddress
        {
            get { return Utf8.GetString(_originatorAddress, 0, _originatorAddress.Length); }
            private set { _originatorAddress = Utf8.GetBytes(value); }
        }
#else
        public string OriginatorAddress { get; private set; }
#endif

        /// <summary>
        /// Gets the originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

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
                capacity += 4; // HostToConnect length
                capacity += _hostToConnect.Length; // HostToConnect
                capacity += 4; // PortToConnect
                capacity += 4; // OriginatorAddress length
                capacity += _originatorAddress.Length; // OriginatorAddress
                capacity += 4; // OriginatorPort
                return capacity;
            }
        }
#endif

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

#if true //old TUNING
            _hostToConnect = ReadBinary();
#else
            HostToConnect = ReadString();
#endif
            PortToConnect = ReadUInt32();
#if true //old TUNING
            _originatorAddress = ReadBinary();
#else
            OriginatorAddress = ReadString();
#endif
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if true //old TUNING
            WriteBinaryString(_hostToConnect);
#else
            Write(HostToConnect);
#endif
            Write(PortToConnect);
#if true //old TUNING
            WriteBinaryString(_originatorAddress);
#else
            Write(OriginatorAddress);
#endif
            Write(OriginatorPort);
        }
    }
}
