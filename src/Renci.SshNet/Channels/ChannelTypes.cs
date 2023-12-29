namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Lists channel types as defined by the protocol.
    /// </summary>
    internal enum ChannelTypes
    {
        /// <summary>
        /// Session.
        /// </summary>
        Session,

        /// <summary>
        /// X11.
        /// </summary>
        X11,

        /// <summary>
        /// Forwarded-tcpip.
        /// </summary>
        ForwardedTcpip,

        /// <summary>
        /// Direct-tcpip.
        /// </summary>
        DirectTcpip
    }
}
