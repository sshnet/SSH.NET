using System;
using System.Text;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System.Xml;
using System.Text.RegularExpressions;

namespace Renci.SshNet.NetConf
{
    internal class NetConfSession : SubsystemSession
    {
        private readonly StringBuilder _data = new StringBuilder();

        private bool _usingFramingProtocol;

        private const string _prompt = "]]>]]>";

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
        /// <param name="operationTimeout">The operation timeout.</param>
        public NetConfSession(Session session, TimeSpan operationTimeout)
            : base(session, "netconf", operationTimeout, Encoding.UTF8)
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
            this._data.Clear();

            XmlNamespaceManager ns = null;
            if (automaticMessageIdHandling)
            {
                _messageId++;
                ns = new XmlNamespaceManager(rpc.NameTable);
                ns.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");
                rpc.SelectSingleNode("/nc:rpc/@message-id", ns).Value = _messageId.ToString();
            }
            _rpcReply = new StringBuilder();
            _rpcReplyReceived.Reset();
            var reply = new XmlDocument();
            if (_usingFramingProtocol)
            {
                StringBuilder command = new StringBuilder(rpc.InnerXml.Length + 10);
                command.AppendFormat("\n#{0}\n", rpc.InnerXml.Length);
                command.Append(rpc.InnerXml);
                command.Append("\n##\n");
                this.SendData(Encoding.UTF8.GetBytes(command.ToString()));

                this.WaitOnHandle(this._rpcReplyReceived, this._operationTimeout);
                reply.LoadXml(_rpcReply.ToString());
            }
            else
            {
                this.SendData(Encoding.UTF8.GetBytes(rpc.InnerXml + _prompt));
                this.WaitOnHandle(this._rpcReplyReceived, this._operationTimeout);
                reply.LoadXml(_rpcReply.ToString());
            }
            if (automaticMessageIdHandling)
            {
                //string reply_id = rpc.SelectSingleNode("/nc:rpc-reply/@message-id", ns).Value;
                string reply_id = rpc.SelectSingleNode("/nc:rpc/@message-id", ns).Value;
                if (reply_id != _messageId.ToString())
                {
                    throw new NetConfServerException("The rpc message id does not match the rpc-reply message id.");
                }
            }
            return reply;
        }

        protected override void OnChannelOpen()
        {
            this._data.Clear();

            string message = string.Format("{0}{1}", this.ClientCapabilities.InnerXml, _prompt);

            this.SendData(Encoding.UTF8.GetBytes(message));

            this.WaitOnHandle(this._serverCapabilitiesConfirmed, this._operationTimeout);
        }

        protected override void OnDataReceived(uint dataTypeCode, byte[] data)
        {
            string chunk = Encoding.UTF8.GetString(data);

            if (this.ServerCapabilities == null)   // This must be server capabilities, old protocol
            {
                this._data.Append(chunk);  

                if (!chunk.Contains(_prompt))
                {
                    return;
                }
                try
                {
                    chunk = this._data.ToString(); 
                    this._data.Clear();

                    this.ServerCapabilities = new XmlDocument();
                    this.ServerCapabilities.LoadXml(chunk.Replace(_prompt, ""));
                }
                catch (XmlException e)
                {
                    throw new NetConfServerException("Server capabilities received are not well formed XML", e);
                }

                XmlNamespaceManager ns = new XmlNamespaceManager(this.ServerCapabilities.NameTable);

                ns.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");

                this._usingFramingProtocol = (this.ServerCapabilities.SelectSingleNode("/nc:hello/nc:capabilities/nc:capability[text()='urn:ietf:params:netconf:base:1.1']", ns) != null);

                this._serverCapabilitiesConfirmed.Set();
            }
            else if (this._usingFramingProtocol)
            {
                int position = 0;

                for (; ; )
                {
                    Match match = Regex.Match(chunk.Substring(position), @"\n#(?<length>\d+)\n");
                    if (!match.Success)
                    {
                        break;
                    }
                    int fractionLength = Convert.ToInt32(match.Groups["length"].Value);
                    this._rpcReply.Append(chunk, position + match.Index + match.Length, fractionLength);
                    position += match.Index + match.Length + fractionLength;
                }
                if (Regex.IsMatch(chunk.Substring(position), @"\n##\n"))
                {
                    this._rpcReplyReceived.Set();
                }
            }
            else  // Old protocol
            {
                this._data.Append(chunk);

                if (!chunk.Contains(_prompt))
                {
                    return;
                    //throw new NetConfServerException("Server XML message does not end with the prompt " + _prompt);
                }
                
                chunk = this._data.ToString();
                this._data.Clear();

                this._rpcReply.Append(chunk.Replace(_prompt, ""));
                this._rpcReplyReceived.Set();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this._serverCapabilitiesConfirmed != null)
                {
                    this._serverCapabilitiesConfirmed.Dispose();
                    this._serverCapabilitiesConfirmed = null;
                }

                if (this._rpcReplyReceived != null)
                {
                    this._rpcReplyReceived.Dispose();
                    this._rpcReplyReceived = null;
                }
            }
        }
    }
}
