namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "eow@openssh.com" type channel request information
    /// </summary>
    public class EndOfWriteRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "eow@openssh.com";

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
        /// Initializes a new instance of the <see cref="EndOfWriteRequestInfo"/> class.
        /// </summary>
        public EndOfWriteRequestInfo()
        {
            this.WantReply = false;
        }
    }
}
