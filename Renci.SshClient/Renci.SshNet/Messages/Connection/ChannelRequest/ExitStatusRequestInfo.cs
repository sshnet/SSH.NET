namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "exit-status" type channel request information
    /// </summary>
    internal class ExitStatusRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name.
        /// </summary>
        public const string NAME = "exit-status";

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
        /// Gets the exit status number.
        /// </summary>
        public uint ExitStatus { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitStatusRequestInfo"/> class.
        /// </summary>
        public ExitStatusRequestInfo()
        {
            this.WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitStatusRequestInfo"/> class.
        /// </summary>
        /// <param name="exitStatus">The exit status number.</param>
        public ExitStatusRequestInfo(uint exitStatus)
            : this()
        {
            this.ExitStatus = exitStatus;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.ExitStatus = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.ExitStatus);
        }
    }
}
