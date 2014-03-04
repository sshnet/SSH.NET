namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "signal" type channel request information
    /// </summary>
    internal class SignalRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name.
        /// </summary>
        public const string NAME = "signal";

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
        /// Gets the name of the signal.
        /// </summary>
        /// <value>
        /// The name of the signal.
        /// </value>
        public string SignalName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRequestInfo"/> class.
        /// </summary>
        public SignalRequestInfo()
        {
            this.WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRequestInfo"/> class.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        public SignalRequestInfo(string signalName)
            : this()
        {
            this.SignalName = signalName;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.SignalName = this.ReadAsciiString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.WriteAscii(this.SignalName);
        }
    }
}
