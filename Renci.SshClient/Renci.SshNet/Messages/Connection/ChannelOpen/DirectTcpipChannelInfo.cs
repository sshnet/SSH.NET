namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "direct-tcpip" channel type
    /// </summary>
    internal class DirectTcpipChannelInfo : ChannelOpenInfo
    {
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
        public string HostToConnect { get; private set; }

        /// <summary>
        /// Gets the port to connect.
        /// </summary>
        public uint PortToConnect { get; private set; }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
        public string OriginatorAddress { get; private set; }

        /// <summary>
        /// Gets the originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectTcpipChannelInfo"/> class.
        /// </summary>
        public DirectTcpipChannelInfo()
        {

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
            this.HostToConnect = hostToConnect;
            this.PortToConnect = portToConnect;
            this.OriginatorAddress = originatorAddress;
            this.OriginatorPort = originatorPort;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.HostToConnect = this.ReadString();
            this.PortToConnect = this.ReadUInt32();
            this.OriginatorAddress = this.ReadString();
            this.OriginatorPort = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.HostToConnect);
            this.Write(this.PortToConnect);
            this.Write(this.OriginatorAddress);
            this.Write(this.OriginatorPort);
        }
    }
}
