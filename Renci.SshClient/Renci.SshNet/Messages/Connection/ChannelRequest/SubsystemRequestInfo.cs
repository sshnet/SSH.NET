namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "subsystem" type channel request information
    /// </summary>
    internal class SubsystemRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "subsystem";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets the name of the subsystem.
        /// </summary>
        /// <value>
        /// The name of the subsystem.
        /// </value>
        public string SubsystemName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsystemRequestInfo"/> class.
        /// </summary>
        public SubsystemRequestInfo()
        {
            this.WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsystemRequestInfo"/> class.
        /// </summary>
        /// <param name="subsystem">The subsystem.</param>
        public SubsystemRequestInfo(string subsystem)
            : this()
        {
            this.SubsystemName = subsystem;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.SubsystemName = this.ReadAsciiString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.WriteAscii(this.SubsystemName);
        }
    }
}
