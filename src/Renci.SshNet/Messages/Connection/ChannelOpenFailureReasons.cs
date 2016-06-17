namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// List channel open failure reasons defined by the protocol.
    /// </summary>
    internal enum ChannelOpenFailureReasons : uint
    {
        /// <summary>
        /// SSH_OPEN_ADMINISTRATIVELY_PROHIBITED
        /// </summary>
        AdministativelyProhibited = 1,
        /// <summary>
        /// SSH_OPEN_CONNECT_FAILED
        /// </summary>
        ConnectFailed = 2,
        /// <summary>
        /// SSH_OPEN_UNKNOWN_CHANNEL_TYPE
        /// </summary>
        UnknownChannelType = 3,
        /// <summary>
        /// SSH_OPEN_RESOURCE_SHORTAGE
        /// </summary>
        ResourceShortage = 4
    }
}
