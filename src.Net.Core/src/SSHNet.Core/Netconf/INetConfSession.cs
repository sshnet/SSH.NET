using System.Xml;

namespace Renci.SshNet.NetConf
{
    internal interface INetConfSession : ISubsystemSession
    {
        /// <summary>
        /// Gets the NetConf server capabilities.
        /// </summary>
        /// <value>
        /// The NetConf server capabilities.
        /// </value>
        XmlDocument ServerCapabilities { get; }

        /// <summary>
        /// Gets the NetConf client capabilities.
        /// </summary>
        /// <value>
        /// The NetConf client capabilities.
        /// </value>
        XmlDocument ClientCapabilities { get; }

        XmlDocument SendReceiveRpc(XmlDocument rpc, bool automaticMessageIdHandling);
    }
}
