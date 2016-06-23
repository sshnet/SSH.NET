
namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Lists channel types as defined by the protocol.
    /// </summary>
    internal enum ChannelTypes
    {
        /// <summary>
        /// session
        /// </summary>
        Session,
        /// <summary>
        /// x11
        /// </summary>
        X11,
        /// <summary>
        /// forwarded-tcpip
        /// </summary>
        ForwardedTcpip,
        /// <summary>
        /// direct-tcpip
        /// </summary>
        DirectTcpip
    }
}
