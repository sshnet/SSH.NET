﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when private key authentication method is used.
    /// </summary>
    public class PrivateKeyConnectionInfo : ConnectionInfo, IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Gets the key files used for authentication.
        /// </summary>
        public ICollection<IPrivateKeySource> KeyFiles { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="keyFiles">Connection key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, DefaultPort, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="keyFiles">Connection key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, params IPrivateKeySource[] keyFiles)
            : this(host, port, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, params IPrivateKeySource[] keyFiles)
            : this(host, port, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, params IPrivateKeySource[] keyFiles)
            : this(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, params IPrivateKeySource[] keyFiles)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, params IPrivateKeySource[] keyFiles)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, params IPrivateKeySource[] keyFiles)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, params IPrivateKeySource[] keyFiles)
            : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, new PrivateKeyAuthenticationMethod(username, keyFiles))
        {
            KeyFiles = new Collection<IPrivateKeySource>(keyFiles);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources.
                if (AuthenticationMethods != null)
                {
                    foreach (var authenticationMethod in AuthenticationMethods)
                    {
                        if (authenticationMethod is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        ~PrivateKeyConnectionInfo()
        {
            Dispose(disposing: false);
        }
    }
}
