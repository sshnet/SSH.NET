#if NETFRAMEWORK && !NET20 && !NET35
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Renci.SshNet {
    /// <summary>
    /// Provides connection information when private key authentication method is used
    /// </summary>
    public class AgentConnectionInfo : ConnectionInfo, IDisposable {
        /// <summary>
        /// Gets the key files used for authentication.
        /// </summary>
        public IAgentProtocol Protocol { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="protocol">Connection key files.</param>
        public AgentConnectionInfo (string host, string username, IAgentProtocol protocol) : this (host, 22, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, protocol) {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="protocol">Connection key files.</param>
        public AgentConnectionInfo (string host, int port, string username, IAgentProtocol protocol) : this (host, port, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, protocol) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="protocol">The key files.</param>
        public AgentConnectionInfo (string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, IAgentProtocol protocol) : this (host, port, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, protocol) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="protocol">The key files.</param>
        public AgentConnectionInfo (string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, IAgentProtocol protocol) : this (host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, protocol) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="protocol">The key files.</param>
        public AgentConnectionInfo (string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, IAgentProtocol protocol) : this (host, 22, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, protocol) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="protocol">The key files.</param>
        public AgentConnectionInfo (string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, IAgentProtocol protocol) : this (host, 22, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, protocol) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="protocol">The key files.</param>
        public AgentConnectionInfo (string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, IAgentProtocol protocol) : this (host, 22, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, protocol) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="protocol">The key files.</param>
        public AgentConnectionInfo (string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, IAgentProtocol protocol) : base (host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, new AgentAuthenticationMethod (username, protocol)) {
            this.Protocol = protocol;
        }

        #region IDisposable Members

        private bool isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose () {
            Dispose (true);

            GC.SuppressFinalize (this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose (bool disposing) {
            // Check to see if Dispose has already been called.
            if (!this.isDisposed) {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing) {
                    // Dispose managed resources.
                    if (this.AuthenticationMethods != null) {
                        foreach (var authenticationMethods in this.AuthenticationMethods.OfType<IDisposable> ()) {
                            authenticationMethods.Dispose ();
                        }
                    }
                }

                // Note disposing has been done.
                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~AgentConnectionInfo () {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose (false);
        }

        #endregion
    }
}
#endif