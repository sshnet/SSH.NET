#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Xml;

using Renci.SshNet.Common;
using Renci.SshNet.NetConf;

namespace Renci.SshNet
{
    /// <summary>
    /// Contains operation for working with NetConf server.
    /// </summary>
    public class NetConfClient : BaseClient
    {
        private int _operationTimeout;

        /// <summary>
        /// Holds <see cref="INetConfSession"/> instance that used to communicate to the server.
        /// </summary>
        private INetConfSession? _netConfSession;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite time-out period.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> represents a value that is less than -1 or greater than <see cref="int.MaxValue"/> milliseconds.</exception>
        public TimeSpan OperationTimeout
        {
            get
            {
                return TimeSpan.FromMilliseconds(_operationTimeout);
            }
            set
            {
                _operationTimeout = value.AsTimeout(nameof(OperationTimeout));
            }
        }

        /// <summary>
        /// Gets the current NetConf session.
        /// </summary>
        /// <value>
        /// The current NetConf session.
        /// </value>
        internal INetConfSession? NetConfSession
        {
            get { return _netConfSession; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        public NetConfClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, ownsConnectionInfo: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public NetConfClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), ownsConnectionInfo: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public NetConfClient(string host, int port, string username, params IPrivateKeySource[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), ownsConnectionInfo: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        public NetConfClient(string host, string username, params IPrivateKeySource[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <see langword="true"/>, then the
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <see langword="true"/>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal NetConfClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            _operationTimeout = Timeout.Infinite;
            AutomaticMessageIdHandling = true;
        }

        /// <summary>
        /// Gets the NetConf server capabilities.
        /// </summary>
        /// <value>
        /// The NetConf server capabilities.
        /// </value>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public XmlDocument ServerCapabilities
        {
            get
            {
                if (_netConfSession is null)
                {
                    throw new SshConnectionException("Client not connected.");
                }

                return _netConfSession.ServerCapabilities;
            }
        }

        /// <summary>
        /// Gets the NetConf client capabilities.
        /// </summary>
        /// <value>
        /// The NetConf client capabilities.
        /// </value>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public XmlDocument ClientCapabilities
{
            get
            {
                if (_netConfSession is null)
                {
                    throw new SshConnectionException("Client not connected.");
                }

                return _netConfSession.ClientCapabilities;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether automatic message id handling is
        /// enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if automatic message id handling is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        public bool AutomaticMessageIdHandling { get; set; }

        /// <summary>
        /// Sends the receive RPC.
        /// </summary>
        /// <param name="rpc">The RPC.</param>
        /// <returns>
        /// Reply message to RPC request.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public XmlDocument SendReceiveRpc(XmlDocument rpc)
        {
            if (_netConfSession is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            return _netConfSession.SendReceiveRpc(rpc, AutomaticMessageIdHandling);
        }

        /// <summary>
        /// Sends the receive RPC.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns>
        /// Reply message to RPC request.
        /// </returns>
        public XmlDocument SendReceiveRpc(string xml)
        {
            var rpc = new XmlDocument();
            rpc.LoadXml(xml);
            return SendReceiveRpc(rpc);
        }

        /// <summary>
        /// Sends the close RPC.
        /// </summary>
        /// <returns>
        /// Reply message to closing RPC request.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public XmlDocument SendCloseRpc()
        {
            if (_netConfSession is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

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

            _netConfSession?.Disconnect();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
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
