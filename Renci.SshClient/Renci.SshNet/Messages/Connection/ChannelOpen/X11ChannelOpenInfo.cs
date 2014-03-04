namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "x11" channel type
    /// </summary>
    internal class X11ChannelOpenInfo : ChannelOpenInfo
    {
        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "x11";

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

            this.OriginatorAddress = this.ReadString();
            this.OriginatorPort = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.OriginatorAddress);
            this.Write(this.OriginatorPort);
        }
    }
}
