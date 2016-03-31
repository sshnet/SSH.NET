﻿using System;
using Renci.SshNet.Common;
using Renci.SshNet.NetConf;
using System.Xml;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    //  TODO:   Please help with documentation here, as I don't know the details, specially for the methods not documented.
    /// <summary>
    /// Contains operation for working with NetConf server.
    /// </summary>
    public class NetConfClient : BaseClient
    {
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
        public TimeSpan OperationTimeout { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        public NetConfClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public NetConfClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        public NetConfClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public NetConfClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        public NetConfClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is null.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal NetConfClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            OperationTimeout = new TimeSpan(0, 0, 0, 0, -1);
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

            _netConfSession = ServiceFactory.CreateNetConfSession(Session, OperationTimeout);
            _netConfSession.Connect();
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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
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
    }
}
