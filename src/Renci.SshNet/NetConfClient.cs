using System;
using Renci.SshNet.Common;
using Renci.SshNet.NetConf;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Renci.SshNet
{
    //  TODO:   Please help with documentation here, as I don't know the details, specially for the methods not documented.
    /// <summary>
    /// Contains operation for working with NetConf server.
    /// </summary>
    public class NetConfClient : BaseClient
    {
        private int _operationTimeout;

        /// <summary>
        /// Holds <see cref="INetConfSession"/> instance that used to communicate to the server
        /// </summary>
        private INetConfSession _netConfSession;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite time-out period.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> represents a value that is less than -1 or greater than <see cref="Int32.MaxValue"/> milliseconds.</exception>
        public TimeSpan OperationTimeout {
            get { return TimeSpan.FromMilliseconds(_operationTimeout); }
            set
            {
                var timeoutInMilliseconds = value.TotalMilliseconds;
                if (timeoutInMilliseconds < -1d || timeoutInMilliseconds > int.MaxValue)
                    throw new ArgumentOutOfRangeException("value", "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");

                _operationTimeout = (int) timeoutInMilliseconds;
            }
        }

        /// <summary>
        /// Gets the current NetConf session.
        /// </summary>
        /// <value>
        /// The current NetConf session.
        /// </value>
        internal INetConfSession NetConfSession
        {
            get { return _netConfSession; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        public NetConfClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public NetConfClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        public NetConfClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public NetConfClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        public NetConfClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        private NetConfClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
            : this(connectionInfo, ownsConnectionInfo, new ServiceFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="serviceFactory">The factory to use for creating new services.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal NetConfClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            _operationTimeout = SshNet.Session.Infinite;
            AutomaticMessageIdHandling = true;
        }

        #endregion

        /// <summary>
        /// Gets the NetConf server capabilities.
        /// </summary>
        /// <value>
        /// The NetConf server capabilities.
        /// </value>
        public XmlDocument ServerCapabilities 
        {
            get { return _netConfSession.ServerCapabilities; }
        }

        /// <summary>
        /// Gets the NetConf client capabilities.
        /// </summary>
        /// <value>
        /// The NetConf client capabilities.
        /// </value>
        public XmlDocument ClientCapabilities
        {
            get { return _netConfSession.ClientCapabilities; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether automatic message id handling is
        /// enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if automatic message id handling is enabled; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        public bool AutomaticMessageIdHandling { get; set; }

        /// <summary>
        /// Sends the receive RPC.
        /// </summary>
        /// <param name="rpc">The RPC.</param>
        /// <returns>Reply message to RPC request</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public XmlDocument SendReceiveRpc(XmlDocument rpc)
        {
            return _netConfSession.SendReceiveRpc(rpc, AutomaticMessageIdHandling);
        }

        /// <summary>
        /// Sends the receive RPC.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns>Reply message to RPC request</returns>
        public XmlDocument SendReceiveRpc(string xml)
        {
            var rpc = new XmlDocument();
            rpc.LoadXml(xml);
            return SendReceiveRpc(rpc);
        }

        /// <summary>
        /// Sends the close RPC.
        /// </summary>
        /// <returns>Reply message to closing RPC request</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public XmlDocument SendCloseRpc()
        {
            var rpc = new XmlDocument();
            rpc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><rpc message-id=\"6666\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\"><close-session/></rpc>");
            return _netConfSession.SendReceiveRpc(rpc, AutomaticMessageIdHandling);
        }

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            _netConfSession = CreateAndConnectNetConfSession();
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            _netConfSession.Disconnect();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_netConfSession != null)
                {
                    _netConfSession.Dispose();
                    _netConfSession = null;
                }
            }
        }

        private INetConfSession CreateAndConnectNetConfSession()
        {
            var netConfSession = ServiceFactory.CreateNetConfSession(Session, _operationTimeout);
            try
            {
                netConfSession.Connect();
                return netConfSession;
            }
            catch
            {
                netConfSession.Dispose();
                throw;
            }
        }
    }
}
