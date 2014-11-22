namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "forwarded-tcpip" channel type
    /// </summary>
    internal class ForwardedTcpipChannelInfo : ChannelOpenInfo
    {
        /// <summary>
        /// Initializes a new <see cref="ForwardedTcpipChannelInfo"/> instance.
        /// </summary>
        public ForwardedTcpipChannelInfo()
        {
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
        public string ConnectedAddress { get; private set; }

        /// <summary>
        /// Gets the connected port.
        /// </summary>
        public uint ConnectedPort { get; private set; }

        /// <summary>
        /// Gets the originator address.
        /// </summary>
        public string OriginatorAddress { get; private set; }

        /// <summary>
        /// Gets the originator port.
        /// </summary>
        public uint OriginatorPort { get; private set; }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            ConnectedAddress = ReadString();
            ConnectedPort = ReadUInt32();
            OriginatorAddress = ReadString();
            OriginatorPort = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(ConnectedAddress);
            Write(ConnectedPort);
            Write(OriginatorAddress);
            Write(OriginatorPort);
        }
    }
}
