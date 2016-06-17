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
        public const string Name = "shell";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return Name; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellRequestInfo"/> class.
        /// </summary>
        public ShellRequestInfo()
        {
            WantReply = true;
        }
    }
}
