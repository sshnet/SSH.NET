using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Renci.SshNet.Common;

namespace Renci.SshNet.NetConf
{
    /// <summary>
    /// Represents a <c>NETCONF</c> session.
    /// </summary>
    internal sealed class NetConfSession : SubsystemSession, INetConfSession
    {
        private const string Prompt = "]]>]]>";

        private readonly StringBuilder _data = new StringBuilder();
        private bool _usingFramingProtocol;
        private EventWaitHandle _serverCapabilitiesConfirmed = new AutoResetEvent(initialState: false);
        private EventWaitHandle _rpcReplyReceived = new AutoResetEvent(initialState: false);
        private StringBuilder _rpcReply = new StringBuilder();
        private int _messageId;

        /// <summary>
        /// Gets the <c>NETCONF</c> server capabilities.
        /// </summary>
        /// <value>
        /// The <c>NETCONF</c> server capabilities.
        /// </value>
        public XmlDocument ServerCapabilities { get; private set; }

        /// <summary>
        /// Gets the <c>NETCONF</c> client capabilities.
        /// </summary>
        /// <value>
        /// The <c>NETCONF</c> client capabilities.
        /// </value>
        public XmlDocument ClientCapabilities { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfSession"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for an operation to complete, or <c>-1</c> to wait indefinitely.</param>
        public NetConfSession(ISession session, int operationTimeout)
            : base(session, "netconf", operationTimeout)
        {
            ClientCapabilities = new XmlDocument();
            ClientCapabilities.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                                "<hello xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\">" +
                                                    "<capabilities>" +
                                                        "<capability>" +
                                                            "urn:ietf:params:netconf:base:1.0" +
                                                        "</capability>" +
                                                    "</capabilities>" +
                                                "</hello>");
        }

        /// <summary>
        /// Sends the specified RPC request and returns the reply sent by the <c>NETCONF</c> server.
        /// </summary>
        /// <param name="rpc">The RPC request.</param>
        /// <param name="automaticMessageIdHandling"><see langword="true"/> to automatically increment the message id and verify the message id of the RPC reply.</param>
        /// <returns>
        /// The RPC reply.
        /// </returns>
        /// <exception cref="NetConfServerException"><paramref name="automaticMessageIdHandling"/> is <see langword="true"/> and the message id in the RPC reply does not match the message id of the RPC request.</exception>
        public XmlDocument SendReceiveRpc(XmlDocument rpc, bool automaticMessageIdHandling)
        {
            _ = _data.Clear();

            XmlNamespaceManager nsMgr = null;
            if (automaticMessageIdHandling)
            {
                _messageId++;
                nsMgr = new XmlNamespaceManager(rpc.NameTable);
                nsMgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");
                rpc.SelectSingleNode("/nc:rpc/@message-id", nsMgr).Value = _messageId.ToString(CultureInfo.InvariantCulture);
            }

            _rpcReply = new StringBuilder();
            _ = _rpcReplyReceived.Reset();
            var reply = new XmlDocument();
            if (_usingFramingProtocol)
            {
                var command = new StringBuilder(rpc.InnerXml.Length + 10);
                _ = command.AppendFormat(CultureInfo.InvariantCulture, "\n#{0}\n", rpc.InnerXml.Length);
                _ = command.Append(rpc.InnerXml);
                _ = command.Append("\n##\n");
                SendData(Encoding.UTF8.GetBytes(command.ToString()));

                WaitOnHandle(_rpcReplyReceived, OperationTimeout);
                reply.LoadXml(_rpcReply.ToString());
            }
            else
            {
                SendData(Encoding.UTF8.GetBytes(rpc.InnerXml + Prompt));
                WaitOnHandle(_rpcReplyReceived, OperationTimeout);
                reply.LoadXml(_rpcReply.ToString());
            }

            if (automaticMessageIdHandling)
            {
                var replyId = rpc.SelectSingleNode("/nc:rpc/@message-id", nsMgr).Value;
                if (replyId != _messageId.ToString(CultureInfo.InvariantCulture))
                {
                    throw new NetConfServerException("The rpc message id does not match the rpc-reply message id.");
                }
            }

            return reply;
        }

        protected override void OnChannelOpen()
        {
            _ = _data.Clear();

            var message = string.Concat(ClientCapabilities.InnerXml, Prompt);

            SendData(Encoding.UTF8.GetBytes(message));

            WaitOnHandle(_serverCapabilitiesConfirmed, OperationTimeout);
        }

        protected override void OnDataReceived(byte[] data)
        {
            var chunk = Encoding.UTF8.GetString(data);

            if (ServerCapabilities is null)
            {
                _ = _data.Append(chunk);

#if NET || NETSTANDARD2_1_OR_GREATER
                if (!chunk.Contains(Prompt, StringComparison.Ordinal))
#else
                if (!chunk.Contains(Prompt))
#endif // NET || NETSTANDARD2_1_OR_GREATER
                {
                    return;
                }

                try
                {
                    chunk = _data.ToString();
                    _ = _data.Clear();

                    ServerCapabilities = new XmlDocument();
#if NET || NETSTANDARD2_1_OR_GREATER
                    ServerCapabilities.LoadXml(chunk.Replace(Prompt, string.Empty, StringComparison.Ordinal));
#else
                    ServerCapabilities.LoadXml(chunk.Replace(Prompt, string.Empty));
#endif // NET || NETSTANDARD2_1_OR_GREATER
                }
                catch (XmlException e)
                {
                    throw new NetConfServerException("Server capabilities received are not well formed XML", e);
                }

                var nsMgr = new XmlNamespaceManager(ServerCapabilities.NameTable);
                nsMgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");

                _usingFramingProtocol = ServerCapabilities.SelectSingleNode("/nc:hello/nc:capabilities/nc:capability[text()='urn:ietf:params:netconf:base:1.1']", nsMgr) is not null;

                _ = _serverCapabilitiesConfirmed.Set();
            }
            else if (_usingFramingProtocol)
            {
                var position = 0;

                for (; ; )
                {
                    var match = Regex.Match(chunk.Substring(position), @"\n#(?<length>\d+)\n");
                    if (!match.Success)
                    {
                        break;
                    }

                    var fractionLength = Convert.ToInt32(match.Groups["length"].Value, CultureInfo.InvariantCulture);
                    _ = _rpcReply.Append(chunk, position + match.Index + match.Length, fractionLength);
                    position += match.Index + match.Length + fractionLength;
                }

#if NET7_0_OR_GREATER
                if (Regex.IsMatch(chunk.AsSpan(position), @"\n##\n"))
#else
                if (Regex.IsMatch(chunk.Substring(position), @"\n##\n"))
#endif // NET7_0_OR_GREATER
                {
                    _ = _rpcReplyReceived.Set();
                }
            }
            else
            {
                _ = _data.Append(chunk);

#if NET || NETSTANDARD2_1_OR_GREATER
                if (!chunk.Contains(Prompt, StringComparison.Ordinal))
#else
                if (!chunk.Contains(Prompt))
#endif // NET || NETSTANDARD2_1_OR_GREATER
                {
                    return;
                }

                chunk = _data.ToString();
                _ = _data.Clear();

#if NET || NETSTANDARD2_1_OR_GREATER
                _ = _rpcReply.Append(chunk.Replace(Prompt, string.Empty, StringComparison.Ordinal));
#else
                _ = _rpcReply.Append(chunk.Replace(Prompt, string.Empty));
#endif // NET || NETSTANDARD2_1_OR_GREATER
                _ = _rpcReplyReceived.Set();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_serverCapabilitiesConfirmed is not null)
                {
                    _serverCapabilitiesConfirmed.Dispose();
                    _serverCapabilitiesConfirmed = null;
                }

                if (_rpcReplyReceived is not null)
                {
                    _rpcReplyReceived.Dispose();
                    _rpcReplyReceived = null;
                }
            }
        }
    }
}
