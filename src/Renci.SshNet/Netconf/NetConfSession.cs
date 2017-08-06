using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Renci.SshNet.Common;
using System.Xml;
using System.Text.RegularExpressions;

namespace Renci.SshNet.NetConf
{
    internal class NetConfSession : SubsystemSession, INetConfSession
    {
        private const string Prompt = "]]>]]>";

        private readonly StringBuilder _data = new StringBuilder();
        private bool _usingFramingProtocol;
        private EventWaitHandle _serverCapabilitiesConfirmed = new AutoResetEvent(false);
        private EventWaitHandle _rpcReplyReceived = new AutoResetEvent(false);
        private StringBuilder _rpcReply = new StringBuilder();
        private int _messageId;

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
            _data.Clear();

            XmlNamespaceManager nsMgr = null;
            if (automaticMessageIdHandling)
            {
                _messageId++;
                nsMgr = new XmlNamespaceManager(rpc.NameTable);
                nsMgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");
                rpc.SelectSingleNode("/nc:rpc/@message-id", nsMgr).Value = _messageId.ToString(CultureInfo.InvariantCulture);
            }
            _rpcReply = new StringBuilder();
            _rpcReplyReceived.Reset();
            var reply = new XmlDocument();
            if (_usingFramingProtocol)
            {
                var command = new StringBuilder(rpc.InnerXml.Length + 10);
                command.AppendFormat("\n#{0}\n", rpc.InnerXml.Length);
                command.Append(rpc.InnerXml);
                command.Append("\n##\n");
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
            _data.Clear();

            var message = string.Format("{0}{1}", ClientCapabilities.InnerXml, Prompt);

            SendData(Encoding.UTF8.GetBytes(message));

            WaitOnHandle(_serverCapabilitiesConfirmed, OperationTimeout);
        }

        protected override void OnDataReceived(byte[] data)
        {
            var chunk = Encoding.UTF8.GetString(data);

            if (ServerCapabilities == null)   // This must be server capabilities, old protocol
            {
                _data.Append(chunk);  

                if (!chunk.Contains(Prompt))
                {
                    return;
                }
                try
                {
                    chunk = _data.ToString(); 
                    _data.Clear();

                    ServerCapabilities = new XmlDocument();
                    ServerCapabilities.LoadXml(chunk.Replace(Prompt, ""));
                }
                catch (XmlException e)
                {
                    throw new NetConfServerException("Server capabilities received are not well formed XML", e);
                }

                var nsMgr = new XmlNamespaceManager(ServerCapabilities.NameTable);
                nsMgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");

                _usingFramingProtocol = (ServerCapabilities.SelectSingleNode("/nc:hello/nc:capabilities/nc:capability[text()='urn:ietf:params:netconf:base:1.1']", nsMgr) != null);

                _serverCapabilitiesConfirmed.Set();
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
                    var fractionLength = Convert.ToInt32(match.Groups["length"].Value);
                    _rpcReply.Append(chunk, position + match.Index + match.Length, fractionLength);
                    position += match.Index + match.Length + fractionLength;
                }
                if (Regex.IsMatch(chunk.Substring(position), @"\n##\n"))
                {
                    _rpcReplyReceived.Set();
                }
            }
            else  // Old protocol
            {
                _data.Append(chunk);

                if (!chunk.Contains(Prompt))
                {
                    return;
                    //throw new NetConfServerException("Server XML message does not end with the prompt " + _prompt);
                }
                
                chunk = _data.ToString();
                _data.Clear();

                _rpcReply.Append(chunk.Replace(Prompt, ""));
                _rpcReplyReceived.Set();
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
