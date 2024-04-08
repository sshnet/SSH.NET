namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents an unknown request information that we can't handle.
    /// </summary>
    internal sealed class UnknownRequestInfo : RequestInfo
    {
        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        public override string RequestName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownRequestInfo"/> class.
        /// <paramref name="requestName">The name of the unknown request.</paramref>
        /// </summary>
        internal UnknownRequestInfo(string requestName)
        {
            RequestName = requestName;
        }
    }
}
