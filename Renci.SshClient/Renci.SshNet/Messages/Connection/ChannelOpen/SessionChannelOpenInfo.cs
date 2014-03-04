namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Used to open "session" channel type
    /// </summary>
    internal class SessionChannelOpenInfo : ChannelOpenInfo
    {
        /// <summary>
        /// Specifies channel open type
        /// </summary>
        public const string NAME = "session";

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
    }
}
