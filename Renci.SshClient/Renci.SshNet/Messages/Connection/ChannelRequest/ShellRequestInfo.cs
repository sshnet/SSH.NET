namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "shell" type channel request information
    /// </summary>
    internal class ShellRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "shell";

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
        /// Initializes a new instance of the <see cref="ShellRequestInfo"/> class.
        /// </summary>
        public ShellRequestInfo()
        {
            this.WantReply = true;
        }
    }
}
