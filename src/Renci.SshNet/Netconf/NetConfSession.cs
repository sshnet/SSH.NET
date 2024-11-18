using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Renci.SshNet.Common;

namespace Renci.SshNet.NetConf
{
    internal sealed partial class NetConfSession : SubsystemSession, INetConfSession
    {
        private const string Prompt = "]]>]]>";
        private const string LengthPattern = @"\n#(?<length>\d+)\n";
        private const string ReplyPattern = @"\n##\n";
        private readonly StringBuilder _data = new StringBuilder();
        private bool _usingFramingProtocol;
        private EventWaitHandle _serverCapabilitiesConfirmed = new AutoResetEvent(initialState: false);
        private EventWaitHandle _rpcReplyReceived = new AutoResetEvent(initialState: false);
        private StringBuilder _rpcReply = new StringBuilder();
        private int _messageId;

#if NET7_0_OR_GREATER
        private static readonly Regex LengthRegex = GetLengthRegex();
        private static readonly Regex ReplyRegex = GetReplyRegex();

        [GeneratedRegex(LengthPattern)]
        private static partial Regex GetLengthRegex();

        [GeneratedRegex(ReplyPattern)]
        private static partial Regex GetReplyRegex();
#else
        private static readonly Regex LengthRegex = new Regex(LengthPattern, RegexOptions.Compiled);
        private static readonly Regex ReplyRegex = new Regex(ReplyPattern, RegexOptions.Compiled);
#endif

        /// <summary>
        /// Gets NetConf server capabilities.
        /// </summary>
        public XmlDocument ServerCapabilities { get; private set; }

        /// <summary>
        /// Gets NetConf client capabilities.
        /// </summary>
        public XmlDocument ClientCapabilities { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfSession"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="operationTimeout">The number of milliseconds to wait for an operation to complete, or -1 to wait indefinitely.</param>
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

                if (!chunk.Contains(Prompt))
                {
                    return;
                }

                try
                {
                    chunk = _data.ToString();
                    _ = _data.Clear();

                    ServerCapabilities = new XmlDocument();
                    ServerCapabilities.LoadXml(chunk.Replace(Prompt, string.Empty));
                }
                catch (XmlException e)
                {
                    throw new NetConfServerException("Server capabilities received are not well formed XML", e);
                }

                var nsMgr = new XmlNamespaceManager(ServerCapabilities.NameTable);
                nsMgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");

                const string xpath = "/nc:hello/nc:capabilities/nc:capability[text()='urn:ietf:params:netconf:base:1.1']";

                // Per RFC6242 section 4.1, If the :base:1.1 capability is advertised by both
                // peers, the chunked transfer mechanism is used for the remainder of the NETCONF
                // session. Otherwise, the old end-of-message based mechanism(see Section 4.3) is used.

                // This will currently evaluate to false since we (the client) do not advertise 1.1 capability.
                // Despite some code existing for the 1.1 framing protocol, it is thought to be incorrect or
                // incomplete. The NETCONF code is practically untested at the time of writing.
                _usingFramingProtocol = ServerCapabilities.SelectSingleNode(xpath, nsMgr) != null
                    && ClientCapabilities.SelectSingleNode(xpath, nsMgr) != null;

                _ = _serverCapabilitiesConfirmed.Set();
            }
            else if (_usingFramingProtocol)
            {
                var position = 0;

                for (; ; )
                {
                    var match = LengthRegex.Match(chunk.Substring(position));
                    if (!match.Success)
                    {
                        break;
                    }

                    var fractionLength = Convert.ToInt32(match.Groups["length"].Value, CultureInfo.InvariantCulture);
                    _ = _rpcReply.Append(chunk, position + match.Index + match.Length, fractionLength);
                    position += match.Index + match.Length + fractionLength;
                }

#if NET7_0_OR_GREATER
                if (ReplyRegex.IsMatch(chunk.AsSpan(position)))
#else
                if (ReplyRegex.IsMatch(chunk.Substring(position)))
#endif // NET7_0_OR_GREATER
                {
                    _ = _rpcReplyReceived.Set();
                }
            }
            else
            {
                _ = _data.Append(chunk);

                if (!chunk.Contains(Prompt))
                {
                    return;
                }

                chunk = _data.ToString();
                _ = _data.Clear();

                _ = _rpcReply.Append(chunk.Replace(Prompt, string.Empty));
                _ = _rpcReplyReceived.Set();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_serverCapabilitiesConfirmed != null)
                {
                    _serverCapabilitiesConfirmed.Dispose();
                    _serverCapabilitiesConfirmed = null;
                }

                if (_rpcReplyReceived != null)
                {
                    _rpcReplyReceived.Dispose();
                    _rpcReplyReceived = null;
                }
            }
        }
    }
}
