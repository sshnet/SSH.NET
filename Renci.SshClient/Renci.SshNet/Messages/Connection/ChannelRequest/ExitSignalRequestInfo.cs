namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "exit-signal" type channel request information
    /// </summary>
    internal class ExitSignalRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "exit-signal";

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
        /// Gets a value indicating whether core is dumped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if core is dumped; otherwise, <c>false</c>.
        /// </value>
        public bool CoreDumped { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets message language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitSignalRequestInfo"/> class.
        /// </summary>
        public ExitSignalRequestInfo()
        {
            this.WantReply = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitSignalRequestInfo"/> class.
        /// </summary>
        /// <param name="signalName">Name of the signal.</param>
        /// <param name="coreDumped">if set to <c>true</c> then core is dumped.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="language">The language.</param>
        public ExitSignalRequestInfo(string signalName, bool coreDumped, string errorMessage, string language)
            : this()
        {
            this.SignalName = signalName;
            this.CoreDumped = coreDumped;
            this.ErrorMessage = errorMessage;
            this.Language = language;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.SignalName = this.ReadAsciiString();
            this.CoreDumped = this.ReadBoolean();
            this.ErrorMessage = this.ReadString();
            this.Language = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.WriteAscii(this.SignalName);
            this.Write(this.CoreDumped);
            this.Write(this.ErrorMessage);
            this.Write(this.Language);
        }

    }
}
