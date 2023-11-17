using System.Xml;

using Renci.SshNet.Common;

namespace Renci.SshNet.NetConf
{
    /// <summary>
    /// Represents a <c>NETCONF</c> session.
    /// </summary>
    internal interface INetConfSession : ISubsystemSession
    {
        /// <summary>
        /// Gets the <c>NETCONF</c> server capabilities.
        /// </summary>
        /// <value>
        /// The <c>NETCONF</c> server capabilities.
        /// </value>
        XmlDocument ServerCapabilities { get; }

        /// <summary>
        /// Gets the <c>NETCONF</c> client capabilities.
        /// </summary>
        /// <value>
        /// The <c>NETCONF</c> client capabilities.
        /// </value>
        XmlDocument ClientCapabilities { get; }

        /// <summary>
        /// Sends the specified RPC request and returns the reply sent by the <c>NETCONF</c> server.
        /// </summary>
        /// <param name="rpc">The RPC request.</param>
        /// <param name="automaticMessageIdHandling"><see langword="true"/> to automatically increment the message id and verify the message id of the RPC reply.</param>
        /// <returns>
        /// The RPC reply.
        /// </returns>
        /// <exception cref="NetConfServerException"><paramref name="automaticMessageIdHandling"/> is <see langword="true"/> and the message id in the RPC reply does not match the message id of the RPC request.</exception>
        XmlDocument SendReceiveRpc(XmlDocument rpc, bool automaticMessageIdHandling);
    }
}
